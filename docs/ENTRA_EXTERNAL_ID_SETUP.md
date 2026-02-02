# Microsoft Entra External ID Setup Guide

This guide walks you through setting up Microsoft Entra External ID for the HaircutHistory app.

## Overview

Microsoft Entra External ID (formerly Azure AD B2C) provides customer identity and access management (CIAM) for your app. It supports:
- Email/password sign-up and sign-in
- Social identity providers (Google, Apple, Facebook, etc.)
- Passwordless authentication
- Self-service password reset

## Step 1: Create an External Tenant

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Navigate to **Identity** → **Overview** → **Manage tenants**
3. Click **Create**
4. Select **External** tenant type, then **Continue**
5. Choose:
   - **30-day free trial** (no Azure subscription required), or
   - **Use Azure Subscription** for production
6. Fill in the **Basics** tab:
   - **Tenant Name**: `Haircut History` (display name)
   - **Domain Name**: `haircuthistory` (becomes `haircuthistory.onmicrosoft.com`)
   - **Country/Region**: Select your region (cannot be changed later)
7. If using Azure subscription, select your subscription and resource group
8. Click **Review + Create** → **Create**
9. Wait for deployment (can take up to 30 minutes)

## Step 2: Switch to Your External Tenant

1. After creation, click your profile in the top-right corner
2. Click **Switch directory**
3. Select your new external tenant

## Step 3: Get Your Tenant Information

1. In your external tenant, go to **Overview**
2. Note down:
   - **Tenant ID**: e.g., `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
   - **Primary domain**: e.g., `haircuthistory.onmicrosoft.com`
   - **Tenant subdomain**: e.g., `haircuthistory` (the part before `.onmicrosoft.com`)

## Step 4: Register the MAUI Application

1. Go to **Applications** → **App registrations**
2. Click **New registration**
3. Fill in:
   - **Name**: `HaircutHistory MAUI App`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: Leave blank for now (we'll add these next)
4. Click **Register**
5. **Copy the Application (client) ID** - you'll need this for configuration

### Add Redirect URIs

1. Go to **Authentication** in your app registration
2. Click **Add a platform**

#### For iOS:
- Select **iOS / macOS**
- Enter Bundle ID: `com.haircuthistory.app`
- Click **Configure**
- This creates: `msauth.com.haircuthistory.app://auth`

#### For Android:
- Select **Android**
- Enter Package name: `com.haircuthistory.app`
- Enter Signature hash: (see "Getting Android Signature Hash" below)
- Click **Configure**

#### For Windows:
- Select **Mobile and desktop applications**
- Add custom URI: `msal{YOUR_CLIENT_ID}://auth`
  (Replace `{YOUR_CLIENT_ID}` with your actual client ID)

### Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission** → **Microsoft Graph** → **Delegated permissions**
3. Select:
   - `openid` (Sign users in)
   - `offline_access` (Maintain access to data)
   - `profile` (View users' basic profile)
   - `email` (View users' email address)
4. Click **Add permissions**
5. Click **Grant admin consent for [tenant]**

## Step 5: Configure User Flows (Optional)

If you want to customize the sign-in experience:

1. Go to **External Identities** → **User flows**
2. Click **New user flow**
3. Select **Sign up and sign in**
4. Configure:
   - **Name**: e.g., `signupsignin`
   - **Identity providers**: Select which providers to enable
   - **User attributes**: Select what to collect during sign-up

## Step 6: Add Social Identity Providers (Optional)

### Google

1. Go to **External Identities** → **All identity providers**
2. Click **Google**
3. Get your credentials from [Google Cloud Console](https://console.cloud.google.com/):
   - Create OAuth 2.0 credentials
   - Add authorized redirect URI: `https://{tenant-subdomain}.ciamlogin.com/oauth2/authresp`
4. Enter **Client ID** and **Client secret**
5. Click **Save**

### Apple (for iOS)

1. Go to **External Identities** → **All identity providers**
2. Click **Apple**
3. Get your credentials from [Apple Developer](https://developer.apple.com/):
   - Create a Services ID
   - Create a private key
4. Enter your Apple credentials
5. Click **Save**

## Step 7: Update App Configuration

Update `src/HaircutHistoryApp/Services/AzureConfig.cs`:

```csharp
// Replace with your actual values from Steps 3 and 4
public const string TenantSubdomain = "haircuthistory";  // Your tenant subdomain
public const string TenantId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";  // Your tenant ID
public const string ClientId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";  // Your app client ID
public const string AndroidSignatureHash = "YOUR_SIGNATURE_HASH";  // From keytool
```

## Step 8: Platform-Specific Configuration

### Android

The `Platforms/Android/AndroidManifest.xml` already includes the MSAL activity. Make sure it has:

```xml
<activity android:name="microsoft.identity.client.BrowserTabActivity"
          android:exported="true">
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="msauth"
              android:host="com.haircuthistory.app" />
    </intent-filter>
</activity>
```

### iOS

The `Platforms/iOS/Info.plist` already includes the MSAL URL scheme:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>msauth.com.haircuthistory.app</string>
        </array>
    </dict>
</array>
```

## Getting Android Signature Hash

### Debug Keystore (Development)

```bash
# On Windows (Git Bash or WSL)
keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore | openssl sha1 -binary | openssl base64

# Password is: android
```

### Release Keystore (Production)

```bash
keytool -exportcert -alias YOUR_KEY_ALIAS -keystore /path/to/your.keystore | openssl sha1 -binary | openssl base64
```

## Step 9: Deploy Azure Functions

After Entra setup, configure and deploy your API:

### Configure Function App Settings

In Azure Portal, go to your Function App → Configuration → Application settings:

| Setting | Value |
|---------|-------|
| `CosmosDb__ConnectionString` | Your Cosmos DB connection string |
| `BlobStorage__ConnectionString` | Your Storage connection string |
| `Entra__TenantId` | Your external tenant ID |
| `Entra__ClientId` | Your app client ID |
| `Entra__Authority` | `https://{tenant-subdomain}.ciamlogin.com/` |

### Deploy

```bash
cd src/HaircutHistoryApp.Api
func azure functionapp publish haircuthistory-api
```

## Verification

1. Run the MAUI app
2. Tap "Sign In"
3. You should see the Entra sign-in page
4. Create an account or sign in with a social provider
5. After successful authentication, you return to the app

## Troubleshooting

### "AADSTS50011: The redirect URL does not match"
- Verify redirect URIs in app registration match exactly
- For Android, ensure the signature hash is correct
- Check that the scheme is `msauth` (not `msal` for Android)

### "AADSTS700016: Application not found"
- Verify the Client ID in `AzureConfig.cs` matches your app registration
- Ensure you're using the correct tenant

### Silent token acquisition fails
- Tokens may have expired; interactive sign-in will be triggered
- Check that `offline_access` scope is granted

### Social sign-in not showing
- Verify the identity provider is configured and enabled
- Check that the provider is added to your user flow (if using user flows)

## Additional Resources

- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/en-us/entra/external-id/)
- [MSAL.NET Documentation](https://learn.microsoft.com/en-us/entra/msal/dotnet/)
- [Sample Code on GitHub](https://github.com/Azure-Samples/ms-identity-ciam-dotnet-tutorial)
