# PanoPOS Desktop

## Configuration

`desktop.config.json` is the single build-time source for the API origin,
terminal CihazId, request timeout and health timeout. The initial values match
the backend HTTP launch profile and development seed device.

Change this file and rebuild for another terminal/API host. Runtime settings
and setup integration are not implemented. Do not put PINs or tokens here.
Tauri's native HTTP allowlist is generated from this same API origin during
Cargo build; it permits only `/api/v1/*` on that host.

The API has no CORS policy. Native Desktop uses the official Tauri HTTP
plugin; browser development uses Vite's same-origin API proxy. No backend
configuration or code change is required. `npm run preview` is only a static
asset preview, not the API-enabled development host.

## Run

- `npm ci`
- `npm run dev`: browser development
- `npm run tauri -- dev`: native development
- `npm test`: isolated Desktop tests, no live API or database writes
- `npm run build`: strict TypeScript and Vite
- `cargo check --manifest-path src-tauri/Cargo.toml`
- `npm run tauri -- build --no-bundle`: Windows executable (no installer)

Rust/Cargo must be on PATH. The backend must be running separately for live
login. Session/token remain in memory only. Login has no branch selector:
the backend resolves the active branch from the configured device.
Health means the API process responds, not a database connectivity check.
An unreachable logout clears local memory but cannot confirm server logout.

## Shell and navigation

React Router uses hash routes (`#/login`, `#/`, `#/hizli-satis`) so packaged
Tauri assets do not require server-side route fallback. All non-login routes
require the in-memory session; logout/401 returns to login. Navigation and
dashboard cards use the same module list. Only Home and Quick Sale are enabled;
Quick Sale is intentionally a non-commercial placeholder for the next task.

The shared shell owns the collapsible sidebar, session header and user logout
disclosure. Sidebar state is not persisted. No dashboard totals are fabricated.
Lucide is the single navigation icon library. The local clock updates itself
once per minute, not the whole shell.

Login and Shell share one health provider and the existing API health method.
It checks on startup, once per minute while visible, on network recovery, and
on explicit retry; focus/visibility changes only check when the last check is
at least one minute old. Connection failure does not clear the session or hide
the shell. This is a connection indicator, not offline transaction support.
