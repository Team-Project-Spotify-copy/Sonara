# Sonara — project state

Last updated: 2026-09-06 · branch `integration/recovery`

## Environment

| Piece | State |
|---|---|
| Neon PostgreSQL | Working. Connection string in .NET User Secrets. Endpoint id must be `ep-soft-sea-aww0rea1-pooler` — an earlier value was off by one character and Neon reports that as `28P01 password authentication failed`, which reads like a bad password. |
| Azure Blob | Working (`sonarastorage`, containers `audio` + `images`, SAS generation available). |
| JWT | Working. `sub`, `email`, role, `iss=SonaraApi`, `aud=SonaraClients`. |
| reCAPTCHA v3 | Working. Server secret valid. In Development the literal token `dev-dummy-token` bypasses verification (`RecaptchaServices.DevBypassToken`). |
| Ethereum / Hardhat | Working. `npx hardhat node` + `npx hardhat run scripts/deploy.js --network localhost` deploys deterministically to the configured `Ethereum:ContractAddress`. |
| Redis | **Not installed.** Not needed to start. While down: anonymous search, popular tracks/albums, and the entire password-reset flow return 500 (`RedisCacheService` has no error handling). |

Setup steps are in [`DEVELOPMENT.md`](../DEVELOPMENT.md).

## Auth — current state

### Backend (`Backend/`)

Implemented and compiling; see "Blockers" for why runtime verification is partial.

- `POST /api/auth/register` — reCAPTCHA → validation → BCrypt → default role + Free plan → **persists the refresh token and sets the HttpOnly cookie** (previously it minted a refresh token, dropped it, and set no cookie).
- `POST /api/auth/login` — verified end to end. Returns `{userId, accessToken}` + rotating refresh cookie (HttpOnly, Secure, SameSite=None).
- `POST /api/auth/refresh` — verified: rotates the token and revokes the old one; replaying the old cookie returns 401.
- `POST /api/auth/logout` — **new**. `LogoutCommand` revokes the presented refresh token and clears the cookie. Idempotent. Not yet runtime-verified.
- `POST /api/auth/forgot-password` · `verify-reset-code` · `reset-password` — pre-existing, now reachable from the UI. All three depend on Redis.
- `GET /api/profile` — current-user endpoint used for session restore.

Register validation now rejects a malformed email, a blank username, and a password under 8 characters (`ValidationException` → 400 with per-field `errors`). A duplicate email is a `ConflictException` → **409** (it was an unmapped `InvalidOperationException` → 500).

### Frontend (`Frontend/src/`)

- `api/auth.service.js` — **new**. Single auth surface over the shared `api` client, so every call gets the bearer interceptor, `withCredentials`, and single-flight refresh. `AuthError` normalises `{code, message, errors}` into a display string.
- `contexts/account.context.jsx` — rewritten. Token-based (`status: loading | authenticated | anonymous`), restores the session from `/api/profile` on mount, listens for `auth:expired`. Still exposes `userId` / `accessToken` for existing consumers (`SubscriptionPage`, `user/Account`).
- `api/client.js` — now exports `setAccessToken` and dispatches `auth:expired` when refresh fails (previously it cleared the token silently and left the user on a dead page).
- `components/auth/RequireAuth.jsx` — **new** route guard. Waits for session restore before deciding, so a refresh does not bounce an authenticated user to `/login`.
- `App.jsx` — `/library`, `/account`, `/account/:username`, `/subscriptions` are guarded. `/forgot-password` is registered (it was linked from two screens but had no route).
- `Login.jsx` / `Register.jsx` — use the auth service and context instead of raw `axios`; loading, disabled and error states; client validation mirroring the server. Register previously read `response.data.AccessToken` (PascalCase → `undefined`) and stored nothing, so a registered user appeared signed in with no token.
- `ForgotPassword.jsx` — **new**. `ResetPassword.jsx` and `CreateNewPassword.jsx` now call the API instead of `console.log`ing; both bounce back if the flow state is missing.
- `layout/TopBar.jsx` — the previously inert avatar is now an account menu with **Log out**.

### Verified end to end (browser, real Neon)

Login → token stored → profile hydrated → redirect back to the originally requested route · session survives a full page reload · guards redirect anonymous users · logout clears storage and redirects · error and loading states render · no JavaScript console errors.

## Blockers

1. **Smart App Control is enforced on this machine** (`VerifiedAndReputablePolicyState = 1`). It blocks any newly built unsigned assembly, so the modified `Application.dll` cannot load: `FileLoadException … An Application Control policy has blocked this file (0x800711C7)`, CodeIntegrity events 3033/3077. Consequences: the modified backend cannot run, and `dotnet test` cannot even discover tests. The unmodified backend runs because its assembly hash was already approved. Turning SAC off is irreversible and is the machine owner's decision.
2. **Backend test suite: 80 failed / 39 passed (119)** — pre-existing, unrelated to auth. Two causes: a circular FK in `SeedDataExtension` (`User[0].ActiveSubscriptionId` ⇄ `UserSubscription.OwnerId`) breaks `EnsureCreated()`, and `TestData.SeededFreeSubscriptionId` is a `SubscriptionPlan` id assigned to `User.ActiveSubscriptionId`, which FKs to `UserSubscription` — fixture drift from PR #21.
3. **`john_doe` cannot be used for login.** Its stored hash is valid BCrypt, but neither supplied password matches. Verification used a throwaway account instead.
4. **Figma design not applied.** No Figma file key exists anywhere in the repo and every Figma tool requires one, so the new design could not be read. Auth UI keeps the existing layout and CSS; only functional states were added.
5. **Redis absent** — blocks the password-reset flow at runtime.

## Out of scope / untouched

Home feed (`/api/feed` does not exist on this branch), Search, PlayerBar, playback page, the Redis resilience gap, and the legacy test fixtures.
