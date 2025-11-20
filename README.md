# Crow Vault

Crow Vault is a cross-platform .NET MAUI app that secures notes, passwords, and reminders. A fully in-memory REST server (Minimal API + TestServer) is hosted inside the app for now, ensuring clients speak to a realistic HTTP surface until the dedicated service is ready.

## Projects

| Path | Description |
| --- | --- |
| `src/Core/Dadstart.Labs.Crow.Core` | Domain models, API contracts, and service abstractions |
| `src/Server/Dadstart.Labs.Crow.Server` | In-memory REST API, session/auth services, and test server host |
| `src/Apps/Dadstart.Labs.Crow.App` | .NET MAUI client (Android, iOS, macOS, Windows) using CommunityToolkit + local notifications |
| `tests/Dadstart.Labs.Crow.Core.Tests` | xUnit tests covering repository logic and the HTTP surface |

See `docs/ARCHITECTURE.md` for a deeper walkthrough.

## Getting started

1. Install the .NET 9 SDK + MAUI workloads (the repo pins SDK `9.0.307` via `global.json`).
2. Restore workloads + packages:

   ```powershell
   dotnet workload restore
   dotnet restore CrowVault.sln
   ```

3. Run the MAUI app on the desired target (examples):

   ```powershell
   # Windows
   dotnet build src\Apps\Dadstart.Labs.Crow.App\Dadstart.Labs.Crow.App.csproj -t:Run -f net9.0-windows10.0.19041.0

   # Android emulator
   dotnet build src\Apps\Dadstart.Labs.Crow.App\Dadstart.Labs.Crow.App.csproj -t:Run -f net9.0-android
   ```

4. Execute tests to validate the server + repository:

   ```powershell
   dotnet test CrowVault.sln
   ```

## Current capabilities

- PIN / password / biometric unlock pipeline with secure hashing and per-device biometric enrollment (Windows Hello wired up today; other platforms plug in via `DeviceSecurityService`)
- REST endpoints for setup, authentication, and CRUD operations on notes, passwords, and reminders
- In-app reminder alerts with vibration cues while the client is running (platform notification adapters can be dropped in later without touching the UI)
- MVVM-driven MAUI UI with quick-add actions for rapid iteration during MVP development

## Next steps

- Persist vault data beyond the app lifetime (SQLite + encrypted container)
- Implement remote server deployment using the existing Minimal API
- Flesh out detail/edit screens, search filters, and secret sharing workflows
- Expand automated testing (UI + component) and add CI via GitHub Actions
