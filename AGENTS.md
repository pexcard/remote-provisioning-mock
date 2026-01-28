# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Remote Provisioning Service Mock is a mock implementation of the PEX Remote Token Provisioning API. It allows testers and developers to configure custom responses for token provisioning endpoints, simulate various scenarios (success, errors, delays), and audit all API requests for verification.

**Target Framework:** .NET 8.0
**Storage:** Azure Table Storage
**Purpose:** Testing and integration support for mobile wallet provisioning flows

## Build and Development Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Build for release
dotnet build -c Release

# Run the service locally
dotnet run --project RemoteProvisioningServiceMock/RemoteProvisioningServiceMock.csproj

# Run tests
dotnet test
```

### Local Development

The service uses Azure Storage Emulator by default (`UseDevelopmentStorage=true` in appsettings.json). Start the Azure Storage Emulator or Azurite before running.

- **HTTP:** http://localhost:54492
- **HTTPS:** https://localhost:54491
- **Swagger UI:** Available at root URL (http://localhost:54492/)

## Project Structure

```
RemoteProvisioningServiceMock/
├── Controllers/           # API endpoints
│   ├── TokenController.cs       # Core provisioning endpoints (/token/*)
│   ├── AuditController.cs       # Audit log queries (/audit/*)
│   └── SettingsController.cs    # Response configuration (/business/*/settings)
├── Models/               # DTOs and request/response models
│   ├── DecisionRequest.cs       # Token provisioning request
│   ├── DecisionResponse.cs      # Token provisioning response
│   ├── SendOtpRequest.cs        # OTP delivery request
│   ├── CallbackRequest.cs       # Callback notification
│   ├── ResponseSettingsModel.cs # API configuration
│   └── *Audit.cs                # Audit wrapper models
├── Storage/              # Data access layer
│   ├── Abstract/                # Base Azure Table Storage class
│   ├── Entity/                  # Table storage entities
│   ├── ResponseSettingsStorage.cs
│   ├── TokenProvisioningStorage.cs
│   ├── OtpCodeStorage.cs
│   └── CallbackStorage.cs
├── Extensions/           # Utility helpers
│   ├── JsonExtensions.cs        # JSON serialization
│   └── AzureTableStorageExtensions.cs
├── Program.cs            # Entry point
├── Startup.cs            # DI and middleware configuration
└── appsettings.json      # Configuration
```

## API Endpoints

### Token Provisioning (`/token`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/token/provisioning` | Get decision for step-up authentication flow |
| POST | `/token/otp-code` | Send OTP passcode for step-up flow |
| POST | `/token/callback` | Receive callbacks about token creation |

All `/token/provisioning` and `/token/otp-code` requests validate the `Authorization` header (Basic auth with shared secret).

### Settings Configuration (`/business/{businessId}/cardholder/{cardholderId}/settings`)

| Method | Description |
|--------|-------------|
| GET | Retrieve response settings for a cardholder |
| PUT | Configure mock response behavior |

**Configurable Settings:**
- `SharedSecret` - Expected Basic auth secret
- `StatusCode` - HTTP status code to return (200, 400, 401, 403, 404, 500)
- `TimeoutMls` - Response delay in milliseconds
- `Contacts` - Contact items for step-up flow responses

### Audit Logs (`/audit`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/audit/provisioning?cardholderId=X&from=Y&search=Z` | Query provisioning requests |
| GET | `/audit/otp-code?cardholderId=X&from=Y&search=Z` | Query OTP requests |
| GET | `/audit/callback?cardholderId=X&from=Y&search=Z` | Query callbacks |

## Architecture Patterns

### Storage Layer

Uses Azure Table Storage with the repository pattern:

- **ResponseSettingsStorage** - Configuration storage (PartitionKey: BusinessId, RowKey: CardholderId)
- **TokenProvisioningStorage** - Provisioning audit log
- **OtpCodeStorage** - OTP audit log
- **CallbackStorage** - Callback audit log

All storage classes extend `AzureTableStorageAbstract` which provides:
- Retry policy (LinearRetry, 500ms delay, 3 attempts)
- Pagination via continuation tokens
- DateTime validation for Azure Table limits

### Dependency Injection

Storage providers registered as singletons in `Startup.cs`:

```csharp
services.AddSingleton<ResponseSettingsStorage>();
services.AddSingleton<TokenProvisioningStorage>();
services.AddSingleton<OtpCodeStorage>();
services.AddSingleton<CallbackStorage>();
```

### JSON Serialization

Uses Newtonsoft.Json for entity serialization. PascalCase property naming configured in Startup.cs for API responses.

## Testing Scenarios

Configure mock responses via the Settings API to test:

1. **Success flows** - StatusCode: 200 with valid Contacts
2. **Authentication failures** - StatusCode: 401 (invalid secret)
3. **Authorization failures** - StatusCode: 403
4. **Not found** - StatusCode: 404
5. **Server errors** - StatusCode: 500
6. **Timeout simulation** - Set TimeoutMls to desired delay

## CI/CD Pipeline

### Build Pipeline (`service-build-pipeline.yml`)
- Trigger: Pull requests to main
- Steps: Build → Test → SonarCloud analysis → Mend vulnerability scan

### Publish Pipeline (`service-publish-pipeline.yml`)
- Trigger: Push to main branch
- Steps: Build → Test → Publish → SonarCloud → Mend → Artifact upload

Both pipelines use:
- Ubuntu-latest runner
- .NET 8.0 SDK
- SonarCloud for code quality
- Mend for dependency scanning

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Azure.Cosmos.Table | 1.0.8 | Azure Table Storage client |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI documentation |

## Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  }
}
```

For production, set `StorageConnectionString` to your Azure Storage account connection string.

## Common Development Tasks

### Adding a New Audit Table

1. Create entity class in `Storage/Entity/` extending `TableEntity`
2. Create storage class in `Storage/` extending `AzureTableStorageAbstract`
3. Register in `Startup.cs` as singleton
4. Add controller endpoint in `AuditController.cs`

### Modifying Response Behavior

1. Update `ResponseSettingsModel` and `ResponseSettingsEntity` with new fields
2. Update `ResponseSettingsStorage` for persistence
3. Apply new settings in `TokenController` endpoints

### Testing Locally

1. Start Azure Storage Emulator or Azurite
2. Run the service: `dotnet run --project RemoteProvisioningServiceMock`
3. Configure test settings via PUT to `/business/{bid}/cardholder/{cid}/settings`
4. Execute test requests against `/token/*` endpoints
5. Verify via `/audit/*` endpoints
