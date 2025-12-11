# Social Authentication Setup Guide

This guide walks you through setting up Google, Facebook, and Apple Sign-In for HairCut History.

## Overview

The app uses PlayFab as the backend, which handles the actual authentication. You need to:
1. Set up OAuth credentials with each provider (Google, Facebook, Apple)
2. Configure PlayFab to accept tokens from these providers
3. Update the app's `SocialAuthConfig.cs` with your credentials

---

## 1. Google Sign-In Setup

### Step 1: Create Google Cloud Project
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable the **Google+ API** (or People API)

### Step 2: Create OAuth 2.0 Credentials
1. Go to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth client ID**
3. Create credentials for each platform:

#### Android:
- Application type: **Android**
- Package name: `com.haircuthistory.app`
- SHA-1 fingerprint: Get from your keystore
  ```bash
  keytool -list -v -keystore ~/.android/debug.keystore -alias androiddebugkey
  ```

#### iOS:
- Application type: **iOS**
- Bundle ID: `com.haircuthistory.app`

#### Web (for PlayFab):
- Application type: **Web application**
- Authorized redirect URIs: Add your PlayFab callback URL

### Step 3: Configure PlayFab
1. Go to [PlayFab Game Manager](https://developer.playfab.com/)
2. Select your title (1F53F6)
3. Go to **Add-ons** > **Google**
4. Enter your **Google App ID** (OAuth Client ID for Web)
5. Enter your **Google App Secret** (Client Secret)
6. Save

### Step 4: Update App Config
Edit `src/HaircutHistoryApp/Services/SocialAuthConfig.cs`:
```csharp
public const string GoogleClientId = "YOUR_ANDROID_OR_IOS_CLIENT_ID.apps.googleusercontent.com";
```

---

## 2. Facebook Login Setup

### Step 1: Create Facebook App
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Click **My Apps** > **Create App**
3. Select **Consumer** or **None** type
4. Enter app name: "HairCut History"

### Step 2: Add Facebook Login
1. In your app dashboard, click **Add Product**
2. Find **Facebook Login** and click **Set Up**
3. Select **iOS** and **Android**

### Step 3: Configure Platforms

#### Android:
- Package Name: `com.haircuthistory.app`
- Default Activity Class: `com.haircuthistory.app.MainActivity`
- Key Hashes: Generate with:
  ```bash
  keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore | openssl sha1 -binary | openssl base64
  ```

#### iOS:
- Bundle ID: `com.haircuthistory.app`

### Step 4: Get App Credentials
1. Go to **Settings** > **Basic**
2. Note your **App ID** and **App Secret**

### Step 5: Configure PlayFab
1. Go to PlayFab Game Manager
2. Go to **Add-ons** > **Facebook**
3. Enter your **Facebook App ID**
4. Enter your **Facebook App Secret**
5. Save

### Step 6: Update App Config
```csharp
public const string FacebookAppId = "YOUR_FACEBOOK_APP_ID";
```

---

## 3. Apple Sign-In Setup (iOS only)

### Step 1: Enable Sign in with Apple
1. Go to [Apple Developer Portal](https://developer.apple.com/)
2. Go to **Certificates, Identifiers & Profiles**
3. Select your App ID
4. Enable **Sign in with Apple** capability

### Step 2: Create Services ID (for PlayFab)
1. Go to **Identifiers** > **+**
2. Select **Services IDs**
3. Register a new Services ID
4. Enable **Sign in with Apple**
5. Configure the web domain and return URLs

### Step 3: Create Key for Server-to-Server
1. Go to **Keys** > **+**
2. Create a new key with **Sign in with Apple** enabled
3. Download the key file (.p8)
4. Note the **Key ID**

### Step 4: Configure PlayFab
1. Go to PlayFab Game Manager
2. Go to **Add-ons** > **Apple**
3. Enter:
   - **App Bundle ID**: `com.haircuthistory.app`
   - **Team ID**: Your Apple Team ID
   - **Key ID**: From Step 3
   - **Private Key**: Contents of .p8 file
4. Save

### Step 5: Update Xcode Project
In your iOS project:
1. Add **Sign in with Apple** capability
2. Ensure entitlements are configured

---

## 4. Update Android Manifest

Add to `Platforms/Android/AndroidManifest.xml`:

```xml
<!-- For Google Sign-In -->
<queries>
    <intent>
        <action android:name="android.intent.action.VIEW" />
        <data android:scheme="https" />
    </intent>
</queries>

<!-- Deep link for OAuth callback -->
<activity android:name="Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity"
          android:exported="true">
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="com.haircuthistory.app" android:host="callback" />
    </intent-filter>
</activity>
```

---

## 5. Update iOS Info.plist

Add to `Platforms/iOS/Info.plist`:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>com.haircuthistory.app</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>com.haircuthistory.app</string>
            <!-- Facebook URL scheme -->
            <string>fbYOUR_FACEBOOK_APP_ID</string>
        </array>
    </dict>
</array>

<!-- Facebook -->
<key>FacebookAppID</key>
<string>YOUR_FACEBOOK_APP_ID</string>
<key>FacebookDisplayName</key>
<string>HairCut History</string>

<key>LSApplicationQueriesSchemes</key>
<array>
    <string>fbapi</string>
    <string>fb-messenger-share-api</string>
</array>
```

---

## Testing

### Test in Development
1. Use debug/development signing
2. Test on real devices (simulators may have limitations)
3. Check PlayFab logs for authentication errors

### Common Issues

**Google: "Sign-in failed"**
- Check SHA-1 fingerprint matches
- Ensure OAuth consent screen is configured
- Verify package name matches exactly

**Facebook: "App not set up"**
- Add your Facebook account as a tester
- App may be in development mode (only testers can use)

**Apple: "Invalid client"**
- Verify Services ID configuration
- Check return URL matches exactly
- Ensure key is correctly uploaded to PlayFab

---

## Security Notes

1. **Never commit secrets** - Keep App Secrets out of source control
2. **Use environment variables** - For CI/CD, use secrets management
3. **Rotate keys** - Periodically rotate API keys and secrets
4. **Monitor usage** - Check PlayFab and provider dashboards for anomalies
