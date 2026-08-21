# Sonara Player — Frontend Integration Contract

Source of truth for the frontend. Every route, field and status below was verified against the
running API (`dotnet run --project Project/WebApp.csproj`, `http://localhost:5094`).

- Base URL (dev): `http://localhost:5094`
- Live OpenAPI: `GET /openapi/v1.json` · Scalar UI: `/scalar/v1` (Development only)
- All JSON is **camelCase**. All enums serialize as **strings**.
- All timestamps are UTC ISO-8601 (`"2026-01-01T00:00:00Z"`).

## 1. Authentication

Send the access token from `POST /api/Auth/login` on every authenticated call:

```
Authorization: Bearer <accessToken>
```

The server derives the user **only** from the token. `X-User-Id` and any client-supplied id are
ignored. Refresh uses the `refreshToken` HttpOnly cookie, so send `credentials: "include"` on
`POST /api/Auth/refresh`.

| Access level | Routes |
|---|---|
| Anonymous OK | `GET /api/tracks`, `GET /api/tracks/{id}`, `POST /api/tracks/batch`, `GET /api/albums/{id}`, `GET /api/artists/{id}`, `GET /api/search`, `GET /api/playlists/{id}`, `GET /api/playlists/{id}/tracks` |
| Token required | everything under `/api/tracks/{id}/stream`, `/like`, `/listen`, `/api/tracks/liked`, `/api/history`, all playlist writes and `GET /api/playlists` |

On anonymous catalog calls `isLiked` is always `false`.

## 2. Error envelope

Every non-2xx response uses this exact shape:

```json
{
  "statusCode": 404,
  "message": "Entity \"Track\" with key (…) not found.",
  "code": "not_found",
  "errors": null,
  "traceId": "00-f7a5997b0394c4bd…-00"
}
```

`errors` is `null` except on `400`, where it is `{ "<Field>": ["<message>", …] }`.

| Status | `code` | Meaning |
|---|---|---|
| 400 | `validation_failed` | Model/business validation failed; read `errors` |
| 400 | `bad_request` | Malformed argument |
| 401 | `unauthorized` | Missing/expired/invalid token → refresh, then retry |
| 403 | `forbidden` | Not the owner, or playlist is private |
| 404 | `not_found` | Track / album / artist / playlist does not exist |
| 409 | `media_unavailable` | Track exists but has no playable audio — **not** a 404 |
| 409 | `conflict` | Resource state conflict |
| 502 | `storage_unavailable` | Media storage failed; safe to retry |
| 500 | `internal_error` | Generic message only; details stay server-side |

## 3. Track model

Returned by every catalog list. This is the object the player and queue hold.

```json
{
  "id": "99999999-9999-9999-9999-111111111111",
  "title": "Midnight City Drive",
  "artistId": "66666666-6666-6666-6666-111111111111",
  "artistName": "The Midnight Wave",
  "albumId": "77777777-7777-7777-7777-111111111111",
  "albumTitle": "Neon Nights",
  "artworkUrl": "https://…/images/albums/album_1.jpg",
  "durationMs": 210000,
  "durationSeconds": 210,
  "genres": ["Synthwave"],
  "playsCount": 12500,
  "hasStream": true,
  "isLiked": false,
  "createdAt": "2026-01-01T00:00:00Z"
}
```

| Field | Type | Notes |
|---|---|---|
| `id`, `artistId` | `string` (uuid) | never null |
| `albumId`, `albumTitle` | `string \| null` | null for singles |
| `artworkUrl` | `string \| null` | album cover, else artist avatar |
| `durationMs` | `number` (int) | authoritative duration |
| `durationSeconds` | `number` | derived convenience value, read-only |
| `genres` | `string[]` | possibly empty |
| `playsCount` | `number` (int64) | |
| `hasStream` | `boolean` | **if `false`, do not call `/stream`** — it returns 409 |
| `isLiked` | `boolean` | `false` when anonymous |

There is **no audio URL on this object by design.** Playback always goes through `/stream` (§7).

### Pagination envelope

Every paged endpoint returns:

```json
{
  "items": [ /* … */ ],
  "pageIndex": 1,
  "totalPages": 5,
  "totalCount": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

`pageSize` is clamped to `1…100`; `page` below 1 becomes 1.

## 4. Catalog

### `GET /api/tracks` — anonymous OK

Query: `page` (default 1), `pageSize` (default 20, max 100), `genre` (case-insensitive name),
`artistId`, `albumId`, `sort` = `Newest` (default) | `Popular` | `Title`.

→ `200` paginated `Track[]`.

### `POST /api/tracks/batch` — anonymous OK

Rehydrate a saved queue in one request instead of N calls.

```json
{ "ids": ["99999999-…-222222222222", "99999999-…-111111111111"] }
```

→ `200` `Track[]` — **in the same order as `ids`**; unknown ids are silently dropped.
→ `400` if `ids` is empty or longer than 200.

### `GET /api/albums/{id}` — anonymous OK

```json
{
  "id": "…", "title": "Deep Ocean", "coverUrl": "https://…", "type": "EP",
  "releaseDate": "2023-08-15T00:00:00Z",
  "artistId": "…", "artistName": "Alex Sound",
  "tracksCount": 1, "totalDurationMs": 340000,
  "tracks": [ /* Track[] */ ]
}
```
→ `404` if unknown.

### `GET /api/artists/{id}` — anonymous OK

```json
{
  "id": "…", "name": "Alex Sound", "avatarUrl": "https://…", "verified": true,
  "bio": "Electronic & Ambient music creator.",
  "albums": [ { "id": "…", "title": "…", "coverUrl": "…", "type": "EP",
                "releaseDate": "…", "artistId": "…", "artistName": "…", "tracksCount": 1 } ],
  "topTracks": [ /* Track[], max 10, by playsCount desc */ ]
}
```
→ `404` if unknown.

## 5. Track details

### `GET /api/tracks/{id}` — anonymous OK

Everything in **Track**, plus:

```json
{
  "artistAvatarUrl": "https://…/avatars/avatar_2.jpg",
  "artistVerified": true,
  "albumCoverUrl": "https://…/albums/album_2.jpg",
  "albumType": "EP",
  "albumReleaseDate": "2023-08-15T00:00:00Z",
  "likesCount": 1
}
```

| Field | Type |
|---|---|
| `artistAvatarUrl` | `string \| null` |
| `artistVerified` | `boolean` |
| `albumCoverUrl`, `albumType` | `string \| null` |
| `albumReleaseDate` | `string \| null` (ISO date-time) |
| `likesCount` | `number` (int64) |

→ `404` `not_found` if the track does not exist. A track with no audio still returns `200`
with `hasStream: false`.

## 6. Search

### `GET /api/search?q={query}&limit={n}` — anonymous OK

- Case-insensitive; the query is trimmed.
- Queries shorter than **2 characters** (and empty/missing `q`) return `200` with empty
  sections — **never an error**.
- `limit` is per section, clamped to `1…50` (default 10).
- Ordering: prefix matches first, then tracks by `playsCount`, artists by `verified`,
  albums by `releaseDate`, playlists by `createdAt`.
- **Private playlists never appear**, not even for their owner.

```json
{
  "query": "ab",
  "limit": 10,
  "tracks":    { "items": [ /* Track[] */ ], "total": 1 },
  "artists":   { "items": [ { "id": "…", "name": "…", "avatarUrl": "…", "verified": true } ], "total": 0 },
  "albums":    { "items": [ /* AlbumSummary[] */ ], "total": 0 },
  "playlists": { "items": [ { "id": "…", "name": "…", "description": "…", "coverUrl": "…",
                             "ownerId": "…", "ownerUsername": "…", "tracksCount": 1 } ], "total": 0 },
  "totalResults": 1
}
```

`total` is the full match count; `items` is capped at `limit`.

## 7. Stream

### `GET /api/tracks/{id}/stream` — **token required**

```json
{
  "trackId": "99999999-9999-9999-9999-222222222222",
  "url": "https://…/audio/tracks/tracks_2.mp3?sv=…&sig=…",
  "contentType": "audio/mpeg",
  "durationMs": 340000,
  "mode": "SignedUrl",
  "expiresAt": "2026-08-14T08:40:58Z",
  "supportsRangeRequests": true
}
```

| Field | Type | Notes |
|---|---|---|
| `url` | `string` | assign directly to `audio.src` |
| `contentType` | `string` | `audio/mpeg`, `audio/mp4`, `audio/ogg`, `audio/flac`, … |
| `mode` | `"SignedUrl" \| "DirectUrl"` | |
| `expiresAt` | `string \| null` | null when `mode === "DirectUrl"` |
| `supportsRangeRequests` | `boolean` | always `true` — seeking works natively |

**Player rules**

1. The URL is served by storage, so HTTP Range/seeking works with no extra headers.
2. Do **not** attach the `Authorization` header when fetching `url` — it is pre-authorized.
3. `expiresAt` is deliberately ~1 min earlier than the real expiry. Re-request `/stream`
   when a track starts, and never cache the URL past `expiresAt`.
4. The response is `Cache-Control: private, no-store` — never share it between users.

**Errors:** `401` no token · `404` unknown track · `409 media_unavailable` track has no audio
(check `hasStream` first) · `502 storage_unavailable` retry.

## 8. Favorites

Implemented on the existing `LikedTrack` domain. All routes require a token; the join table is
never exposed. Every response is the same `TrackLikeState` object:

```json
{
  "trackId": "…",
  "isLiked": true,
  "likesCount": 1,
  "likedAt": "2026-08-14T07:35:00Z"
}
```

`likedAt` is `null` when `isLiked` is `false`. `likesCount` is the global total.

| Method | Route | Notes |
|---|---|---|
| `GET` | `/api/tracks/{id}/like` | current state |
| `POST` | `/api/tracks/{id}/like` | **idempotent** — liking twice is a no-op |
| `DELETE` | `/api/tracks/{id}/like` | **idempotent** — unliking twice is a no-op |

All three return `200` with `TrackLikeState`, `401` without a token, `404` for an unknown track.

### `GET /api/tracks/liked` — token required

Query `page`, `pageSize`. → `200` paginated `Track[]`, newest like first, all `isLiked: true`.

## 9. Playback history

### `POST /api/tracks/{id}/listen` — token required

Call **once per play**, when the track finishes or the user skips away. **Not on progress ticks.**

```json
{ "durationListenedMs": 45000 }
```

→ `200`:

```json
{
  "trackId": "…",
  "status": "Recorded",
  "recorded": true,
  "playsCount": 1,
  "listenedAt": "2026-08-14T07:35:00Z",
  "requiredListenedMs": 30000
}
```

| `status` | Meaning |
|---|---|
| `"Recorded"` | Written to history, `playsCount` incremented |
| `"TooShort"` | Below `requiredListenedMs` — not counted |
| `"Throttled"` | Same user + track already recorded within 30 s |

The threshold is 30 000 ms, or half the track for tracks shorter than that.
`listenedAt` is `null` unless `status === "Recorded"`. Non-`Recorded` results are **normal**,
not errors — do not retry them.

**Errors:** `400` negative/oversized duration · `401` · `404` unknown track.

### `GET /api/history` — token required

Query `page`, `pageSize`. → `200` paginated, newest first:

```json
{
  "items": [
    {
      "id": "bbbbbbbb-…-111111111111",
      "listenedAt": "2026-08-14T07:35:00Z",
      "durationListenedMs": 60000,
      "track": { /* full Track object */ }
    }
  ],
  "pageIndex": 1, "totalPages": 1, "totalCount": 1,
  "hasPreviousPage": false, "hasNextPage": false
}
```

Only the caller's own history is ever returned.

## 10. Playlists

`Playlist` object:

```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-111111111111",
  "userId": "33333333-3333-3333-3333-111111111111",
  "ownerUsername": "john_doe",
  "name": "Night Vibes",
  "description": "Best tracks for late-night coding.",
  "isPrivate": false,
  "coverUrl": "https://…/playlists/playlists_1.jpg",
  "createdAt": "2026-01-01T00:00:00Z",
  "tracksCount": 1,
  "totalDurationMs": 210000,
  "isOwner": false
}
```

`description` and `coverUrl` may be `null`. Use `isOwner` to decide whether to show edit controls.

| Method | Route | Auth | Returns |
|---|---|---|---|
| `GET` | `/api/playlists` | token | `200` `Playlist[]` — caller's own |
| `GET` | `/api/playlists/{id}` | anonymous OK | `200` `Playlist` |
| `POST` | `/api/playlists` | token | `201` `Playlist` + `Location` |
| `PUT` | `/api/playlists/{id}` | owner | `200` `Playlist` |
| `DELETE` | `/api/playlists/{id}` | owner | `204` |
| `GET` | `/api/playlists/{id}/tracks` | anonymous OK | `200` `PlaylistTrack[]` |
| `POST` | `/api/playlists/{id}/tracks` | owner | `200` `Playlist` |
| `DELETE` | `/api/playlists/{id}/tracks/{trackId}` | owner | `200` `Playlist` |

### Create / update

`multipart/form-data` (cover upload). Fields: `Name` (required, 1–100 chars, trimmed),
`Description` (optional, ≤500, blank → `null`), `IsPrivate` (bool), `CoverImage` (optional file).
`CoverImage` is optional on create — a playlist can be made from a name alone.
→ `400 validation_failed` on a blank or overlong name.

### Read tracks

```json
[
  { "position": 0, "addedAt": "2026-01-01T00:00:00Z", "track": { /* full Track */ } },
  { "position": 1, "addedAt": "2026-01-02T00:00:00Z", "track": { /* full Track */ } }
]
```

`position` is a 0-based index in add order — feed this array straight into the queue.

### Add / remove tracks

`POST` body: `{ "trackId": "99999999-…-111111111111" }`.
Both add and remove are **idempotent** and return the updated `Playlist`, so `tracksCount` and
`totalDurationMs` can be applied without a refetch.
→ `400` on an empty/missing `trackId` · `404` unknown playlist **or** unknown track ·
`403` not the owner.

### Visibility

Private playlists return `403 forbidden` for anyone but the owner (including anonymous), on both
`GET /api/playlists/{id}` and `/tracks`. Non-existent ids return `404`.

## 11. Queue

The queue is **client-side only** — the backend intentionally persists no transient playback
state. Compose it from any `Track[]` (catalog, playlist, search, liked), persist the id list
locally, and rehydrate with `POST /api/tracks/batch`, which preserves your order.

## 12. SignalR

**Not implemented.** No hub is registered and SignalR is not referenced by the backend. Do not
build a realtime client against this API. The domain contains dormant `ListeningRoom` /
`RoomMember` entities, but no hub, endpoint, or event contract exists for them — shared listening
is a separate future feature.

## Integration checklist

1. Attach `Authorization: Bearer …` to all authenticated calls; on `401`, refresh once and retry.
2. Read `hasStream` before calling `/stream`; treat `409 media_unavailable` as "unplayable track".
3. Fetch `/stream` per track start; honour `expiresAt`; never cache or share the URL.
4. Call `/listen` once per play with the real listened duration — never per progress tick.
5. Drive like buttons from the returned `TrackLikeState`; both verbs are idempotent.
6. Treat a short/empty search as a valid empty result, not an error.
7. Read the `code` field, not the message text, when branching on errors.
