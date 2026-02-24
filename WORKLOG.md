# Work Log

Running log of Claude Code sessions. Most recent entry at the top.
Each session appends a new entry using the template below.

---

## Session Log

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
