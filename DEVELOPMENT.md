# Sonara — local development setup

Everything below is required to get `Backend` running and `Frontend` building from a
clean clone. Two of these steps are load-bearing: without them the API will not start
and the SPA will not build.

## 1. Prerequisites

| Tool | Version used |
|---|---|
| .NET SDK | 10.0.x |
| Node.js | 22.x |
| PostgreSQL | Neon (hosted) — no local install needed |
| Redis | optional, see §5 |

## 2. Backend secrets (.NET User Secrets)

Secrets are **never** committed. `Backend/Project/WebApp.csproj` declares a
`UserSecretsId`, so all server-side configuration goes through User Secrets.

Required keys — ask a maintainer for the values:

```
ConnectionStrings:DefaultConnection    Neon PostgreSQL connection string
ConnectionStrings:AzureBlobStorage     Azure Storage account connection string
Jwt:Key                                HMAC signing key (required; app throws at startup if absent)
Recaptcha:SecretKey                    Google reCAPTCHA v3 server secret
Ethereum:Url                           Local Hardhat JSON-RPC endpoint
Ethereum:ContractAddress               Address of the deployed premiumSubscription contract
```

Set them all at once without putting values on a command line:

```bash
cat secrets.json | dotnet user-secrets set --project Backend/Project/WebApp.csproj
```

where `secrets.json` is a **temporary, untracked** file of the form
`{ "ConnectionStrings:DefaultConnection": "...", ... }`. Delete it afterwards.

List the keys you currently have (values redacted):

```bash
dotnet user-secrets list --project Backend/Project/WebApp.csproj | sed 's/ = .*/ = <redacted>/'
```

> `Ethereum:Url` and `Ethereum:ContractAddress` are **not optional**.
> `BlockchainListenerService` is registered unconditionally in `Program.cs` and its
> constructor throws `ArgumentNullException` when either key is missing, which aborts
> host startup with `Hosting failed to start`.

## 3. Local blockchain node (required)

`Frontend/premium-subscription` is a Hardhat project. It is needed twice: it produces the
contract artifact the SPA imports, and it provides the RPC node the backend listens to.

```bash
cd Frontend/premium-subscription
npm ci
npx hardhat compile                                   # generates artifacts/ (gitignored)
npx hardhat node                                      # leave running on 127.0.0.1:8545
npx hardhat run scripts/deploy.js --network localhost # in a second shell
```

On a fresh chain the deploy is deterministic and prints the address that
`Ethereum:ContractAddress` expects. Re-run the deploy whenever you restart the node.

> `npx hardhat compile` is a **build prerequisite for the SPA**, not just for the chain.
> `Frontend/src/utilites/blockchainUtils.ts` statically imports
> `premium-subscription/artifacts/.../premiumSubscription.json`, and that path is
> gitignored. Skipping this step fails `vite build` and `vite dev` with
> `UNRESOLVED_IMPORT`, because the import chain reaches `main.jsx`.

## 4. Frontend

```bash
cd Frontend
npm ci        # or: npm install
npm run dev   # http://localhost:5173
```

Environment variables live in `Frontend/.env` / `.env.local` (both gitignored); see
`.env.example` for the full list. **Only client-safe values belong here.** Never put a
database password, an Azure account key, a JWT signing key, or the reCAPTCHA *server*
secret in a `VITE_*` variable — Vite inlines them into the public bundle.

`Cors:AllowedOrigins` in `appsettings.Development.json` must list the Vite origin, and the
CORS policy must keep `AllowCredentials()` for the refresh-token cookie.

## 5. Redis

`ConnectionStrings:Redis` defaults to `localhost:6379` in `appsettings.Development.json`.

Redis is **not** required for startup, but `RedisCacheService` has no error handling, so
while Redis is down these paths return 500:

- anonymous `GET /api/search`
- anonymous `GET /api/tracks/popular`, `GET /api/albums/popular`
- the whole password-reset flow (`forgot-password` / `verify-reset-code` /
  `reset-password`) — the code and reset token are stored only in the cache

Authenticated search and every other endpoint work without it.

## 6. Running

```bash
dotnet run --project Backend/Project/WebApp.csproj   # http://localhost:5094
```

In Development the API root redirects to the Scalar UI at `/scalar/v1`.
Migrations are **not** applied at startup — run `dotnet ef database update` explicitly.

## 7. reCAPTCHA in development

`/api/auth/register` and `/api/auth/login` are gated by reCAPTCHA v3. In the Development
environment `RecaptchaServices` accepts the literal token `dev-dummy-token` and skips
verification, so the auth flow can be exercised from curl or an automated browser (real
reCAPTCHA v3 scores automated browsers below the 0.5 threshold and would reject them).
This bypass is Development-only.
