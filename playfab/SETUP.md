# PlayFab Setup Guide for HaircutHistoryApp

## Title ID: 1F53F6

Follow these steps to configure your PlayFab backend.

---

## 1. Enable Authentication

1. Go to [PlayFab Game Manager](https://developer.playfab.com/)
2. Select your title **1F53F6**
3. Navigate to **Settings** (gear icon) → **API Features**
4. Enable **Allow client to post player statistics**
5. Navigate to **Add-ons** → **Authentication**
6. Enable **Email and password based authentication**

---

## 2. Create Player Statistics (Leaderboards)

Go to **Leaderboards** → **New Leaderboard** and create each:

| Statistic Name | Reset Frequency | Aggregation |
|----------------|-----------------|-------------|
| `haircuts_created` | Never | Last |
| `barber_visits` | Never | Last |
| `profiles_shared` | Never | Last |
| `clients_viewed` | Never | Last |
| `notes_added` | Never | Last |

---

## 3. Upload CloudScript

1. Go to **Automation** → **CloudScript** → **Revisions**
2. Click **Upload New Revision**
3. Upload the file: `playfab/CloudScript.js`
4. Click **Deploy Revision** to make it live

---

## 4. Configure Player Data Permissions

1. Go to **Settings** → **API Features**
2. Under **Client API Access**:
   - Enable **Allow client to post player statistics**
   - Enable **Allow clients to access player data**

---

## Achievement System

### Client Achievements (15 total)

**Haircut Profiles** (1, 5, 10, 25, 50, 100)
- HAIRCUT_1: First Cut
- HAIRCUT_5: Style Explorer
- HAIRCUT_10: Style Collector
- HAIRCUT_25: Style Enthusiast
- HAIRCUT_50: Style Master
- HAIRCUT_100: Style Legend

**Barber Visits** (1, 5, 10, 25, 50, 100)
- VISIT_1: Fresh Cut
- VISIT_5: Regular
- VISIT_10: Loyal Customer
- VISIT_25: VIP Client
- VISIT_50: Barbershop Regular
- VISIT_100: Lifetime Member

**Sharing** (1, 10, 50)
- SHARE_1: First Share
- SHARE_10: Connector
- SHARE_50: Networker

### Barber Achievements (7 total)

**Clients Viewed** (1, 10, 50, 100)
- CLIENT_1: First Client
- CLIENT_10: Growing Clientele
- CLIENT_50: Busy Barber
- CLIENT_100: Master Barber

**Notes Added** (1, 25, 100)
- NOTE_1: Helpful Tip
- NOTE_25: Note Taker
- NOTE_100: Detail Oriented

---

## How Achievements Work

1. **User creates a haircut profile** → `haircuts_created` stat increments
2. **User shares via QR code** → `profiles_shared` stat increments
3. **Barber scans and confirms** → `barber_visits` stat increments (for client)
4. **Barber views client profile** → `clients_viewed` stat increments
5. **Barber adds a note** → `notes_added` stat increments

When a stat hits a threshold (1, 5, 10, 25, 50, 100), the corresponding achievement unlocks automatically.

---

## Troubleshooting

### "Invalid Title ID"
- Verify Title ID is `1F53F6` in `PlayFabConfig.cs`

### "Statistics not updating"
- Enable "Allow client to post player statistics" in API Features

### CloudScript errors
- Check **Automation** → **CloudScript** → **Error Logs**

---

## Support

- [PlayFab Documentation](https://docs.microsoft.com/gaming/playfab/)
- [PlayFab Forums](https://community.playfab.com/)
