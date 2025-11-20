# Comprehensive Code Review Plan: Crow Project

## Overview

This document provides a step-by-step guided walkthrough for reviewing the entire Crow codebase. It's designed for senior C# engineers who are new to .NET MAUI but have strong expertise in C# and ASP.NET Core.

**Project Summary**: Crow is a secure, cross-platform personal information management application for storing notes, passwords, and reminders. It consists of:
- **Backend**: ASP.NET Core REST API with in-memory storage (MVP phase)
- **Frontend**: .NET MAUI cross-platform application (iOS, Android, Windows, macOS)
- **Shared**: Domain models and DTOs used by both layers

**Tech Stack**:
- .NET 10 with C# 14
- .NET MAUI for cross-platform UI
- ASP.NET Core for REST API
- CommunityToolkit.Mvvm for MVVM pattern
- xUnit for testing
- JWT authentication with BCrypt password hashing
- AES encryption for stored passwords

---

## Review Structure

The review is organized into **7 phases**, progressing from foundation to application layers:

1. **Phase 1**: Project Structure & Configuration (15 min)
2. **Phase 2**: Domain Models & DTOs (20 min)
3. **Phase 3**: API Backend Layer (30 min)
4. **Phase 4**: MAUI Application Layer (45 min)
5. **Phase 5**: Test Projects (30 min)
6. **Phase 6**: Platform-Specific Code (15 min)
7. **Phase 7**: Documentation & Resources (10 min)

**Total Estimated Time**: ~2.5 hours for thorough review

---

## Phase 1: Project Structure & Configuration (15 minutes)

**Purpose**: Understand the solution structure, dependencies, and build configuration.

### 1.1 Solution File
- **File**: `Crow.sln`
- **Review Focus**:
  - Solution contains 7 projects
  - Project references and dependencies
  - Build configurations (Debug/Release, Any CPU/x64/x86)

### 1.2 Project Files (.csproj)
Review each project file to understand dependencies and target frameworks:

#### Crow.Models.csproj
- **File**: `Crow.Models/Crow.Models.csproj`
- **Review Focus**:
  - Target framework (should be net10.0)
  - NuGet packages
  - Nullable reference types enabled
  - C# language version (should be 14)

#### Crow.Api.csproj
- **File**: `Crow.Api/Crow.Api.csproj`
- **Review Focus**:
  - Target framework (net10.0)
  - ASP.NET Core packages
  - JWT authentication packages
  - OpenAPI/Swagger configuration

#### Crow.App.csproj
- **File**: `Crow.App/Crow.App.csproj`
- **Review Focus**:
  - **MAUI-specific**: Multiple target frameworks (net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows10.0.19041.0)
  - MAUI packages
  - CommunityToolkit.Mvvm package
  - Platform-specific references
  - Resource files (fonts, images, styles)

#### Test Project Files
- **Files**:
  - `Crow.Api.Tests/Crow.Api.Tests.csproj`
  - `Crow.Api.IntegrationTests/Crow.Api.IntegrationTests.csproj`
  - `Crow.Models.Tests/Crow.Models.Tests.csproj`
  - `Crow.App.Tests/Crow.App.Tests.csproj`
- **Review Focus**:
  - xUnit test framework
  - Moq for mocking
  - Test project references

### 1.3 Configuration Files

#### API Configuration
- **Files**:
  - `Crow.Api/appsettings.json` - Production settings
  - `Crow.Api/appsettings.Development.json` - Development overrides
  - `Crow.Api/appsettings.Testing.json` - Testing configuration
  - `Crow.Api/Properties/launchSettings.json` - Launch profiles
- **Review Focus**:
  - JWT configuration (SecretKey, Issuer, Audience, ExpirationMinutes)
  - HTTPS ports and URLs
  - Environment-specific settings

#### MAUI Configuration
- **File**: `Crow.App/Properties/launchSettings.json`
- **Review Focus**:
  - Launch profiles for different platforms
  - Debug configurations

### 1.4 Root Documentation
- **Files**:
  - `README.md` - Project overview and getting started
  - `AGENTS.md` - Coding guidelines and conventions
  - `LICENSE` - License information
- **Review Focus**:
  - Understand project goals and architecture
  - Review coding standards and conventions
  - Note supported platforms and versions

---

## Phase 2: Domain Models & DTOs (20 minutes)

**Purpose**: Understand the core domain entities and API contracts. This is the foundation shared by both API and MAUI app.

### 2.1 Domain Models (Immutable Records)

All domain models use C# 14 record types with primary constructors for immutability.

#### User Model
- **File**: `Crow.Models/User.cs`
- **Review Focus**:
  - Immutable record structure
  - Fields: Id (Guid), Username, Email, PasswordHash, CreatedAt, UpdatedAt
  - All timestamps use `DateTimeOffset` (UTC)

#### Note Model
- **File**: `Crow.Models/Note.cs`
- **Review Focus**:
  - Fields: Id, Title, Content, CreatedAt, UpdatedAt, Tags (List<string>)
  - Tags are non-nullable (defaults to empty list)

#### Password Model
- **File**: `Crow.Models/Password.cs`
- **Review Focus**:
  - Fields: Id, Title, Username, EncryptedPassword, Url (nullable), Notes (nullable), CreatedAt, UpdatedAt
  - Note: Password is stored encrypted, not plaintext

#### Reminder Model
- **File**: `Crow.Models/Reminder.cs`
- **Review Focus**:
  - Fields: Id, Title, Description (nullable), DueDate (nullable), IsCompleted, CreatedAt, UpdatedAt
  - Completion status tracking

### 2.2 Data Transfer Objects (DTOs)

DTOs use records with data annotations for validation. Review in this order:

#### Authentication DTOs
- **LoginDto.cs**: Simple username/password
  - Required fields with error messages
- **RegisterDto.cs**: Registration with validation
  - Username: 3-50 chars, alphanumeric + underscore (regex)
  - Email: EmailAddress validation, max 255 chars
  - Password: MinLength 8, MaxLength 100

#### Create DTOs (Required Fields)
- **CreateNoteDto.cs**:
  - Title: Required, MaxLength 200
  - Content: Required, MaxLength 10000
  - Tags: Optional List<string>
- **CreatePasswordDto.cs**:
  - Title: Required, MaxLength 200
  - Username: Required, MaxLength 255
  - Password: Required, MaxLength 500 (plaintext before encryption)
  - Url: Optional, URL validation, MaxLength 2048
  - Notes: Optional, MaxLength 5000
- **CreateReminderDto.cs**:
  - Title: Required, MaxLength 200
  - Description: Optional, MaxLength 5000
  - DueDate: Optional DateTimeOffset

#### Update DTOs (All Optional for Partial Updates)
- **UpdateNoteDto.cs**: All fields nullable
- **UpdatePasswordDto.cs**: All fields nullable, URL validation when provided
- **UpdateReminderDto.cs**: Includes IsCompleted for status updates

#### Response DTOs
- **AuthResponseDto.cs**: Token, RefreshToken, Username, UserId
- **PasswordResponseDto.cs**: Decrypted password response (separate from domain model)
- **RefreshTokenDto.cs**: Refresh token request

### 2.3 Factory Pattern Implementation

Factories encapsulate domain logic for creating and updating entities. They use `TimeProvider` for testability.

#### UserFactory.cs
- **File**: `Crow.Models/Factories/UserFactory.cs`
- **Review Focus**:
  - Constructor injection of `TimeProvider`
  - `Create()` method with validation
  - Email normalization (ToLowerInvariant)
  - String trimming

#### NoteFactory.cs
- **File**: `Crow.Models/Factories/NoteFactory.cs`
- **Review Focus**:
  - `Create()` method with validation
  - `WithUpdate()` method using `with` expressions for immutability
  - Tags default to empty list (`[]`)
  - Always updates `UpdatedAt` timestamp

#### PasswordFactory.cs
- **File**: `Crow.Models/Factories/PasswordFactory.cs`
- **Review Focus**:
  - Accepts `encryptedPassword` (encryption handled in controller)
  - Validation for all required fields
  - `WithUpdate()` for partial updates

#### ReminderFactory.cs
- **File**: `Crow.Models/Factories/ReminderFactory.cs`
- **Review Focus**:
  - `Create()` with optional description and due date
  - `WithUpdate()` for partial updates
  - `MarkCompleted()` and `MarkIncomplete()` convenience methods

**Key Patterns to Note**:
- All factories use `TimeProvider` instead of `DateTimeOffset.UtcNow`
- Validation throws `ArgumentException` with parameter names
- Immutability via `with` expressions
- String normalization (trim, lowercase where appropriate)

---

## Phase 3: API Backend Layer (30 minutes)

**Purpose**: Review the ASP.NET Core REST API implementation, including controllers, services, and authentication.

### 3.1 Application Entry Point

#### Program.cs
- **File**: `Crow.Api/Program.cs`
- **Review Focus**:
  - Minimal API setup (no Startup.cs)
  - Service registration (factories, services, authentication)
  - **TimeProvider registration**: `TimeProvider.System` registered as singleton
  - JWT authentication configuration
  - CORS policy (AllowAnyOrigin for development)
  - OpenAPI/Swagger mapping (development only)
  - Middleware pipeline order

**Key Services Registered**:
- `TimeProvider` → `TimeProvider.System`
- Factories (UserFactory, NoteFactory, PasswordFactory, ReminderFactory)
- `IStorageService` → `InMemoryStorageService`
- `IEncryptionService` → `AesEncryptionService`
- `IPasswordHasher` → `BCryptPasswordHasher`
- `IJwtTokenService` → `JwtTokenService`
- `IAuthService` → `AuthService` (scoped)

### 3.2 Controllers

Controllers follow RESTful conventions and use dependency injection.

#### AuthController.cs
- **File**: `Crow.Api/Controllers/AuthController.cs`
- **Review Focus**:
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/login` - User authentication
  - No `[Authorize]` attribute (public endpoints)
  - Returns `AuthResponseDto` with JWT token

#### NotesController.cs
- **File**: `Crow.Api/Controllers/NotesController.cs`
- **Review Focus**:
  - `[Authorize]` attribute on controller (all endpoints protected)
  - CRUD operations:
    - `GET /api/notes` - Get all notes
    - `GET /api/notes/{id}` - Get by ID
    - `POST /api/notes` - Create note (uses CreateNoteDto → NoteFactory)
    - `PUT /api/notes/{id}` - Update note (uses UpdateNoteDto → NoteFactory.WithUpdate)
    - `DELETE /api/notes/{id}` - Delete note
  - Returns domain models directly

#### PasswordsController.cs
- **File**: `Crow.Api/Controllers/PasswordsController.cs`
- **Review Focus**:
  - `[Authorize]` attribute on controller
  - Encryption/decryption flow:
    - Create: DTO password → Encrypt → Factory → Storage
    - Get: Storage → Decrypt → PasswordResponseDto
  - Returns `PasswordResponseDto` (decrypted) instead of domain model
  - Update handles optional password (only encrypts if provided)

#### RemindersController.cs
- **File**: `Crow.Api/Controllers/RemindersController.cs`
- **Review Focus**:
  - `[Authorize]` attribute on controller
  - CRUD operations similar to NotesController
  - Uses `ReminderFactory.WithUpdate()` for updates
  - Returns domain models directly

### 3.3 Service Interfaces

#### IStorageService.cs
- **File**: `Crow.Api/Services/IStorageService.cs`
- **Review Focus**:
  - In-memory storage interface (MVP phase)
  - CRUD operations for Users, Notes, Passwords, Reminders
  - Refresh token management methods
  - All methods are async (prepared for future database integration)

#### IEncryptionService.cs
- **File**: `Crow.Api/Services/IEncryptionService.cs`
- **Review Focus**:
  - `Encrypt()` and `Decrypt()` methods
  - Used for password storage encryption

#### IPasswordHasher.cs
- **File**: `Crow.Api/Services/IPasswordHasher.cs`
- **Review Focus**:
  - `HashPassword()` and `VerifyPassword()` methods
  - Used for user password hashing (BCrypt)

#### IJwtTokenService.cs
- **File**: `Crow.Api/Services/IJwtTokenService.cs`
- **Review Focus**:
  - `GenerateToken()` - Creates JWT token
  - `GenerateRefreshToken()` - Creates refresh token
  - Token generation and validation

#### IAuthService.cs
- **File**: `Crow.Api/Services/IAuthService.cs`
- **Review Focus**:
  - `RegisterAsync()` - User registration
  - `LoginAsync()` - User authentication
  - `RefreshTokenAsync()` - Token refresh
  - User lookup methods

### 3.4 Service Implementations

#### InMemoryStorageService.cs
- **File**: `Crow.Api/Services/InMemoryStorageService.cs`
- **Review Focus**:
  - Thread-safe in-memory storage using `ConcurrentDictionary`
  - User storage by ID, Username, Email
  - Refresh token storage and management
  - All CRUD operations implemented
  - **Note**: Data is lost on restart (MVP phase)

#### AesEncryptionService.cs
- **File**: `Crow.Api/Services/AesEncryptionService.cs`
- **Review Focus**:
  - AES encryption implementation
  - Key management (should be secure in production)
  - Encryption/decryption methods

#### BCryptPasswordHasher.cs
- **File**: `Crow.Api/Services/BCryptPasswordHasher.cs`
- **Review Focus**:
  - BCrypt password hashing
  - Secure password verification

#### JwtTokenService.cs
- **File**: `Crow.Api/Services/JwtTokenService.cs`
- **Review Focus**:
  - JWT token generation using `System.IdentityModel.Tokens.Jwt`
  - Token claims (UserId, Username)
  - Refresh token generation (random string)
  - Configuration from `IConfiguration`

#### AuthService.cs
- **File**: `Crow.Api/Services/AuthService.cs`
- **Review Focus**:
  - Registration: Checks for duplicate username/email
  - Login: Validates credentials
  - Refresh token: Validates and rotates tokens
  - Uses `UserFactory` for user creation
  - Returns `AuthResponseDto` with tokens

### 3.5 API Test File
- **File**: `Crow.Api/Crow.Api.http`
- **Review Focus**:
  - HTTP client test file (Visual Studio/Rider)
  - Example API calls for testing
  - Authentication flow examples

---

## Phase 4: MAUI Application Layer (45 minutes)

**Purpose**: Understand the .NET MAUI application structure, MVVM pattern, and UI implementation.

**MAUI Concepts for C# Engineers**:
- **XAML**: XML-based markup for UI (similar to WPF/UWP)
- **Code-Behind**: C# files paired with XAML (`.xaml.cs`)
- **MVVM**: Model-View-ViewModel pattern (separation of UI and logic)
- **Dependency Injection**: Same patterns as ASP.NET Core
- **Shell**: Navigation system in MAUI
- **Binding**: Data binding between ViewModels and Views

### 4.1 Application Entry Point

#### MauiProgram.cs
- **File**: `Crow.App/MauiProgram.cs`
- **Review Focus**:
  - MAUI app builder pattern (similar to `Program.cs` in API)
  - Font configuration (OpenSans fonts)
  - **Service Registration**:
    - `TimeProvider` → `TimeProvider.System`
    - Factories (same as API)
    - `IAuthService` → `Services.AuthService`
    - `IApiService` → `ApiService` (with HttpClient)
    - ViewModels (transient)
    - Pages (transient)
  - HttpClient registration for API calls
  - Logging configuration (Debug in DEBUG mode)

**Key Difference from API**: ViewModels and Pages are registered in DI (MVVM pattern)

### 4.2 Application Shell & Navigation

#### App.xaml
- **File**: `Crow.App/App.xaml`
- **Review Focus**:
  - Application-level resources
  - Global styles and themes

#### App.xaml.cs
- **File**: `Crow.App/App.xaml.cs`
- **Review Focus**:
  - Application initialization
  - Window creation using `AppShell`

#### AppShell.xaml
- **File**: `Crow.App/AppShell.xaml`
- **Review Focus**:
  - Shell navigation structure
  - Route definitions for pages
  - Navigation menu/tabs (if present)

#### AppShell.xaml.cs
- **File**: `Crow.App/AppShell.xaml.cs`
- **Review Focus**:
  - Shell initialization
  - Navigation logic

### 4.3 Pages (Views)

Pages are the UI layer. Each page has a XAML file (UI markup) and a code-behind file (event handlers).

#### LoginPage
- **Files**:
  - `Crow.App/Pages/LoginPage.xaml` - UI markup
  - `Crow.App/Pages/LoginPage.xaml.cs` - Code-behind
- **Review Focus**:
  - Login form UI (username, password fields)
  - Data binding to `LoginPageViewModel`
  - Navigation to main page after login
  - **MAUI-specific**: Entry controls, Button controls, data binding syntax

#### MainPage
- **Files**:
  - `Crow.App/Pages/MainPage.xaml` - UI markup
  - `Crow.App/Pages/MainPage.xaml.cs` - Code-behind
- **Review Focus**:
  - Main navigation/dashboard
  - Navigation to Notes, Passwords, Reminders pages
  - Data binding to `MainPageViewModel`

#### NotesPage
- **Files**:
  - `Crow.App/Pages/NotesPage.xaml` - UI markup
  - `Crow.App/Pages/NotesPage.xaml.cs` - Code-behind
- **Review Focus**:
  - Notes list UI (CollectionView or ListView)
  - Create/edit/delete note functionality
  - Data binding to `NotesPageViewModel`
  - **MAUI-specific**: CollectionView, data templates

#### PasswordsPage
- **Files**:
  - `Crow.App/Pages/PasswordsPage.xaml` - UI markup
  - `Crow.App/Pages/PasswordsPage.xaml.cs` - Code-behind
- **Review Focus**:
  - Password list UI
  - Secure password display (masked)
  - Create/edit/delete password functionality
  - Data binding to `PasswordsPageViewModel`

#### RemindersPage
- **Files**:
  - `Crow.App/Pages/RemindersPage.xaml` - UI markup
  - `Crow.App/Pages/RemindersPage.xaml.cs` - Code-behind
- **Review Focus**:
  - Reminders list UI
  - Due date display
  - Completion status (checkboxes)
  - Data binding to `RemindersPageViewModel`

### 4.4 ViewModels (MVVM Pattern)

ViewModels contain the presentation logic and are bound to Views via data binding. They use `CommunityToolkit.Mvvm` for MVVM helpers.

#### LoginPageViewModel.cs
- **File**: `Crow.App/ViewModels/LoginPageViewModel.cs`
- **Review Focus**:
  - Properties: Username, Password (bindable)
  - Commands: LoginCommand, RegisterCommand
  - Uses `IAuthService` for authentication
  - Navigation after successful login
  - Error handling and user feedback

#### MainPageViewModel.cs
- **File**: `Crow.App/ViewModels/MainPageViewModel.cs`
- **Review Focus**:
  - Navigation commands to different pages
  - User information display
  - Logout functionality

#### NotesPageViewModel.cs
- **File**: `Crow.App/ViewModels/NotesPageViewModel.cs`
- **Review Focus**:
  - Observable collection of notes
  - CRUD operations using `IApiService`
  - Loading states and error handling
  - Commands for create/edit/delete operations

#### PasswordsPageViewModel.cs
- **File**: `Crow.App/ViewModels/PasswordsPageViewModel.cs`
- **Review Focus**:
  - Observable collection of passwords
  - CRUD operations using `IApiService`
  - Secure password handling
  - Loading states and error handling

#### RemindersPageViewModel.cs
- **File**: `Crow.App/ViewModels/RemindersPageViewModel.cs`
- **Review Focus**:
  - Observable collection of reminders
  - CRUD operations using `IApiService`
  - Completion status updates
  - Due date filtering/sorting (if implemented)

**MVVM Pattern Notes**:
- ViewModels use `CommunityToolkit.Mvvm` attributes (`[ObservableProperty]`, `[RelayCommand]`)
- Properties are bindable to XAML
- Commands handle user actions
- ViewModels don't reference Views directly (decoupled)

### 4.5 Services (Client-Side)

#### IApiService.cs
- **File**: `Crow.App/Services/IApiService.cs`
- **Review Focus**:
  - Interface for API communication
  - Methods for Notes, Passwords, Reminders CRUD
  - Async methods returning domain models/DTOs

#### ApiService.cs
- **File**: `Crow.App/Services/ApiService.cs`
- **Review Focus**:
  - HttpClient-based API client
  - Base URL configuration (`http://localhost:5064/api`)
  - **Authentication**: Updates Authorization header from `IAuthService`
  - CRUD operations for all entities
  - Error handling (`EnsureSuccessStatusCode`)
  - JSON serialization/deserialization

#### IAuthService.cs
- **File**: `Crow.App/Services/IAuthService.cs`
- **Review Focus**:
  - Client-side authentication interface
  - Login, Register, Logout methods
  - Token storage and management
  - `IsAuthenticated` property

#### AuthService.cs
- **File**: `Crow.App/Services/AuthService.cs`
- **Review Focus**:
  - Client-side authentication implementation
  - Token storage (likely using `SecureStorage` or `Preferences`)
  - API calls to `/api/auth/login` and `/api/auth/register`
  - Token refresh logic (if implemented)

### 4.6 Converters

#### ListToStringConverter.cs
- **File**: `Crow.App/Converters/ListToStringConverter.cs`
- **Review Focus**:
  - Value converter for XAML data binding
  - Converts `List<string>` (tags) to display string
  - Used in UI for displaying tags

### 4.7 Resources

#### Styles
- **Files**:
  - `Crow.App/Resources/Styles/Colors.xaml` - Color definitions
  - `Crow.App/Resources/Styles/Styles.xaml` - Global styles
- **Review Focus**:
  - MAUI resource dictionary syntax
  - Theme colors and styles
  - Reusable UI resources

#### Fonts
- **Files**:
  - `Crow.App/Resources/Fonts/OpenSans-Regular.ttf`
  - `Crow.App/Resources/Fonts/OpenSans-Semibold.ttf`
- **Review Focus**:
  - Font resources (referenced in `MauiProgram.cs`)

#### Images
- **Files**:
  - `Crow.App/Resources/Images/*.png` - App icons/images
  - `Crow.App/Resources/AppIcon/*.svg` - App icons
  - `Crow.App/Resources/Splash/*.svg` - Splash screen
- **Review Focus**:
  - Image resources for different platforms

---

## Phase 5: Test Projects (30 minutes)

**Purpose**: Review test coverage, test patterns, and test organization.

### 5.1 API Unit Tests

#### AuthControllerTests.cs
- **File**: `Crow.Api.Tests/Controllers/AuthControllerTests.cs`
- **Review Focus**:
  - Registration tests (success, duplicate username/email)
  - Login tests (success, invalid credentials)
  - Mock usage (`IAuthService`)
  - Assertions and test structure

#### NotesControllerTests.cs
- **File**: `Crow.Api.Tests/Controllers/NotesControllerTests.cs`
- **Review Focus**:
  - CRUD operation tests
  - Authorization tests (if present)
  - Mock usage (`IStorageService`, `NoteFactory`)
  - Edge cases (not found, validation errors)

#### PasswordsControllerTests.cs
- **File**: `Crow.Api.Tests/Controllers/PasswordsControllerTests.cs`
- **Review Focus**:
  - CRUD operation tests
  - Encryption/decryption flow tests
  - Mock usage (`IStorageService`, `IEncryptionService`, `PasswordFactory`)

#### PasswordsControllerEncryptionTests.cs
- **File**: `Crow.Api.Tests/Controllers/PasswordsControllerEncryptionTests.cs`
- **Review Focus**:
  - Encryption-specific tests
  - Round-trip encryption/decryption
  - Security considerations

#### RemindersControllerTests.cs
- **File**: `Crow.Api.Tests/Controllers/RemindersControllerTests.cs`
- **Review Focus**:
  - CRUD operation tests
  - Completion status tests
  - Mock usage (`IStorageService`, `ReminderFactory`)

### 5.2 Service Unit Tests

#### AuthServiceTests.cs
- **File**: `Crow.Api.Tests/Services/AuthServiceTests.cs`
- **Review Focus**:
  - Registration logic tests
  - Login logic tests
  - Password hashing verification
  - Token generation tests
  - Mock usage (`IStorageService`, `IPasswordHasher`, `IJwtTokenService`, `UserFactory`)

#### AesEncryptionServiceTests.cs
- **File**: `Crow.Api.Tests/Services/AesEncryptionServiceTests.cs`
- **Review Focus**:
  - Encryption/decryption round-trip tests
  - Different input scenarios
  - Security considerations

#### BCryptPasswordHasherTests.cs
- **File**: `Crow.Api.Tests/Services/BCryptPasswordHasherTests.cs`
- **Review Focus**:
  - Password hashing tests
  - Verification tests
  - Hash uniqueness tests

#### InMemoryStorageServiceTests.cs
- **File**: `Crow.Api.Tests/Services/InMemoryStorageServiceTests.cs`
- **Review Focus**:
  - CRUD operation tests for all entities
  - Thread safety tests (if present)
  - Edge cases (not found, duplicates)

### 5.3 Model Tests

#### NoteTests.cs
- **File**: `Crow.Models.Tests/NoteTests.cs`
- **Review Focus**:
  - Record immutability tests
  - Value equality tests
  - Factory method tests (if present)

#### PasswordTests.cs
- **File**: `Crow.Models.Tests/PasswordTests.cs`
- **Review Focus**:
  - Record immutability tests
  - Value equality tests
  - Factory method tests (if present)

#### ReminderTests.cs
- **File**: `Crow.Models.Tests/ReminderTests.cs`
- **Review Focus**:
  - Record immutability tests
  - Value equality tests
  - Factory method tests (if present)
  - Completion status tests

### 5.4 Integration Tests

#### IntegrationTestBase.cs
- **File**: `Crow.Api.IntegrationTests/IntegrationTestBase.cs`
- **Review Focus**:
  - Base class for integration tests
  - `GetAuthenticatedClientAsync()` helper method
  - Test user creation and authentication
  - HttpClient setup

#### WebApplicationFactory.cs
- **File**: `Crow.Api.IntegrationTests/WebApplicationFactory.cs`
- **Review Focus**:
  - Custom WebApplicationFactory for testing
  - Test server configuration
  - Service overrides (if any)

#### AuthIntegrationTests.cs
- **File**: `Crow.Api.IntegrationTests/AuthIntegrationTests.cs`
- **Review Focus**:
  - End-to-end authentication tests
  - Registration flow
  - Login flow
  - Token validation

#### NotesIntegrationTests.cs
- **File**: `Crow.Api.IntegrationTests/NotesIntegrationTests.cs`
- **Review Focus**:
  - End-to-end CRUD tests
  - Authentication required
  - Full request/response cycle

#### PasswordsIntegrationTests.cs
- **File**: `Crow.Api.IntegrationTests/PasswordsIntegrationTests.cs`
- **Review Focus**:
  - End-to-end CRUD tests
  - Encryption/decryption in full flow
  - Authentication required

#### RemindersIntegrationTests.cs
- **File**: `Crow.Api.IntegrationTests/RemindersIntegrationTests.cs`
- **Review Focus**:
  - End-to-end CRUD tests
  - Completion status updates
  - Authentication required

#### ProtectedEndpointsIntegrationTests.cs
- **File**: `Crow.Api.IntegrationTests/ProtectedEndpointsIntegrationTests.cs`
- **Review Focus**:
  - Authorization tests
  - Unauthorized access tests
  - Token validation tests

### 5.5 App Tests

#### UnitTest1.cs (Placeholder)
- **File**: `Crow.App.Tests/UnitTest1.cs`
- **Review Focus**:
  - Placeholder test file
  - **Note**: MAUI app tests may be limited (UI testing is complex)

---

## Phase 6: Platform-Specific Code (15 minutes)

**Purpose**: Understand platform-specific implementations and configurations.

### 6.1 Android Platform

#### MainActivity.cs
- **File**: `Crow.App/Platforms/Android/MainActivity.cs`
- **Review Focus**:
  - Android activity initialization
  - MAUI app integration

#### MainApplication.cs
- **File**: `Crow.App/Platforms/Android/MainApplication.cs`
- **Review Focus**:
  - Android application initialization
  - Global Android configuration

#### AndroidManifest.xml
- **File**: `Crow.App/Platforms/Android/AndroidManifest.xml`
- **Review Focus**:
  - Android permissions
  - App metadata
  - Target SDK version
  - Internet permission (for API calls)

#### colors.xml
- **File**: `Crow.App/Platforms/Android/Resources/values/colors.xml`
- **Review Focus**:
  - Android-specific color resources

### 6.2 iOS Platform

#### AppDelegate.cs
- **File**: `Crow.App/Platforms/iOS/AppDelegate.cs`
- **Review Focus**:
  - iOS app delegate initialization
  - MAUI app integration

#### Program.cs
- **File**: `Crow.App/Platforms/iOS/Program.cs`
- **Review Focus**:
  - iOS entry point
  - App initialization

#### Info.plist
- **File**: `Crow.App/Platforms/iOS/Info.plist`
- **Review Focus**:
  - iOS app metadata
  - Permissions (network access, etc.)
  - Bundle identifier
  - Minimum iOS version

#### PrivacyInfo.xcprivacy
- **File**: `Crow.App/Platforms/iOS/Resources/PrivacyInfo.xcprivacy`
- **Review Focus**:
  - iOS privacy manifest (required for App Store)
  - Privacy practices declaration

### 6.3 macOS Platform (MacCatalyst)

#### AppDelegate.cs
- **File**: `Crow.App/Platforms/MacCatalyst/AppDelegate.cs`
- **Review Focus**:
  - macOS app delegate initialization

#### Program.cs
- **File**: `Crow.App/Platforms/MacCatalyst/Program.cs`
- **Review Focus**:
  - macOS entry point

#### Info.plist
- **File**: `Crow.App/Platforms/MacCatalyst/Info.plist`
- **Review Focus**:
  - macOS app metadata
  - Permissions

#### Entitlements.plist
- **File**: `Crow.App/Platforms/MacCatalyst/Entitlements.plist`
- **Review Focus**:
  - macOS app entitlements
  - Capabilities and permissions

### 6.4 Windows Platform

#### App.xaml / App.xaml.cs
- **Files**: `Crow.App/Platforms/Windows/App.xaml` and `App.xaml.cs`
- **Review Focus**:
  - Windows-specific app initialization
  - UWP/WinUI integration

#### Package.appxmanifest
- **File**: `Crow.App/Platforms/Windows/Package.appxmanifest`
- **Review Focus**:
  - Windows app package manifest
  - Capabilities (internet, etc.)
  - App metadata
  - Target Windows version

#### app.manifest
- **File**: `Crow.App/Platforms/Windows/app.manifest`
- **Review Focus**:
  - Windows application manifest
  - DPI awareness
  - Compatibility settings

---

## Phase 7: Documentation & Resources (10 minutes)

**Purpose**: Review project documentation and resource files.

### 7.1 Documentation Files

#### README.md
- **File**: `README.md`
- **Review Focus**:
  - Project overview
  - Getting started guide
  - Architecture description
  - Tech stack details
  - Supported platforms

#### AGENTS.md
- **File**: `AGENTS.md`
- **Review Focus**:
  - Coding guidelines
  - C# style conventions
  - Testing requirements
  - Platform support details
  - CI/CD configuration

#### docs/prompts/v0.maui.md
- **File**: `docs/prompts/v0.maui.md`
- **Review Focus**:
  - Project requirements/prompts
  - MVP scope definition

#### docs/prompts/v0.web.md
- **File**: `docs/prompts/v0.web.md`
- **Review Focus**:
  - Future web client requirements
  - Roadmap items

### 7.2 Resource Files

#### Raw Resources
- **File**: `Crow.App/Resources/Raw/*.txt` (if present)
- **Review Focus**:
  - Embedded resource files
  - Usage in application

---

## Review Checklist

Use this checklist to ensure thorough review:

### Phase 1: Project Structure
- [ ] Solution file structure understood
- [ ] All .csproj files reviewed
- [ ] Configuration files reviewed
- [ ] Root documentation read

### Phase 2: Domain Models
- [ ] All domain models reviewed (User, Note, Password, Reminder)
- [ ] All DTOs reviewed (Create, Update, Response)
- [ ] All factories reviewed (UserFactory, NoteFactory, PasswordFactory, ReminderFactory)
- [ ] Validation rules understood
- [ ] Immutability patterns understood

### Phase 3: API Backend
- [ ] Program.cs service registration reviewed
- [ ] All controllers reviewed
- [ ] All service interfaces reviewed
- [ ] All service implementations reviewed
- [ ] Authentication flow understood
- [ ] Encryption/decryption flow understood

### Phase 4: MAUI Application
- [ ] MauiProgram.cs service registration reviewed
- [ ] All pages (XAML + code-behind) reviewed
- [ ] All ViewModels reviewed
- [ ] MVVM pattern understood
- [ ] API service integration reviewed
- [ ] Authentication service reviewed
- [ ] Resources (styles, fonts, images) reviewed

### Phase 5: Tests
- [ ] API unit tests reviewed
- [ ] Service unit tests reviewed
- [ ] Model tests reviewed
- [ ] Integration tests reviewed
- [ ] Test coverage assessed

### Phase 6: Platform-Specific
- [ ] Android platform code reviewed
- [ ] iOS platform code reviewed
- [ ] macOS platform code reviewed
- [ ] Windows platform code reviewed
- [ ] Platform manifests reviewed

### Phase 7: Documentation
- [ ] README.md reviewed
- [ ] AGENTS.md reviewed
- [ ] Prompt files reviewed

---

## Key Questions to Answer During Review

### Architecture & Design
1. Is the separation of concerns clear (API, Models, App)?
2. Are dependencies properly injected?
3. Is the MVVM pattern correctly implemented?
4. Are domain models truly immutable?

### Security
1. Is password hashing secure (BCrypt)?
2. Is encryption properly implemented (AES)?
3. Are JWT tokens properly validated?
4. Are API endpoints properly secured with `[Authorize]`?
5. Is token storage secure in the MAUI app?

### Code Quality
1. Are validation rules consistent between DTOs and factories?
2. Is error handling comprehensive?
3. Are async/await patterns used correctly?
4. Are nullable reference types used appropriately?
5. Is the code following C# 14 best practices?

### Testing
1. Is test coverage adequate?
2. Are unit tests properly isolated?
3. Are integration tests comprehensive?
4. Are edge cases tested?

### MAUI-Specific
1. Are ViewModels properly decoupled from Views?
2. Is data binding correctly implemented?
3. Are platform-specific requirements handled?
4. Is navigation properly implemented?

---

## Common Issues to Look For

### API Layer
- Missing validation
- Inconsistent error responses
- Missing authorization checks
- Hardcoded configuration values
- Missing null checks

### MAUI Layer
- Memory leaks (event handlers not unsubscribed)
- Missing error handling in ViewModels
- Hardcoded API URLs
- Missing loading states
- Poor error user feedback

### Domain Layer
- Inconsistent validation between DTOs and factories
- Missing null checks
- Incorrect use of `TimeProvider`
- Missing edge case handling

### Testing
- Missing test coverage
- Tests that don't actually test behavior
- Missing integration tests
- Tests that are too coupled to implementation

---

## Next Steps After Review

1. **Build Verification**: Run `dotnet build` to check for compilation errors
2. **Format Check**: Run `dotnet format --verify-no-changes` to ensure code formatting
3. **Test Execution**: Run `dotnet test` to verify all tests pass
4. **Code Analysis**: Review any analyzer warnings or suggestions
5. **Documentation Updates**: Update documentation if issues are found
6. **Refactoring**: Address any issues or improvements identified during review

---

## MAUI Learning Resources

If you're new to MAUI, here are key concepts to understand:

1. **XAML**: XML-based UI markup (similar to WPF/UWP)
2. **Data Binding**: `{Binding PropertyName}` syntax in XAML
3. **MVVM**: Model-View-ViewModel pattern for separation of concerns
4. **Dependency Injection**: Same patterns as ASP.NET Core
5. **Shell Navigation**: MAUI's navigation system
6. **Platform-Specific Code**: Code in `Platforms/` folders runs only on that platform
7. **Resources**: Styles, fonts, images defined in `Resources/` folder
8. **CommunityToolkit.Mvvm**: Provides MVVM helpers (`[ObservableProperty]`, `[RelayCommand]`)

---

## Summary

This review plan covers **every file** in the Crow repository, organized in a logical progression from foundation to application layers. The review is designed to take approximately **2.5 hours** for a thorough examination.

**Key Takeaways**:
- The project follows clean architecture with clear separation of concerns
- Domain models are immutable records using C# 14 features
- API uses standard ASP.NET Core patterns
- MAUI app uses MVVM pattern with CommunityToolkit.Mvvm
- Comprehensive test coverage across unit and integration tests
- Platform-specific code is isolated in `Platforms/` folders

Follow the phases sequentially, and use the checklist to ensure nothing is missed. Take notes on any issues or improvements you identify during the review.

