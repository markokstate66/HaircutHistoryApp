# Work Log

Running log of Claude Code sessions. Most recent entry at the top.
Each session appends a new entry using the template below.

---

## Session Log

---

### [SESSION-002] — 2026-02-24

**Goal:** Deploy latest API code to Azure, smoke test endpoints, fix broken CI/CD pipeline

**Completed:**
- Deployed all 22 Azure Functions to `haircuthistory-api` using `func azure functionapp publish` — includes new `GetProfileSync` and `GetProfilesBatch` endpoints
- Smoke tested all key endpoints: health check (200), auth-required (UNAUTHORIZED via ApiResponse wrapper), new sync/batch endpoints (working), non-existent routes (404), function count (22)
- Rewrote `.github/workflows/ci.yml`: fixed branch triggers (`main`/`develop` → `master`), fixed project refs (`Core` → `Shared`), added API build step, uses .NET 8
- Removed MAUI build-android and build-ios jobs from CI — user builds locally and uses Codemagic for iOS
- Created `.github/workflows/deploy-api.yml`: automated API deployment on push to master (when API/Shared files change), uses `azure/functions-action@v1` with publish profile, includes health check smoke test
- Stored `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` as GitHub secret (downloaded from Azure via `az` CLI, stored via `gh secret set`)
- Enabled SCM basic auth on Azure function app (was disabled, blocking publish profile deploys)
- CI `build-and-test` job passing (Shared + API build + 58 tests in ~50s)

**In Progress / Pending:**
- [ ] Test bidirectional sync between SQLite cache and Cosmos DB end-to-end
- [ ] Add integration tests or mocked service tests for SyncService/CachedDataService
- [ ] Consider returning proper HTTP 401 status codes from AuthMiddleware instead of 200 + error body
- [ ] Trigger `deploy-api` workflow manually to verify it works with SCM basic auth now enabled
- [ ] Fix pre-existing `AzureStorageConfig` not found errors in `ImageService.cs` and `ProfilePictureService.cs`

**Decisions Made:**
- Auth middleware returns HTTP 200 with `ApiResponse` error body (`UNAUTHORIZED`) rather than HTTP 401 — existing pattern, not changing now
- Resource group is `rg-haircuthistory` (not `HaircutHistoryRG`)
- deploy-api.yml also triggers on `workflow_dispatch` for manual deployments
- CI only builds Shared + API + runs tests — no MAUI builds in GitHub Actions (user builds locally, Codemagic handles iOS)
- Enabled SCM and FTP basic auth on Azure to allow publish profile deployment from GitHub Actions

**Notes:**
- Azure Functions Core Tools v4.6.0, Azure CLI logged into "Summit Technology Group LLC"
- 9 nullable reference warnings in `AuthMiddleware.cs` — not errors, low priority
- `release.yml` and `azure-static-web-app.yml` left unchanged
- 6 commits this session: initial deploy+CI, 4 CI fixes, final MAUI job removal

---

### [SESSION-001] — 2026-02-23

**Goal:** Establish project documentation, commit pending work, and harden the codebase

**Completed:**
- Created `CLAUDE.md` — project constitution with architecture decisions, patterns, tech stack, API routes, and file structure
- Created `CHANGELOG.md` — release history built from git commit log (v0.1.0 through Unreleased)
- Created `WORKLOG.md` — session work log (this file)
- Set up auto-memory (`MEMORY.md`) for cross-session context
- Updated `.gitignore` — added `publish/` and `nul`, removed stray `nul` artifact file
- Committed offline-first sync feature (SqliteService, SyncService, CachedDataService, SyncDtos, Cache models, and all related changes — 48 files, +2899 lines)
- Standardized error handling — refactored `ProfileListViewModel.LoadProfilesAsync` to use `ExecuteAsync`, removed obsolete PlayFab error mapping from `AlertService`, added logging to silent catch
- Rewrote entire test suite for current architecture — 58 tests across 7 test files, all passing. Fixed broken project reference (was `HaircutHistoryApp.Core`, now `HaircutHistoryApp.Shared`). Tests cover: Profile, Measurement, HaircutRecord, User, ProfileShare, ShareToken, ApiResponse, ErrorCodes, PaginatedResponse, SyncDtos

**In Progress / Pending:**
- [ ] Test bidirectional sync between SQLite cache and Cosmos DB end-to-end
- [ ] Add integration tests or mocked service tests for SyncService/CachedDataService (blocked by MAUI TFM mismatch with plain net8.0 test project)
- [ ] End-to-end deployment and smoke test of Azure Functions API

**Decisions Made:**
- Adopted session tracking workflow: read WORKLOG.md at session start, append entry at session end
- Adopted changelog format: Keep a Changelog with Semantic Versioning
- CLAUDE.md serves as the single source of truth for architecture and patterns
- Test project targets `net8.0` and references `HaircutHistoryApp.Shared` only (can't reference MAUI project from plain net8.0)
- Error handling: all ViewModels should use `BaseViewModel.ExecuteAsync` for consistent user-facing errors via `AlertService.GetUserFriendlyMessage`

**Notes:**
- GitHub CLI (`gh`) and Azure CLI (`az`) are available for deployment and repo operations
- Working tree is clean — all changes committed and pushed to `master`
- 5 commits this session: docs, .gitignore, sync feature, error handling, test rewrite

---

<!--
Session template — copy to top of Session Log for each new session:

### [SESSION-XXX] — YYYY-MM-DD

**Goal:**

**Completed:**
-

**In Progress / Pending:**
- [ ]

**Decisions Made:**
-

**Notes:**
-
-->
