# Changelog

All notable changes to HaircutHistoryApp will be documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/): MAJOR.MINOR.PATCH

- **MAJOR** — breaking changes to data models, API contracts, or auth flow
- **MINOR** — new features, new screens, new API endpoints
- **PATCH** — bug fixes, config changes, dependency updates

---

## [Unreleased]

### Added
- Offline-first caching with SQLite and bidirectional sync service (`SqliteService`, `SyncService`, `CachedDataService`)
- Sync DTOs for cloud/local data comparison (`ProfileSyncInfo`, `SyncResponse`, `BatchFetchRequest`, etc.)
- Cache models (`CachedProfile`, `CachedHaircutRecord`, `PendingOperation`, `SyncStatus`)
- Sync API endpoints (batch fetch, sync info) in ProfileFunctions
- AdService full lifecycle management
- Cutting guide and theme selection navigation routes
- Rewritten test suite — 58 tests covering models, DTOs, ApiResponse, and sync types
- CI/CD: API deploy workflow (`.github/workflows/deploy-api.yml`) with automated health check smoke test
- CI/CD: `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` GitHub secret for automated API deployment

### Changed
- All ViewModels now use `BaseViewModel.ExecuteAsync` for consistent error handling
- Updated app icons and splash screen
- Updated platform configs (AndroidManifest, Info.plist)
- Test project retargeted to `net8.0` referencing `HaircutHistoryApp.Shared`
- CI workflow triggers on `master` branch (was `main`/`develop`)
- CI workflow uses .NET 8 for API/Shared/tests build, .NET 10 only for MAUI builds
- CI workflow now builds both Shared and API projects before running tests

### Fixed
- `ProfileListViewModel` was showing raw exception messages to users — now uses `AlertService` friendly messages
- Removed obsolete PlayFab error mapping from `AlertService`
- Added logging to previously silent catch in stats calculation
- Fixed test project broken reference (was `HaircutHistoryApp.Core`, now `HaircutHistoryApp.Shared`)
- CI workflow: fixed reference to deleted `HaircutHistoryApp.Core` project (now uses `HaircutHistoryApp.Shared`)

### Removed
- Old test files for non-existent models (`HaircutProfile`, `ShareSession`, `Achievement`)

---

## Release History

### [0.5.0] — 2026-02-02

#### Added
- Azure Functions API backend (.NET 8 Isolated Worker, Linux Consumption)
  - Profile CRUD endpoints (`/api/profiles`)
  - Haircut record endpoints (`/api/profiles/{id}/haircuts`)
  - Share endpoints (generate token, accept share, revoke)
  - Photo upload/delete via SAS URLs
  - Health check endpoint
- Azure Cosmos DB integration (Users, Profiles, HaircutRecords, ProfileShares containers)
- Azure Blob Storage for photo storage
- Firebase Authentication middleware (JWT validation with cached signing keys)
- Shared library (`HaircutHistoryApp.Shared`) for models and DTOs
- Measurement step ordering (`StepOrder`) for workflow sequencing
- Profile as haircut template model with reusable measurements
- Haircut record model for logging individual haircuts (date, stylist, location, price)
- Content hashing for sync comparison
- Soft deletes across all Cosmos DB documents

#### Changed
- Migrated backend from PlayFab to Azure Functions + Cosmos DB
- Restructured data model: Profile is now a haircut template (not an individual record)
- Authentication switched from PlayFab to Firebase with Google Sign-In

#### Removed
- PlayFab backend dependency
- Barber/Client dual-mode (simplified to single user mode with sharing)

---

### [0.4.0] — 2026-01-01

#### Changed
- Updated Google OAuth client IDs for all platforms (Android, iOS, Web)

---

### [0.3.0] — Prior releases (consolidated)

#### Added
- Google, Facebook, and Apple Sign-In support
- Dark mode and light/dark theme switching across all pages
- Cloud image storage and loading
- Profile thumbnails and avatar generation
- Windows platform support
- Subscription limits (free tier: 1 profile, 3 haircuts)
- In-app purchases with Plugin.InAppBilling
- AdMob ad integration for free tier
- Account deletion page
- Persistent login with device linking
- QR code generation and scanning for profile sharing
- Production features: SEO, profile pictures, security headers, logging
- Full-screen image viewer
- Achievements/statistics page
- Glossary of haircut terminology
- Step-by-step cutting guide from profile measurements

#### Fixed
- Measurement editing: made model observable with two-way bindings
- Light/dark theme support across all pages
- AddEditProfilePage theming and AdMob test app ID
- Premium page showing no options on Windows
- Build errors and code quality improvements
- PlayFab SDK v2 compatibility for account deletion
- Edit profile loading issues

---

### [0.1.0] — Initial

#### Added
- Initial .NET MAUI app scaffold (iOS, Android)
- Basic profile management (create, view, edit)
- MVVM architecture with CommunityToolkit.Mvvm
- Firebase authentication (email/password)
- QR code sharing concept
- Basic UI with indigo/purple color scheme

---

<!--
Release template — copy and fill in when cutting a release:

## [X.Y.Z] - YYYY-MM-DD

### Added
-

### Changed
-

### Fixed
-

### Removed
-
-->
