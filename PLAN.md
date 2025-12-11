# HairCut History App - Implementation Plan

## Overview
A .NET MAUI mobile application (iOS + Android) that helps users remember their haircut settings and share them with barbers/stylists via QR codes. Features cloud sync via Firebase and dual modes (Client/Barber).

## Architecture

### Tech Stack
- **Frontend**: .NET MAUI 8.0 (C#, XAML)
- **Backend**: Firebase (Authentication, Firestore, Storage)
- **QR Code**: ZXing.Net.Maui for generation/scanning
- **Images**: Firebase Storage for cloud sync
- **Architecture Pattern**: MVVM with CommunityToolkit.Mvvm

### Project Structure
```
HaircutHistoryApp/
├── HaircutHistoryApp.sln
├── src/
│   └── HaircutHistoryApp/
│       ├── App.xaml(.cs)
│       ├── MauiProgram.cs
│       ├── AppShell.xaml(.cs)
│       ├── Models/
│       │   ├── User.cs
│       │   ├── HaircutProfile.cs
│       │   ├── HaircutMeasurement.cs
│       │   ├── BarberNote.cs
│       │   └── ShareSession.cs
│       ├── ViewModels/
│       │   ├── BaseViewModel.cs
│       │   ├── LoginViewModel.cs
│       │   ├── ProfileListViewModel.cs
│       │   ├── ProfileDetailViewModel.cs
│       │   ├── AddEditProfileViewModel.cs
│       │   ├── QRShareViewModel.cs
│       │   ├── QRScanViewModel.cs
│       │   ├── BarberDashboardViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Views/
│       │   ├── LoginPage.xaml(.cs)
│       │   ├── RegisterPage.xaml(.cs)
│       │   ├── ProfileListPage.xaml(.cs)
│       │   ├── ProfileDetailPage.xaml(.cs)
│       │   ├── AddEditProfilePage.xaml(.cs)
│       │   ├── QRSharePage.xaml(.cs)
│       │   ├── QRScanPage.xaml(.cs)
│       │   ├── BarberDashboardPage.xaml(.cs)
│       │   ├── ClientViewPage.xaml(.cs)
│       │   └── SettingsPage.xaml(.cs)
│       ├── Services/
│       │   ├── IAuthService.cs
│       │   ├── FirebaseAuthService.cs
│       │   ├── IDataService.cs
│       │   ├── FirebaseDataService.cs
│       │   ├── IImageService.cs
│       │   ├── ImageService.cs
│       │   ├── IQRService.cs
│       │   └── QRService.cs
│       ├── Controls/
│       │   ├── MeasurementCard.xaml(.cs)
│       │   └── ImageGallery.xaml(.cs)
│       ├── Converters/
│       │   └── Various converters
│       ├── Resources/
│       │   ├── Styles/
│       │   ├── Fonts/
│       │   └── Images/
│       └── Platforms/
│           ├── Android/
│           └── iOS/
```

## Data Models

### User
```csharp
public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public UserMode Mode { get; set; } // Client or Barber
    public string? ShopName { get; set; } // For barbers
    public DateTime CreatedAt { get; set; }
}

public enum UserMode { Client, Barber }
```

### HaircutProfile
```csharp
public class HaircutProfile
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; } // "My Regular Cut", "Summer Style"
    public List<HaircutMeasurement> Measurements { get; set; }
    public List<string> ImageUrls { get; set; }
    public string GeneralNotes { get; set; }
    public List<BarberNote> BarberNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### HaircutMeasurement
```csharp
public class HaircutMeasurement
{
    public string Area { get; set; } // "Top", "Sides", "Back", "Neckline", etc.
    public string GuardSize { get; set; } // "2", "3", "Scissors"
    public string Technique { get; set; } // "Fade", "Taper", "Blended"
    public string Notes { get; set; }
}
```

### BarberNote
```csharp
public class BarberNote
{
    public string Id { get; set; }
    public string BarberId { get; set; }
    public string BarberName { get; set; }
    public string ShopName { get; set; }
    public string Note { get; set; } // "Left ear has cowlick, motion down"
    public DateTime CreatedAt { get; set; }
}
```

### ShareSession
```csharp
public class ShareSession
{
    public string Id { get; set; } // Short code for QR
    public string ProfileId { get; set; }
    public string ClientUserId { get; set; }
    public DateTime ExpiresAt { get; set; } // 24 hours
    public bool AllowBarberNotes { get; set; }
}
```

## Features by Screen

### 1. Authentication (LoginPage, RegisterPage)
- Email/password sign up and login via Firebase Auth
- Choose mode: Client or Barber during registration
- Barbers can optionally add shop name

### 2. Client Mode - Profile List (ProfileListPage)
- View all saved haircut profiles
- Add new profile
- Quick share button for each profile
- Search/filter profiles

### 3. Client Mode - Profile Detail (ProfileDetailPage)
- View all measurements in organized cards
- Image gallery with reference photos
- View barber notes from past visits
- Edit and Share buttons

### 4. Client Mode - Add/Edit Profile (AddEditProfilePage)
- Profile name
- Measurement sections:
  - Top (length, technique)
  - Sides (guard size, fade type)
  - Back (guard size, taper style)
  - Neckline (blocked, rounded, tapered)
  - Sideburns
  - Beard (if applicable)
- Photo picker for reference images
- General notes field

### 5. QR Share (QRSharePage)
- Generate QR code containing share session ID
- QR code expires in 24 hours
- Toggle: Allow barber to add notes
- Display code for manual entry

### 6. QR Scan (QRScanPage)
- Camera-based QR scanner
- Manual code entry option
- Opens client view after successful scan

### 7. Barber Mode - Dashboard (BarberDashboardPage)
- Scan QR button (prominent)
- Recent clients viewed
- Quick access to add notes

### 8. Barber Mode - Client View (ClientViewPage)
- Read-only view of client's profile
- All measurements displayed
- Reference images
- Previous barber notes visible
- "Add My Notes" button
- Notes input with save to client's profile

### 9. Settings (SettingsPage)
- Switch between Client/Barber mode
- Account management
- App info

## QR Code Flow

1. **Client generates QR**:
   - Creates ShareSession in Firestore with unique short ID
   - QR contains: `haircut://share/{sessionId}`
   - Session valid for 24 hours

2. **Barber scans QR**:
   - App reads session ID from QR
   - Fetches ShareSession from Firestore
   - Validates not expired
   - Fetches associated HaircutProfile
   - Displays read-only view

3. **Barber adds notes**:
   - If AllowBarberNotes is true
   - Barber writes note
   - Note saved to client's profile (BarberNotes array)
   - Client sees note on next profile view

## Implementation Steps

### Phase 1: Project Setup
1. Create .NET MAUI solution
2. Add NuGet packages (Firebase, ZXing, CommunityToolkit)
3. Configure Firebase project
4. Set up project structure

### Phase 2: Core Infrastructure
5. Implement models
6. Create Firebase services (Auth, Firestore, Storage)
7. Set up dependency injection
8. Create base ViewModel

### Phase 3: Authentication
9. Build login/register pages
10. Implement Firebase authentication
11. User mode selection

### Phase 4: Client Features
12. Profile list page with data binding
13. Profile detail page
14. Add/Edit profile page with image picker
15. Local caching for offline support

### Phase 5: QR Sharing
16. Implement QR generation
17. Implement QR scanning
18. Share session management

### Phase 6: Barber Features
19. Barber dashboard
20. Client view page
21. Note-taking functionality

### Phase 7: Polish
22. UI/UX improvements
23. Error handling
24. Loading states
25. App icons and splash screens

## NuGet Packages Required
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
- Firebase.Auth (or Plugin.Firebase)
- FirebaseAdmin or REST API calls
- ZXing.Net.Maui
- Newtonsoft.Json

## Firebase Setup Required
1. Create Firebase project
2. Enable Authentication (Email/Password)
3. Create Firestore database
4. Set up Storage bucket
5. Download google-services.json (Android) and GoogleService-Info.plist (iOS)
6. Configure security rules

## Estimated File Count
- ~25 XAML files (pages + controls)
- ~25 C# code-behind/ViewModels
- ~10 Service/Model files
- ~5 Configuration files
- Total: ~65 files
