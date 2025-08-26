# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Repository overview
- Project: ChildGuard
- .NET 8 solution with multiple projects: Windows Service, WinForms UI, shared Core, Hooking library, and xUnit tests.

## Common commands
- Restore: dotnet restore .\ChildGuard.sln
- Build: dotnet build .\ChildGuard.sln -c Release -v minimal
- Run UI (dev): dotnet run --project .\ChildGuard.UI\ChildGuard.UI.csproj
- Test all: dotnet test .\ChildGuard.sln -c Release --logger "trx;LogFileName=test_results.trx"
- Run a single test: dotnet test .\ChildGuard.Tests\ChildGuard.Tests.csproj --filter "FullyQualifiedName~ChildGuard.Tests.CoreModelsTests.JsonlSink_Writes_Line"
- Format (lint/format): dotnet format .\ChildGuard.sln --verify-no-changes --severity info

Service lifecycle:
- Publish + install service: pwsh -File .\scripts\publish_and_install_service.ps1
- Uninstall service: pwsh -File .\scripts\uninstall_service.ps1

## Architecture and structure (big picture)
- Windows Service (ChildGuard.Service):
  - Hosts a long-running worker configured as a Windows Service (service name: ChildGuardService).
  - Logs to Windows Event Log. Future responsibilities: orchestrate per-user agents, data sinks, and policy.
- WinForms UI (ChildGuard.UI):
  - Parent-facing local UI for development and configuration. Currently demonstrates input monitoring (low-level hooks) with privacy-preserving counters (does not log actual keys).
- Hooking library (ChildGuard.Hooking):
  - Wraps Windows low-level keyboard/mouse hooks (SetWindowsHookEx, WH_KEYBOARD_LL/WH_MOUSE_LL) and raises sanitized activity events.
  - Target framework: net8.0-windows (Windows-only APIs).
- Core (ChildGuard.Core):
  - Shared models (ActivityEvent, ActivityEventType, ActiveWindowInfo) and abstractions.
  - Includes a JSONL file sink for lightweight logging to disk.
- Tests (ChildGuard.Tests):
  - xUnit tests; example covers JSONL sink behavior.

Notes and constraints:
- Global hooks should run in an interactive user session, not in Session 0 (Windows Service). The UI currently hosts hooks for development; a per-user agent can be introduced later for production.
- Anti-tamper and self-defense should rely on OS-supported mechanisms (service recovery actions, permissions, and admin-only controls), avoiding invasive techniques.

## Assistant/workspace rules
- No CLAUDE rules (CLAUDE.md), Cursor rules (.cursor/rules/ or .cursorrules), or Copilot rules (.github/copilot-instructions.md) are present.

## Maintenance
- Keep this file updated as components evolve (e.g., add per-user agent, device policy enforcement, process/web activity collectors, and richer tests).
