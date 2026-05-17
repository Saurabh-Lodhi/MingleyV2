# Mingley V3 — Fix Summary

## All Bugs Fixed

### 🔐 Authentication
| Bug | Fix |
|-----|-----|
| Login shows GUID error | `AuthController` now returns structured `{ requiresVerification: true, userId, devOtp }` on unverified login |
| OTP not visible on screen | OTP shown in large yellow card on-screen, not just Alert |
| Registration only took email | `EmailInputScreen` now requires **both email AND phone** with proper regex validation |
| No eye icon on password | Eye toggle added to all password fields |
| Phone signup option broken | `SignupOptionsScreen` phone button → same `EmailInput` form |

### 💳 Subscription & Payment
| Bug | Fix |
|-----|-----|
| 500 plan auto-selected | No auto-selection — user taps to select; Continue button disabled until selected |
| No Razorpay UI | `PaymentScreen` shows all methods: UPI, Card, Net Banking, Wallets, EMI |
| Razorpay backend | `/v1/wallet/razorpay/order` and `/v1/wallet/razorpay/verify` endpoints added |

### 📞 Calling
| Bug | Fix |
|-----|-----|
| Calls not working | `CallScreen` fully rewritten with Agora RTC — fetches token from `/v1/calls/{id}/agora-token` |
| Agora AppCertificate | Updated to correct Primary Certificate `ac6827bdc00a49b394b1dfa250c8409a` |
| Answer/Decline buttons | Properly call `callService.answer()` / `callService.decline()` |
| Video call | Camera toggling, full duplex video via Agora RTC |

### 💬 Chat
| Bug | Fix |
|-----|-----|
| Online status always offline | SignalR `UserOnlineStatus` properly received and applied in ChatScreen |
| Gifts reset after refresh | Gifts sent as chat messages via `sendMessage` so they persist in DB |
| Activity/image button broken | `handlePickImage()` uses `expo-image-picker` properly |
| Settings button not working | Settings modal opens with Mute/Block/Report/Clear options |
| Message badge count | Tab bar badge shows `getTotalUnread()` correctly |
| Auto-refresh loop | `ConversationsScreen` no longer calls `loadConversations` on every socket event |

### 🔍 Discover
| Bug | Fix |
|-----|-----|
| Blank screen when no more users | Feed loops infinitely using `currentIdx % totalFeed` |
| Filters not working | Filter modal → `DiscoverController` accepts `gender`, `minAge`, `maxAge`, `onlineOnly` params; `DiscoverService` applies them |

### 🎁 Gifts
| Bug | Fix |
|-----|-----|
| Coins deducted but gifts reset | Gift sends chat message (`type=gift`) persisted in DB |
| Gift emoji missing | `Emoji` field added to `Gift` entity and seeded |

### 💰 Admin / Deposits
| Bug | Fix |
|-----|-----|
| Approve/Reject not working | Admin panel `dep()` / `wd()` now sends `{ note }` body required by `[FromBody] AdminNoteRequest` |

### 🌐 API
| Bug | Fix |
|-----|-----|
| `BASE_URL` undefined | Fixed export in `api.js` |
| ChatDTO field mismatch | `content`, `messageType`, `createdAt`, `isRead` added to `ChatMessageDto` matching frontend |
| matchId vs chatId | `ChatService.GetChatsAsync` sets `MatchId = c.Id` so frontend uses consistent key |

## Setup

### Backend
```bash
cd Mingley.API
dotnet run
# API: http://localhost:7001
# Swagger: http://localhost:7001/swagger
# Admin: http://localhost:7001/admin/index.html  (or serve Mingley.Admin/index.html)
```

### Frontend
```bash
cd Mingley.Frontend
npm install
npx expo start
```

### Razorpay (Production)
1. Create account at razorpay.com
2. Get `KeyId` and `KeySecret` from Dashboard
3. Update `appsettings.json`:
   ```json
   "Razorpay": { "KeyId": "rzp_live_xxx", "KeySecret": "xxx" }
   ```
4. Install SDK: `dotnet add package Razorpay`
5. In `WalletController.CreateRazorpayOrder`: replace mock with `client.Order.Create(dict)`

### Agora (Already configured)
- AppId: `8592b7de7bec4f0a9b1ef2a0a79279f6`
- AppCertificate (Primary): `ac6827bdc00a49b394b1dfa250c8409a`
- Agora Chat AppKey: `61200024644#200032679`

### Admin Login
- Email: `admin@mingley.app`
- Password: `Mingley@123`

### Default Test User (pre-seeded)
- Email: `saurabh@gmail.com`  
- Password: `Mingley@123`
