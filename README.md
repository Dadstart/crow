# Crow

A secure, cross-platform personal information management application for storing notes, passwords, and reminders. Built with .NET MAUI and ASP.NET Core.

## Features

- **Notes**: Create, edit, and organize notes with tags
- **Passwords**: Securely store and manage passwords with AES encryption
- **Reminders**: Track tasks and reminders with due dates
- **Authentication**: Secure JWT-based authentication with BCrypt password hashing
- **Cross-Platform**: Native apps for iOS, Android, Windows, and macOS

## Architecture

Crow follows a clean architecture pattern with separation of concerns:

- **Crow.App**: .NET MAUI cross-platform mobile/desktop application
- **Crow.Api**: ASP.NET Core REST API backend
- **Crow.Models**: Shared domain models and DTOs
- **Test Projects**: Unit tests, integration tests, and API tests

### Current Implementation

- **Storage**: In-memory storage (MVP phase)
- **Authentication**: JWT tokens with configurable expiration
- **Encryption**: AES encryption for password storage
- **Password Hashing**: BCrypt for user passwords

## Tech Stack

- **.NET 10** with **C# 14**
- **.NET MAUI** for cross-platform UI
- **ASP.NET Core** for REST API
- **CommunityToolkit.Mvvm** for MVVM pattern
- **xUnit** for testing
- **Moq** for mocking
- **BCrypt.Net** for password hashing
- **System.IdentityModel.Tokens.Jwt** for JWT authentication

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PowerShell 7.5 (for scripts)
- Platform-specific requirements:
  - **Windows**: Windows 11 24H2 or newer
  - **Android**: Android SDK (API level 21+)
  - **iOS**: macOS with Xcode (iOS 15.0+)
  - **macOS**: macOS 15.0+ with Xcode

## Getting Started

### Clone the Repository

```powershell
git clone <repository-url>
cd crow
```

### Build the Solution

```powershell
dotnet build
```

### Run Tests

```powershell
dotnet test
```

### Configure the API

1. Update `Crow.Api/appsettings.json` with your JWT configuration:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-here-minimum-32-characters",
    "Issuer": "CrowApi",
    "Audience": "CrowApp",
    "ExpirationMinutes": "1440"
  }
}
```

### Run the API

```powershell
cd Crow.Api
dotnet run
```

The API will be available at `https://localhost:5064` (or the port configured in `launchSettings.json`).

### Run the MAUI App

```powershell
cd Crow.App
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

For other platforms, use the appropriate target framework:
- `net10.0-android` for Android
- `net10.0-ios` for iOS
- `net10.0-maccatalyst` for macOS

## Project Structure

```
crow/
├── Crow.Api/              # ASP.NET Core REST API
│   ├── Controllers/      # API controllers
│   ├── Services/         # Business logic services
│   └── Program.cs        # Application entry point
├── Crow.App/             # .NET MAUI application
│   ├── Pages/           # XAML pages
│   ├── ViewModels/      # MVVM view models
│   ├── Services/        # Client services
│   └── Platforms/       # Platform-specific code
├── Crow.Models/         # Shared domain models
│   └── Dtos/            # Data transfer objects
├── Crow.Api.Tests/      # API unit tests
├── Crow.Api.IntegrationTests/  # API integration tests
├── Crow.Models.Tests/   # Model unit tests
└── Crow.App.Tests/      # App unit tests
```

## Development Workflow

### Before Committing

1. Build the solution:
   ```powershell
   dotnet build
   ```

2. Verify code formatting:
   ```powershell
   dotnet format --verify-no-changes
   ```

3. Run tests:
   ```powershell
   dotnet test
   ```

### Code Style

- **C# 14** with nullable reference types enabled
- **Records** for immutable data types
- **DateTimeOffset** for all timestamps (UTC)
- Follow Clean Code principles
- See `AGENTS.md` for detailed coding guidelines

## Testing

The project includes comprehensive test coverage:

- **Unit Tests**: Test individual components and services
- **Integration Tests**: Test API endpoints end-to-end
- **Model Tests**: Test domain model behavior

Run all tests:
```powershell
dotnet test
```

## Supported Platforms

### Mobile
- **iOS**: iOS 15.0+ (iPhone and iPad)
- **Android**: Android 16+ (phone and tablet)

### Desktop
- **Windows**: Windows 11 24H2+ (touch-enabled tablet and desktop)
- **macOS**: macOS 15.0+ (minimal support)

## Security

- Passwords are hashed using BCrypt
- Stored passwords are encrypted using AES encryption
- JWT tokens for secure authentication
- All timestamps stored in UTC using DateTimeOffset

## Contributing

1. Follow the coding guidelines in `AGENTS.md`
2. Ensure all tests pass before submitting
3. Run `dotnet format` to ensure consistent formatting
4. Add tests for new features

## License

See `LICENSE` file for details.

## Roadmap

- [ ] Persistent storage (database integration)
- [ ] Web client
- [ ] Cloud sync
- [ ] Biometric authentication
- [ ] Password generator
- [ ] Export/import functionality
