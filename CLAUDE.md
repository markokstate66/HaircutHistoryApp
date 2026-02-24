# CLAUDE.md — Project Constitution

This file is read at the start of every Claude Code session. Follow all instructions here unless explicitly overridden in the session.

---

## Project Overview

**HaircutHistoryApp** — A cross-platform .NET MAUI mobile app that helps users document haircut recipes/templates, track haircut history, and share profiles with barbers/stylists via QR codes. Backed by Azure Functions API with Cosmos DB and Firebase Authentication.

---

## Architecture Decisions

| Decision | Choice | Reason |
|---|---|---|
| Mobile framework | .NET MAUI 10 (Android, iOS, Windows) | Cross-platform, C#/XAML, single codebase |
| UI pattern | MVVM with CommunityToolkit.Mvvm | Clean separation, observable properties, source generators |
| Backend API | Azure Functions v4 (.NET 8 Isolated Worker) | Serverless, cost-effective, decoupled |
| Hosting plan | Linux Consumption (never Windows) | No cold start penalty, Flex Consumption migration path |
| Database | Azure Cosmos DB (NoSQL) | Flexible schema, partitioned by OwnerUserId |
| Photo storage | Azure Blob Storage with SAS URLs | Secure, scalable, signed access |
| Authentication | Firebase Auth with Google Sign-In | Cross-platform, native SDKs, proven |
| Local cache | SQLite (sqlite-net-pcl) | Offline-first, bidirectional sync |
| QR sharing | ZXing.Net.Maui | Generation and scanning in one library |
| Monetization | Free/Premium tiers with Plugin.InAppBilling + AdMob | Limits on free tier, ad-free premium |
| HTTP client | IHttpClientFactory | Avoids socket exhaustion, enables DI |
| Logging | Built-in ILogger + Application Insights | No third-party logging frameworks |

---

## Data Model

**Hierarchy:** User → Profile (haircut template with measurements) → HaircutRecord (individual haircut log)

- **Profile** = a reusable haircut recipe (e.g., "Dad's winter haircut") with ordered measurement steps
- **HaircutRecord** = a logged instance of getting that haircut (date, stylist, location, price, photos)
- **ProfileShare** = grants a stylist read access to a profile via QR code (24-hour tokens)

### Free Tier Limits
- Max 1 profile
- Max 3 haircuts per profile
- Ads enabled

### Premium
- Unlimited profiles and haircuts
- Ad-free

---

## Patterns — Always Follow

- MVVM everywhere — ViewModels inherit `BaseViewModel`, use `[ObservableProperty]` and `[RelayCommand]`
- All services registered via DI in `MauiProgram.cs` (mobile) and `Program.cs` (API)
- All HTTP clients via `IHttpClientFactory` — never instantiate `HttpClient` directly
- All methods touching network or DB must be `async`/`await` — no `.Result` or `.Wait()`
- Config from `IConfiguration` / environment variables — never hardcode secrets
- Cosmos DB uses soft deletes (`IsDeleted` flag) — never hard-delete documents
- Content hashing for sync comparison between SQLite and Cosmos DB
- API responses use `ApiResponse<T>` wrapper pattern
- Firebase JWT validated in `AuthMiddleware` on every authenticated endpoint
- Platform-specific code behind `#if ANDROID` / `#if IOS` guards or via `INativeAuthService`

---

## What to Avoid

- Do **not** deploy Azure Functions to Windows — always Linux Consumption
- Do **not** hardcode API keys, connection strings, or secrets anywhere in code
- Do **not** use in-process Azure Functions model — always isolated worker
- Do **not** add third-party logging frameworks (Serilog, NLog, etc.)
- Do **not** use `HttpClient` directly outside of IHttpClientFactory
- Do **not** hard-delete Cosmos DB documents — use soft delete with `IsDeleted`
- Do **not** add unnecessary NuGet packages — keep dependencies minimal
- Do **not** swallow exceptions silently — always log and return meaningful errors

---

## Tech Stack

### Mobile App
- .NET MAUI 10 — `net10.0-android`, `net10.0-ios`, `net10.0-windows10.0.19041.0`
- C# 12 / XAML
- CommunityToolkit.Mvvm 8.4.0
- CommunityToolkit.Maui 13.0.0
- ZXing.Net.Maui.Controls 0.7.0
- sqlite-net-pcl 1.9.172
- Azure.Storage.Blobs 12.19.1
- Plugin.InAppBilling 7.1.0 (Android/iOS only)
- Plugin.MauiMTAdmob 1.6.3 (Android/iOS only)
- Newtonsoft.Json 13.0.4

### Backend API
- .NET 8 Isolated Worker Azure Functions v4
- Microsoft.Azure.Cosmos 3.39.1
- Azure.Storage.Blobs 12.19.1
- Microsoft.Identity.Web 2.17.4
- Application Insights

### Cloud Services
- Firebase Authentication (Google Sign-In)
- Azure Cosmos DB (containers: Users, Profiles, HaircutRecords, ProfileShares)
- Azure Blob Storage (photos, avatars)
- Azure Functions (Linux Consumption plan)

---

## API Routes

All routes under `/api/` prefix (configured in host.json).

| Method | Route | Auth | Purpose |
|---|---|---|---|
| GET | `/api/health` | No | Health check |
| GET | `/api/profiles` | Yes | List user's profiles |
| GET | `/api/profiles/{id}` | Yes | Get single profile |
| POST | `/api/profiles` | Yes | Create profile |
| PUT | `/api/profiles/{id}` | Yes | Update profile |
| DELETE | `/api/profiles/{id}` | Yes | Soft delete profile |
| GET | `/api/profiles/shared` | Yes | Profiles shared with user |
| GET | `/api/profiles/sync` | Yes | Sync info for profiles |
| POST | `/api/profiles/batch` | Yes | Batch get profiles |
| GET | `/api/profiles/{profileId}/haircuts` | Yes | List haircuts |
| POST | `/api/profiles/{profileId}/haircuts` | Yes | Create haircut record |
| PUT | `/api/profiles/{profileId}/haircuts/{id}` | Yes | Update haircut |
| DELETE | `/api/profiles/{profileId}/haircuts/{id}` | Yes | Soft delete haircut |
| POST | `/api/profiles/{id}/share` | Yes | Generate share token |
| POST | `/api/share/accept` | Yes | Accept shared profile |
| DELETE | `/api/profiles/{id}/share/{stylistId}` | Yes | Revoke share |
| POST | `/api/photos/upload-url` | Yes | Get SAS upload URL |
| DELETE | `/api/photos` | Yes | Delete photo |

---

## Session Workflow

**At the start of every session:**
1. Read `WORKLOG.md` to understand what was last worked on and what is pending
2. Note any unfinished work from the previous session before starting new work

**At the end of every session:**
1. Append a new dated entry to `WORKLOG.md` (see format in that file)
2. Mark completed items, note anything left unfinished, and record any decisions made

**When work is ready for release:**
1. Update `CHANGELOG.md` under the `[Unreleased]` section with what changed
2. On actual release, move `[Unreleased]` items to a versioned section with the date

---

## Available CLI Tools

- **GitHub CLI** (`gh`) — available for repo, PR, and issue operations
- **Azure CLI** (`az`) — available for Azure resource management and deployment

---

## File Structure

```
HaircutHistoryApp/
├── CLAUDE.md                           ← This file
├── CHANGELOG.md                        ← Release history
├── WORKLOG.md                          ← Session work log
├── PLAN.md                             ← Original implementation plan
├── README.md                           ← Project readme
├── HaircutHistoryApp.sln
├── docs/                               ← Setup guides, legal, marketing
├── src/
│   ├── HaircutHistoryApp/              ← .NET MAUI mobile app
│   │   ├── App.xaml(.cs)
│   │   ├── MauiProgram.cs              ← DI container setup
│   │   ├── AppShell.xaml(.cs)          ← Shell navigation
│   │   ├── Models/                     ← Local observable models
│   │   ├── ViewModels/                 ← MVVM ViewModels
│   │   ├── Views/                      ← XAML pages
│   │   ├── Services/                   ← Auth, data, sync, ads, etc.
│   │   ├── Converters/                 ← XAML value converters
│   │   ├── Resources/                  ← Styles, fonts, images, icons
│   │   └── Platforms/                  ← Android/iOS/Windows specifics
│   ├── HaircutHistoryApp.Api/          ← Azure Functions backend
│   │   ├── Program.cs                  ← DI and Cosmos/Blob config
│   │   ├── Functions/                  ← HTTP trigger functions
│   │   ├── Services/                   ← CosmosDB, Blob services
│   │   ├── Middleware/                 ← Auth middleware (Firebase JWT)
│   │   └── host.json                   ← Functions runtime config
│   └── HaircutHistoryApp.Shared/       ← Shared models and DTOs
│       ├── Models/                     ← User, Profile, HaircutRecord, etc.
│       └── DTOs/                       ← API request/response DTOs
└── tests/
    └── HaircutHistoryApp.Tests/        ← Unit tests
```
