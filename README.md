# 🌟 Mingley Dating App — Backend API

## Tech Stack
- **.NET 8** · Clean Architecture · PostgreSQL (Npgsql) · SignalR · JWT Auth
- React Native Frontend (Expo) · HTML Admin Panel

---

## 🚀 Quick Start

### 1. Prerequisites
- .NET 8 SDK
- PostgreSQL 14+ running locally

### 2. Configure Database
Edit `Mingley.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=MingleyDb;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 3. Run
```bash
cd Mingley.API
dotnet run
```
- API: http://localhost:7001
- Swagger: http://localhost:7001/swagger
- Admin Panel: http://localhost:7001/admin/index.html (copy Mingley.Admin/index.html → API/wwwroot/admin/)

---

## 🔐 Test Credentials (all use password: `Mingley@123`)

| User | Email | Role | Status | Coins |
|------|-------|------|--------|-------|
| Super Admin | admin@mingley.app | admin | Active | 9999 |
| Priya Sharma | priya@demo.com | user | Online, Gold, Matched w/ Rahul | 2500 |
| Rahul Mehta | rahul@demo.com | user | Matched w/ Priya | 5000 |
| Arjun Singh | arjun@demo.com | user | Gold, Matched w/ Aisha | 10000 |
| Aisha Khan | aisha@demo.com | user | Online, Matched w/ Arjun | 1800 |
| Neha Kapoor | neha@demo.com | user | Online | 800 |
| Vikram Nair | vikram@demo.com | user | Active | 3000 |
| Ankita Singh | ankita@demo.com | user | Active | 1200 |
| Deepak Verma | deepak@demo.com | user | Unverified | 500 |
| Rohit Sharma | rohit@demo.com | user | Active | 2000 |

### Pre-seeded Data
- **Match 1**: Rahul ↔ Priya (ID: `a1000001-0000-0000-0000-000000000001`)
- **Match 2**: Arjun ↔ Aisha (ID: `a1000001-0000-0000-0000-000000000002`)
- **Call History**: Arjun→Aisha video call (5 min, 500 coins) + Rahul→Priya audio call (3 min, 30 coins)
- **Pre-seeded messages** in both chats for testing

---

## 💰 Coin Economy
| Action | Cost |
|--------|------|
| Audio call | 10 coins/min |
| Video call | 100 coins/min (premium only) |
| Male sends message | 10 coins (5 with premium) |
| Female messages | 3 free, then 5 coins/msg |
| Super Like | 50 coins |
| SuperChat | 500 coins → girl gets 50% commission (₹25) on respond |
| Verification bonus | +50 coins |
| Welcome bonus | +100 coins |

---

## 📡 API Reference

### Auth
```
POST v1/auth/register          → register + get OTP (logged to console in dev)
POST v1/auth/verify-otp        → verify OTP → get JWT
POST v1/auth/login             → login → get JWT
POST v1/auth/refresh           → refresh JWT
POST v1/auth/logout            → logout
POST v1/auth/forgot-password   → OTP to console
POST v1/auth/reset-password    → reset with OTP
POST v1/auth/change-password   → change (requires auth)
GET  v1/auth/me                → get my profile
```

### Users (all require Bearer token)
```
GET    v1/users/me                          → my full profile
GET    v1/users/{id}                        → another user's profile
PUT    v1/users/me                          → update profile
PUT    v1/users/me/interests                → update interests
PUT    v1/users/me/preferences              → update match preferences
PUT    v1/users/me/location                 → update GPS location
POST   v1/users/me/images                   → add photo
DELETE v1/users/me/images/{imageId}         → delete photo
PUT    v1/users/me/images/reorder           → reorder photos
PUT    v1/users/me/images/{imageId}/primary → set primary photo
POST   v1/users/{id}/block                  → block user
DELETE v1/users/{id}/block                  → unblock user
GET    v1/users/blocked                     → blocked list
POST   v1/users/{id}/report                 → report user
DELETE v1/users/me/account                  → delete account
```

### Discover
```
GET  v1/discover              → feed (page, limit params)
POST v1/discover/swipe        → swipe { targetId, action: like|dislike|superlike }
GET  v1/matches               → all matches
DELETE v1/matches/{matchId}   → unmatch
GET  v1/matches/likes         → who liked me (premium only)
```

### Chat
```
GET    v1/chats                              → all chat list
GET    v1/chats/{chatId}/messages            → messages (page param)
POST   v1/chats/{chatId}/messages            → send message
PUT    v1/chats/{chatId}/read                → mark read
DELETE v1/chats/{chatId}/messages/{msgId}    → delete message
GET    v1/chats/{chatId}/quota               → coin quota info
```

### Calls
```
POST v1/calls/initiate            → { targetId, callType: audio|video }
POST v1/calls/{callId}/answer     → answer
POST v1/calls/{callId}/end        → end (deducts coins)
POST v1/calls/{callId}/decline    → decline
GET  v1/calls/history             → call history
```

### SuperChat
```
POST v1/superchat/send           → { toUserId, message } — costs 500 coins
POST v1/superchat/{id}/respond   → girl responds → match created + commission
GET  v1/superchat/received       → received superchats
GET  v1/superchat/sent           → sent superchats
```

### Wallet
```
GET  v1/wallet/balance       → coin balance + total earned
GET  v1/wallet/packages      → coin packages
GET  v1/wallet/transactions  → transaction history (type: all|credit|debit)
POST v1/wallet/deposit       → submit deposit request (UTR)
POST v1/wallet/withdraw      → withdrawal request (female only)
```

### Subscriptions
```
GET  v1/subscriptions/plans            → all plans
GET  v1/subscriptions/status           → my subscription
POST v1/subscriptions/subscribe        → { planId, autoRenew }
POST v1/subscriptions/{id}/cancel      → cancel
```

### Notifications
```
GET v1/notifications            → list (page param)
GET v1/notifications/unread-count
PUT v1/notifications/{id}/read  → mark one read
PUT v1/notifications/read-all   → mark all read
```

### Other
```
GET  v1/interests               → interest catalog
POST v1/verify                  → verify profile (+50 coins)
GET  v1/privacy/policy          → privacy policy text
POST v1/privacy/accept/{matchId}→ accept privacy popup
GET  v1/gifts/catalog           → gift catalog
POST v1/gifts/send              → send gift { recipientId, giftId, chatId? }
```

### Admin (role: admin required)
```
GET  v1/admin/dashboard
GET  v1/admin/users                        → search/filter/page
GET  v1/admin/users/{id}                   → full user detail + activity
POST v1/admin/users/create
PUT  v1/admin/users/{id}/toggle-status     → suspend/activate
POST v1/admin/users/{id}/grant-subscription→ { planId, days }
POST v1/admin/users/{id}/add-coins         → { coins, note }
GET  v1/admin/users/{id}/chats
GET  v1/admin/users/{id}/messages
GET  v1/admin/deposits
POST v1/admin/deposits/{id}/approve
POST v1/admin/deposits/{id}/reject
GET  v1/admin/withdrawals
POST v1/admin/withdrawals/{id}/approve
POST v1/admin/withdrawals/{id}/reject
GET  v1/admin/reports
POST v1/admin/reports/{id}/resolve
GET  v1/admin/superchat
GET  v1/admin/stats/coins
```

---

## ⚡ SignalR Events

### Connect (pass JWT as ?access_token=... query param)
- Hub: `wss://host/hubs/chat` — main hub (chat + calls + notifications)
- Hub: `wss://host/hubs/notifications` — notifications only

### Server → Client Events
| Event | When |
|-------|------|
| `NewMatch` | Mutual like → match created |
| `Unmatched` | Someone unmatched |
| `UserOnlineStatus` | User online/offline |
| `NewMessage` | Message sent in chat |
| `MessagesRead` | Messages marked read |
| `MessageDeleted` | Message deleted |
| `Typing` | Typing indicator |
| `IncomingCall` | Someone calling you |
| `CallAnswered` | Call accepted |
| `CallDeclined` | Call declined |
| `CallEnded` | Call ended + duration + coins |
| `CallSignal` | WebRTC signal relay |
| `NewSuperChat` | SuperChat received (girl) |
| `SuperChatResponded` | Girl responded → match created |
| `NewNotification` | Any notification |

### Client → Server (Hub methods)
```js
JoinChat(chatId)           // join a chat room (call when opening chat)
LeaveChat(chatId)          // leave chat room
Typing(chatId, isTyping)   // send typing indicator
CallSignal(targetUserId, signalType, signalData)  // WebRTC relay
```
