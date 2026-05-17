import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { Platform } from 'react-native';

// ─── BASE URL ─────────────────────────────────────────────────────────────────
// Web browser      → localhost:7001
// Android Emulator → 10.0.2.2:7001
// Physical device  → change to your LAN IP below
export const BASE_URL = Platform.OS === 'android'
  ? 'http://10.0.2.2:7001'
  : 'http://localhost:7001';

const api = axios.create({
  baseURL: `${BASE_URL}/v1`,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

// ── Attach JWT to every request ───────────────────────────────────────────────
api.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ── Auto-refresh on 401 ───────────────────────────────────────────────────────
api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const orig = err.config;
    if (err.response?.status === 401 && !orig._retry) {
      orig._retry = true;
      try {
        const rt = await AsyncStorage.getItem('refreshToken');
        if (!rt) throw new Error('no refresh token');
        const { data } = await axios.post(`${BASE_URL}/v1/auth/refresh`, { refreshToken: rt });
        const newToken = data.data.accessToken;
        await AsyncStorage.setItem('accessToken', newToken);
        orig.headers.Authorization = `Bearer ${newToken}`;
        return api(orig);
      } catch {
        await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'userId']);
      }
    }
    return Promise.reject(err);
  }
);

// ════════════════════════════════════════════════════════════
// AUTH  →  POST /v1/auth/*
// ════════════════════════════════════════════════════════════
export const authService = {
  register:        (data)                   => api.post('/auth/register', data),
  verifyOtp:       (userId, otp, purpose)   => api.post('/auth/verify-otp', { userId, otp, purpose: purpose || 'registration' }),
  resendOtp:       (userId, purpose)        => api.post('/auth/resend-otp', { userId, purpose: purpose || 'registration' }),
  login:           (identifier, password, fcmToken) => api.post('/auth/login', { identifier, password, fcmToken }),
  refresh:         (refreshToken)           => api.post('/auth/refresh', { refreshToken }),
  forgotPassword:  (identifier)             => api.post('/auth/forgot-password', { identifier }),
  resetPassword:   (data)                   => api.post('/auth/reset-password', data),
  changePassword:  (data)                   => api.post('/auth/change-password', data),
  getMe:           ()                       => api.get('/auth/me'),
  logout:          ()                       => api.post('/auth/logout'),
};

// ════════════════════════════════════════════════════════════
// USERS  →  /v1/users/*
// ════════════════════════════════════════════════════════════
export const userService = {
  getMe:            ()                       => api.get('/users/me'),
  getUser:          (id)                     => api.get(`/users/${id}`),
  updateProfile:    (data)                   => api.put('/users/me', data),
  updateInterests:  (interests)              => api.put('/users/me/interests', { interests }),
  updatePreferences:(data)                   => api.put('/users/me/preferences', data),
  updateLocation:   (data)                   => api.put('/users/me/location', data),
  addImage:         (url, isPrimary)         => api.post('/users/me/images', { url, isPrimary: isPrimary || false }),
  deleteImage:      (imageId)                => api.delete(`/users/me/images/${imageId}`),
  reorderImages:    (order)                  => api.put('/users/me/images/reorder', { order }),
  setPrimaryImage:  (imageId)                => api.put(`/users/me/images/${imageId}/primary`),
  block:            (id)                     => api.post(`/users/${id}/block`),
  unblock:          (id)                     => api.delete(`/users/${id}/block`),
  getBlocked:       ()                       => api.get('/users/blocked'),
  report:           (id, reason, desc)       => api.post(`/users/${id}/report`, { reason, description: desc }),
  deleteAccount:    (password, reason)       => api.delete('/users/me/account', { data: { password, reason } }),
};

// ════════════════════════════════════════════════════════════
// DISCOVER  →  /v1/discover  /v1/matches
// ════════════════════════════════════════════════════════════
export const discoverService = {
  getFeed:    (page, limit, filters)  => api.get(`/discover?page=${page||1}&limit=${limit||20}${filters ? '&'+new URLSearchParams(filters).toString() : ''}`),
  swipe:      (targetId, action)      => api.post('/discover/swipe', { targetId, action }),
  getMatches: (page)                  => api.get(`/matches?page=${page||1}`),
  unmatch:    (matchId)               => api.delete(`/matches/${matchId}`),
  getLikes:   ()                      => api.get('/discover/likes'),
};

// ════════════════════════════════════════════════════════════
// CHAT  →  /v1/chats
// ════════════════════════════════════════════════════════════
export const chatService = {
  getChats:      ()                               => api.get('/chats'),
  getMessages:   (chatId, page)                   => api.get(`/chats/${chatId}/messages?page=${page||1}`),
  sendMessage:   (chatId, content, type, imageUrl, replyToId) =>
                   api.post(`/chats/${chatId}/messages`, { content, messageType: type||'TEXT', imageUrl, replyToMessageId: replyToId }),
  markRead:      (chatId)                         => api.put(`/chats/${chatId}/read`),
  deleteMessage: (chatId, msgId)                  => api.delete(`/chats/${chatId}/messages/${msgId}`),
  getQuota:      (chatId)                         => api.get(`/chats/${chatId}/quota`),
};

// ════════════════════════════════════════════════════════════
// CALLS  →  /v1/calls
// ════════════════════════════════════════════════════════════
export const callService = {
  initiate:     (targetId, callType)  => api.post('/calls/initiate', { targetId, callType }),
  answer:       (callId)              => api.post(`/calls/${callId}/answer`),
  end:          (callId)              => api.post(`/calls/${callId}/end`),
  decline:      (callId)              => api.post(`/calls/${callId}/decline`),
  history:      ()                    => api.get('/calls/history'),
  getAgoraToken:(callId)              => api.get(`/calls/${callId}/agora-token`),
};

// ════════════════════════════════════════════════════════════
// SUBSCRIPTION  →  /v1/subscriptions
// ════════════════════════════════════════════════════════════
export const subscriptionService = {
  getPlans:     ()              => api.get('/subscriptions/plans'),
  subscribe:    (planId, accept)=> api.post('/subscriptions/subscribe', { planId, acceptTerms: accept }),
  getStatus:    ()              => api.get('/subscriptions/status'),
  cancel:       ()              => api.post('/subscriptions/cancel'),
};

// ════════════════════════════════════════════════════════════
// WALLET  →  /v1/wallet/*
// ════════════════════════════════════════════════════════════
export const walletService = {
  getBalance:      ()       => api.get('/wallet/balance'),
  getPackages:     ()       => api.get('/wallet/packages'),
  getTransactions: (type)   => api.get(`/wallet/transactions?type=${type||'all'}`),
  deposit:         (data)   => api.post('/wallet/deposit', data),
  withdraw:        (data)   => api.post('/wallet/withdraw', data),
  razorpayOrder:   (amount) => api.post('/wallet/razorpay/order', { amount }),
  razorpayVerify:  (data)   => api.post('/wallet/razorpay/verify', data),
};

// ════════════════════════════════════════════════════════════
// MISC  →  interests, gifts, verify, privacy
// ════════════════════════════════════════════════════════════
export const miscService = {
  getInterests:   ()              => api.get('/interests'),
  getGifts:       ()              => api.get('/gifts/catalog'),
  sendGift:       (data)          => api.post('/gifts/send', data),
};

// ════════════════════════════════════════════════════════════
// NOTIFICATIONS  →  /v1/notifications
// ════════════════════════════════════════════════════════════
export const notificationService = {
  getAll:      (page)   => api.get(`/notifications?page=${page||1}`),
  markRead:    (id)     => api.put(`/notifications/${id}/read`),
  markAllRead: ()       => api.put('/notifications/read-all'),
  getCount:    ()       => api.get('/notifications/count'),
};

// ════════════════════════════════════════════════════════════
// SUPER CHAT  →  /v1/super-chat
// ════════════════════════════════════════════════════════════
export const superChatService = {
  send:       (targetUserId, message)  => api.post('/super-chat/send', { targetUserId, message }),
  getReceived:()                       => api.get('/super-chat/received'),
  getSent:    ()                       => api.get('/super-chat/sent'),
  respond:    (id)                     => api.post(`/super-chat/${id}/respond`),
};

// ════════════════════════════════════════════════════════════
// ADMIN  →  /v1/admin (admin role required)
// ════════════════════════════════════════════════════════════
export const adminService = {
  dashboard:          ()              => api.get('/admin/dashboard'),
  getUsers:           (p, limit, q)   => api.get(`/admin/users?page=${p||1}&limit=${limit||20}${q?'&search='+q:''}`),
  getUser:            (id)            => api.get(`/admin/users/${id}`),
  toggleStatus:       (id)            => api.put(`/admin/users/${id}/toggle-status`),
  addCoins:           (id, coins, note)=> api.post(`/admin/users/${id}/add-coins`, { coins, note }),
  createUser:         (data)          => api.post('/admin/users/create', data),
  getDeposits:        (status)        => api.get(`/admin/deposits?status=${status||'pending'}`),
  approveDeposit:     (id, note)      => api.post(`/admin/deposits/${id}/approve`, { note: note || '' }),
  rejectDeposit:      (id, note)      => api.post(`/admin/deposits/${id}/reject`, { note: note || '' }),
  getWithdrawals:     (status)        => api.get(`/admin/withdrawals?status=${status||'pending'}`),
  approveWithdrawal:  (id, note)      => api.post(`/admin/withdrawals/${id}/approve`, { note: note || '' }),
  rejectWithdrawal:   (id, note)      => api.post(`/admin/withdrawals/${id}/reject`, { note: note || '' }),
  grantSubscription:  (uid, planId, days) => api.post(`/admin/users/${uid}/grant-subscription`, { planId, days }),
};

export default api;
