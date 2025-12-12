# Configuration Guide

This guide explains how to set up the required configuration files before building the app.

## Overview

For security reasons, files containing API keys and secrets are NOT committed to the repository. You must create these files locally before building.

## Required Configuration Files

### 1. Firebase Configuration (Android)

**File:** `src/HaircutHistoryApp/Platforms/Android/google-services.json`

**Setup:**
1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create or select your project
3. Add an Android app with package name: `com.haircuthistory.app`
4. Download `google-services.json`
5. Place it in `src/HaircutHistoryApp/Platforms/Android/`

**Template:** See `google-services.json.template` in the same directory.

### 2. Firebase Configuration (iOS)

**File:** `src/HaircutHistoryApp/Platforms/iOS/GoogleService-Info.plist`

**Setup:**
1. In Firebase Console, add an iOS app with bundle ID: `com.haircuthistory.app`
2. Download `GoogleService-Info.plist`
3. Place it in `src/HaircutHistoryApp/Platforms/iOS/`

### 3. Azure Storage Configuration

**File:** `src/HaircutHistoryApp/Services/AzureStorageConfig.cs`

**Setup:**
1. Copy `AzureStorageConfig.cs.template` to `AzureStorageConfig.cs`
2. Create an Azure Storage Account in [Azure Portal](https://portal.azure.com/)
3. Go to Storage Account > Access keys
4. Copy the connection string
5. Replace `YOUR_AZURE_STORAGE_CONNECTION_STRING` with your connection string
6. Create a blob container named `haircut-images`

**Note:** For production, use Azure Key Vault instead of hardcoding the connection string.

### 4. Social Authentication Configuration

**File:** `src/HaircutHistoryApp/Services/SocialAuthConfig.cs`

**Setup:**
1. Copy `SocialAuthConfig.cs.template` to `SocialAuthConfig.cs`
2. Configure each provider:

#### Google Sign-In
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create OAuth 2.0 credentials
3. Set the authorized redirect URI
4. Copy the Client ID

#### Facebook Login
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create an app
3. Add Facebook Login product
4. Copy the App ID

#### Apple Sign-In
1. Go to [Apple Developer Portal](https://developer.apple.com/)
2. Create a Services ID
3. Enable Sign in with Apple
4. Copy the Services ID

See `docs/SOCIAL_AUTH_SETUP.md` for detailed instructions.

## PlayFab Configuration

PlayFab is configured in `src/HaircutHistoryApp/Services/PlayFabService.cs`:

```csharp
private const string TitleId = "1F53F6";
```

To use your own PlayFab title:
1. Create an account at [PlayFab](https://developer.playfab.com/)
2. Create a new title
3. Update the `TitleId` constant

See `playfab/SETUP.md` for backend configuration.

## Environment-Specific Configuration

For CI/CD pipelines, use GitHub Secrets:

| Secret Name | Description |
|-------------|-------------|
| `FIREBASE_ANDROID_CONFIG` | Contents of google-services.json |
| `FIREBASE_IOS_CONFIG` | Contents of GoogleService-Info.plist |
| `AZURE_STORAGE_CONNECTION` | Azure Storage connection string |
| `GOOGLE_CLIENT_ID` | Google OAuth Client ID |
| `FACEBOOK_APP_ID` | Facebook App ID |
| `APPLE_SERVICES_ID` | Apple Services ID |

## Security Best Practices

1. **Never commit secrets** - All config files with real values are in `.gitignore`
2. **Rotate compromised keys** - If a key is accidentally committed, rotate it immediately
3. **Use Key Vault** - In production, use Azure Key Vault or similar secret management
4. **Restrict API keys** - Configure API key restrictions in each provider's console
5. **Review access logs** - Monitor for unauthorized API key usage

## Verification

After configuration, verify your setup:

```bash
# Build the project
dotnet build src/HaircutHistoryApp/HaircutHistoryApp.csproj

# Run tests
dotnet test tests/HaircutHistoryApp.Tests/HaircutHistoryApp.Tests.csproj
```

If the build succeeds, your configuration is correct.

## Troubleshooting

### "google-services.json not found"
- Ensure the file is in `Platforms/Android/` directory
- Check file name is exactly `google-services.json` (case-sensitive)

### "Azure Storage connection failed"
- Verify connection string format
- Check storage account firewall settings
- Ensure container exists

### "Social login not working"
- Verify OAuth credentials match platform (Android vs iOS)
- Check redirect URI configuration
- Ensure PlayFab add-ons are configured

## Questions?

If you encounter issues, check:
- Firebase Console for Android/iOS app configuration
- Azure Portal for storage account status
- PlayFab Game Manager for backend setup
