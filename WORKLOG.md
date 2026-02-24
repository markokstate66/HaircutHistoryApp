# Work Log

Running log of Claude Code sessions. Most recent entry at the top.
Each session appends a new entry using the template below.

---

## Session Log

---

### [SESSION-001] — 2026-02-23

**Goal:** Establish project documentation and session tracking

**Completed:**
- Created `CLAUDE.md` — project constitution with architecture decisions, patterns, tech stack, API routes, and file structure
- Created `CHANGELOG.md` — release history built from git commit log (v0.1.0 through Unreleased)
- Created `WORKLOG.md` — session work log (this file)
- Set up auto-memory (`MEMORY.md`) for cross-session context

**In Progress / Pending:**
- [ ] Offline-first sync implementation (SqliteService, SyncService, CachedDataService are in modified/untracked state)
- [ ] Commit current working tree changes (many modified and new files across the project)
- [ ] Review and finalize sync DTOs
- [ ] Test bidirectional sync between SQLite cache and Cosmos DB

**Decisions Made:**
- Adopted session tracking workflow: read WORKLOG.md at session start, append entry at session end
- Adopted changelog format: Keep a Changelog with Semantic Versioning
- CLAUDE.md serves as the single source of truth for architecture and patterns

**Notes:**
- Git status shows extensive uncommitted changes across services, models, ViewModels, and Views — likely from the sync/caching feature work
- New untracked files: SyncDtos.cs, Cache/ models, SqliteService.cs, SyncService.cs, CachedDataService.cs
- GitHub CLI (`gh`) and Azure CLI (`az`) are available for deployment and repo operations

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
