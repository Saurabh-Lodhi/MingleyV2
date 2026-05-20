# Mingley API — Developer Guide

## 🌐 Live API
```
https://mingley-backend-v2.onrender.com
```

## 📖 Swagger UI
```
https://mingley-backend-v2.onrender.com/swagger/index.html
```

## 🖥️ Admin Panel
```
https://mingley-backend-v2.onrender.com/admin/index.html
```
- **Email:** `admin@mingley.app`
- **Password:** `Mingley@123`

---

## 🔑 Test Accounts

> **Universal Password for all accounts:** `Mingley@123`

### 👨 Male Users
| Name | Email | Coins | Premium |
|---|---|---|---|
| Arjun Singh | `arjun@demo.com` | 10,000 | ✅ Gold |
| Rahul Mehta | `rahul@demo.com` | 5,000 | ❌ |
| Vikram Nair | `vikram@demo.com` | 3,000 | ❌ |
| Deepak Verma | `deepak@demo.com` | 500 | ❌ |
| Aman Joshi | `aman@demo.com` | 6,000 | ✅ Gold |

### 👩 Female Users
| Name | Email | Coins | Premium | Online |
|---|---|---|---|---|
| Priya Sharma | `priya@demo.com` | 2,500 | ✅ Gold | 🟢 |
| Shreya Patel | `shreya@demo.com` | 3,500 | ✅ Gold | 🟢 |
| Neha Kapoor | `neha@demo.com` | 800 | ❌ | 🟢 |
| Aisha Khan | `aisha@demo.com` | 1,800 | ❌ | 🟢 |
| Pooja Gupta | `pooja@demo.com` | 950 | ❌ | 🟢 |

---

## 🔐 Authentication

All protected endpoints require JWT Bearer token in header:
```
Authorization: Bearer <token>
```

### Register
```http
POST /v1/auth/register
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phone": "9999999999",
  "password": "Password@123",
  "gender": "male",
  "dateOfBirth": "1995-01-01T00:00:00Z"
}
```

### Verify OTP
```http
POST /v1/auth/verify-otp
Content-Type: application/json

{
  "userId": "<userId from register>",
  "otp": "123456",
  "purpose": "registration"
}
```

### Login
```http
POST /v1/auth/login
Content-Type: application/json

{
  "identifier": "arjun@demo.com",
  "password": "Mingley@123",
  "fcmToken": ""
}
```
**Response:**
```json
{
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "...",
    "user": { "id": "...", "fullName": "Arjun Singh", ... }
  }
}
```

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/v1/auth/register` | Register new user |
| POST | `/v1/auth/verify-otp` | Verify OTP |
| POST | `/v1/auth/resend-otp` | Resend OTP |
| POST | `/v1/auth/login` | Login |
| POST | `/v1/auth/refresh` | Refresh token |
| POST | `/v1/auth/logout` | Logout |

### Users
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/users/me` | Get my profile |
| PUT | `/v1/users/me` | Update profile |
| PUT | `/v1/users/me/interests` | Update interests |
| PUT | `/v1/users/me/preferences` | Update preferences |
| POST | `/v1/users/me/images` | Add image |
| DELETE | `/v1/users/me/images/{id}` | Delete image |
| POST | `/v1/users/{id}/block` | Block user |
| POST | `/v1/users/{id}/report` | Report user |

### Discover
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/discover` | Get discover feed |
| POST | `/v1/discover/swipe` | Swipe (like/pass) |
| GET | `/v1/matches` | Get matches |

**Discover Query Params:**
```
?page=1&limit=20&interestedIn=girls&distance=50&ageRange[]=18&ageRange[]=40&onlineStatus=false&verifiedOnly=false
```

**Swipe Body:**
```json
{ "targetId": "<userId>", "action": "like" }
```
Actions: `like`, `pass`, `superlike`

### Chat
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/chats` | Get all chats |
| GET | `/v1/chats/{chatId}/messages` | Get messages |
| POST | `/v1/chats/{chatId}/messages` | Send message |
| PUT | `/v1/chats/{chatId}/read` | Mark as read |

**Send Message Body:**
```json
{
  "content": "Hello!",
  "messageType": "TEXT"
}
```

### Calls
| Method | Endpoint | Description |
|---|---|---|
| POST | `/v1/calls/initiate` | Start a call |
| POST | `/v1/calls/{id}/answer` | Answer call |
| POST | `/v1/calls/{id}/decline` | Decline call |
| POST | `/v1/calls/{id}/end` | End call |
| GET | `/v1/calls/{id}/agora-token` | Get Agora token |
| GET | `/v1/calls/history` | Call history |

**Initiate Call Body:**
```json
{ "targetId": "<userId>", "callType": "audio" }
```
Call types: `audio`, `video`

### Wallet
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/wallet/balance` | Get coin balance |
| GET | `/v1/wallet/packages` | Get coin packages |
| GET | `/v1/wallet/transactions` | Get transactions |
| POST | `/v1/wallet/razorpay/order` | Create Razorpay order |
| POST | `/v1/wallet/razorpay/verify` | Verify payment |
| POST | `/v1/wallet/deposit` | Request deposit |
| POST | `/v1/wallet/withdraw` | Request withdrawal |

### Subscriptions
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/subscriptions/plans` | Get plans |
| GET | `/v1/subscriptions/status` | Get my subscription |
| POST | `/v1/subscriptions/subscribe` | Subscribe to plan |

### Gifts
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/gifts/catalog` | Get gift catalog |
| POST | `/v1/gifts/send` | Send a gift |

### Notifications
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/notifications` | Get notifications |
| GET | `/v1/notifications/unread-count` | Unread count |
| PUT | `/v1/notifications/{id}/read` | Mark read |
| PUT | `/v1/notifications/read-all` | Mark all read |

### Interests
| Method | Endpoint | Description |
|---|---|---|
| GET | `/v1/interests` | Get all interests |

---

## 🔌 SignalR (Real-time)

**Hub URLs:**
```
wss://mingley-backend-v2.onrender.com/hubs/chat
wss://mingley-backend-v2.onrender.com/hubs/notifications
```

Pass JWT token as query param:
```
/hubs/chat?access_token=<token>
```

**Chat Hub Events:**
| Event | Direction | Description |
|---|---|---|
| `ReceiveMessage` | Server → Client | New message received |
| `UserTyping` | Server → Client | User is typing |
| `UserStoppedTyping` | Server → Client | User stopped typing |
| `UserOnline` | Server → Client | User came online |
| `UserOffline` | Server → Client | User went offline |
| `IncomingCall` | Server → Client | Incoming call |
| `CallAnswered` | Server → Client | Call answered |
| `CallDeclined` | Server → Client | Call declined |
| `CallEnded` | Server → Client | Call ended |

**Notification Hub Events:**
| Event | Direction | Description |
|---|---|---|
| `NewMatch` | Server → Client | New match |
| `NewNotification` | Server → Client | New notification |

---

## 💰 Subscription Plans

| Plan | Price | Duration | Video Calls |
|---|---|---|---|
| Silver | ₹299 | 30 days | ❌ |
| Gold | ₹599 | 30 days | ✅ |
| Platinum | ₹999 | 30 days | ✅ |

---

## 🔧 Third-party Keys (Test Mode)

| Service | Key |
|---|---|
| Razorpay Key ID | `rzp_test_Sq4maCpZVgCTeM` |
| Agora App ID | `8592b7de7bec4f0a9b1ef2a0a79279f6` |

---

## ⚠️ Notes

- Free Render instance **sleeps after 15 min** of inactivity — first request takes ~50 seconds to wake up
- OTP in production requires SMS gateway (MSG91/Twilio) — use demo accounts for testing
- Video/Audio calls require native APK build — does not work in Expo Go
- All amounts in **paise** for Razorpay (₹1 = 100 paise)

---

## 📞 Quick Test Flow

1. Login as `arjun@demo.com` / `Mingley@123` → get token
2. Call `GET /v1/discover` to see female profiles
3. Call `POST /v1/discover/swipe` with `action: "like"`
4. Login as `priya@demo.com` on another device → swipe right on Arjun
5. Both get matched → chat unlocks
6. Send messages via `POST /v1/chats/{chatId}/messages`
