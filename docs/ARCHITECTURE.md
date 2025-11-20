# Crow Vault Architecture

## Solution overview

```
CrowVault.sln
├── src/Core/Dadstart.Labs.Crow.Core          # Domain models, contracts, abstractions
├── src/Server/Dadstart.Labs.Crow.Server      # In-memory REST API + hosting helpers
├── src/Apps/Dadstart.Labs.Crow.App           # .NET MAUI client (iOS, Android, macOS, Windows)
└── tests/Dadstart.Labs.Crow.Core.Tests       # xUnit integration/unit tests
```

### Core library

* Models (`SecureNote`, `PasswordEntry`, `ReminderEntry`) share a base `VaultItem` and immutable tag collections.
* Security contracts define authentication flows (`VaultSetupRequest`, `UnlockRequest`, `UnlockResponse`, `VaultSession`).
* `IVaultRepository`, `IVaultSecurityService`, `IVaultSessionService`, and `IVaultApiClient` describe storage, auth, session, and REST client behavior.
* Routes are centralized via `VaultApiRoutes` for server/client parity.

### Server library

* `InMemoryVaultRepository` persists entities inside concurrent dictionaries.
* `VaultSecretsStore` + `SecretHasher` protect master PIN/password with PBKDF2 (SHA-256, 210k iterations) and track biometric registrations per device.
* `InMemoryVaultSessionService` issues opaque session tokens with configurable lifetimes.
* `InMemoryVaultSecurityService` enforces setup + unlock flows and exposes biometric enrollment.
* Minimal API endpoints are mapped via `VaultEndpointExtensions`, with `SessionValidationFilter` gating protected routes.
* `InProcessVaultServer` boots the API with `TestServer` so clients can talk to the REST surface without a separate process.

### MAUI client

* `MauiProgram` wires CommunityToolkit, Local Notification plugin, and Device Fingerprint plugin, then boots the in-process server and DI graph (services, view models, views, AppShell).
* `VaultApiClient` implements `IVaultApiClient` on top of `HttpClient`, honoring the REST contract and `X-Vault-Session` header.
* `DeviceSecurityService` bridges biometrics/PIN/secure storage needs per platform, generating persistent device IDs and tokens (Windows Hello wired today; Android/iOS/Mac hooks share the same seam).
* `LocalReminderNotificationScheduler` currently provides in-app alerts + vibration while the client is active; swap in platform-native notification adapters as they become available.
* `StartupViewModel` orchestrates setup + unlock UX, preferring biometrics when available; once unlocked it issues `VaultUnlockedMessage` so `App` can swap to `AppShell` (tabs for Notes/Passwords/Reminders).
* Dashboard view models (`NotesViewModel`, `PasswordsViewModel`, `RemindersViewModel`) call the API, hydrate observable collections, and expose quick-add commands; reminder sync also schedules notifications locally.

### Testing

* `VaultServerTests` run against the full in-memory REST server to ensure setup/unlock/note workflows behave over HTTP.
* `VaultRepositoryTests` exercise the repository sorting/filtering behavior.

## Extensibility notes

* Swap `InMemoryVaultRepository` with a persistent backing store (SQLite, EF Core, etc.) without touching controllers or apps.
* Replace `InProcessVaultServer` with actual remote endpoints (or a future web/server) by swapping the `HttpClient` registration; the client only depends on `IVaultApiClient`.
* Biometric + notification services implement interfaces so future platform-specific implementations can be injected per-target if the plugins change.
* Add new surfaces (web, desktop) by referencing the Core + Server libraries and the contracts they define.

