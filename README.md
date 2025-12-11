# HairCut History App

A .NET MAUI mobile application for iOS and Android that helps users remember their haircut settings and share them with barbers/stylists via QR codes.

## Features

### Client Mode
- Create and manage multiple haircut profiles
- Store detailed measurements for each area (top, sides, back, neckline, etc.)
- Add reference photos from camera or gallery
- View notes left by barbers from previous visits
- Generate QR codes to share profiles with barbers (expires in 24 hours)
- Toggle whether barbers can add notes to your profile

### Barber/Stylist Mode
- Scan client QR codes to view their haircut preferences
- See all measurements and reference photos
- Add professional notes (e.g., "Left ear has cowlick, motion down")
- View recent clients for quick access

## Tech Stack

- **.NET MAUI 10** - Cross-platform mobile framework
- **C# / XAML** - Language and UI markup
- **CommunityToolkit.Mvvm** - MVVM pattern implementation
- **CommunityToolkit.Maui** - Additional MAUI controls and behaviors
- **ZXing.Net.Maui** - QR code generation and scanning
- **Newtonsoft.Json** - JSON serialization

## Project Structure

```
src/HaircutHistoryApp/
├── Models/                 # Data models
│   ├── User.cs
│   ├── HaircutProfile.cs
│   ├── HaircutMeasurement.cs
│   ├── BarberNote.cs
│   ├── ShareSession.cs
│   └── RecentClient.cs
├── ViewModels/            # MVVM ViewModels
│   ├── BaseViewModel.cs
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ProfileListViewModel.cs
│   ├── ProfileDetailViewModel.cs
│   ├── AddEditProfileViewModel.cs
│   ├── QRShareViewModel.cs
│   ├── QRScanViewModel.cs
│   ├── BarberDashboardViewModel.cs
│   ├── ClientViewViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ImageViewerViewModel.cs
├── Views/                 # XAML Pages
│   ├── LoginPage.xaml
│   ├── RegisterPage.xaml
│   ├── MainPage.xaml
│   ├── ProfileListPage.xaml
│   ├── ProfileDetailPage.xaml
│   ├── AddEditProfilePage.xaml
│   ├── QRSharePage.xaml
│   ├── QRScanPage.xaml
│   ├── BarberDashboardPage.xaml
│   ├── ClientViewPage.xaml
│   ├── SettingsPage.xaml
│   └── ImageViewerPage.xaml
├── Services/              # Business logic services
│   ├── IAuthService.cs
│   ├── LocalAuthService.cs
│   ├── IDataService.cs
│   ├── LocalDataService.cs
│   ├── IImageService.cs
│   ├── ImageService.cs
│   ├── IQRService.cs
│   └── QRService.cs
├── Converters/            # Value converters
│   └── Converters.cs
└── Resources/             # Styles, colors, fonts, images
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 (with MAUI workload) or VS Code with C# DevKit
- For iOS: macOS with Xcode
- For Android: Android SDK

### Build and Run

```bash
# Restore packages
dotnet restore

# Build for Android
dotnet build -f net10.0-android

# Build for iOS (requires macOS)
dotnet build -f net10.0-ios

# Run on Android emulator
dotnet run -f net10.0-android

# Run on iOS simulator (requires macOS)
dotnet run -f net10.0-ios
```

## How It Works

### QR Code Sharing Flow

1. **Client creates a profile** with measurements and photos
2. **Client generates a QR code** from the profile (valid for 24 hours)
3. **Barber scans the QR code** using the app's camera or enters the 8-character code manually
4. **Barber views the profile** with all measurements and reference photos
5. **Barber can add notes** (if client allowed it) that will be saved to the client's profile

### Data Storage

Currently uses local device storage with `Preferences` API. The architecture is designed to easily swap in cloud storage (Firebase, Azure, etc.) by implementing the service interfaces:

- `IAuthService` - Authentication
- `IDataService` - Profile and session data
- `IImageService` - Photo storage

## App Screenshots

The app features:
- Clean, modern UI with indigo/purple color scheme
- Card-based layouts for profiles and measurements
- Full-screen image viewer
- QR code display with share functionality
- Pull-to-refresh lists

## Future Enhancements

- [ ] Firebase/Azure cloud sync
- [ ] Push notifications for barber notes
- [ ] Appointment reminders integration
- [ ] Multiple profile photos per area
- [ ] Export profile as PDF
- [ ] Social sharing of styles

## License

MIT License - Feel free to use and modify for your own projects.
