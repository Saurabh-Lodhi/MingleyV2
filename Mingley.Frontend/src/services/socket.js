import { HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { BASE_URL } from './api';

let chatHub  = null;
let notifHub = null;

// ── Helpers ───────────────────────────────────────────────────────────────────
const buildHub = (url) =>
  new HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: async () => (await AsyncStorage.getItem('accessToken')) || '' })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(LogLevel.Warning)
    .build();

// ── Chat Hub ──────────────────────────────────────────────────────────────────
export const connectChatHub = async () => {
  if (chatHub?.state === HubConnectionState.Connected) return chatHub;
  chatHub = buildHub(`${BASE_URL}/hubs/chat`);
  try {
    await chatHub.start();
    console.log('✅ ChatHub connected');
  } catch (e) {
    console.error('❌ ChatHub connect error:', e);
  }
  return chatHub;
};

// ── Notification Hub ──────────────────────────────────────────────────────────
export const connectNotifHub = async () => {
  if (notifHub?.state === HubConnectionState.Connected) return notifHub;
  notifHub = buildHub(`${BASE_URL}/hubs/notifications`);
  try {
    await notifHub.start();
    console.log('✅ NotifHub connected');
  } catch (e) {
    console.error('❌ NotifHub connect error:', e);
  }
  return notifHub;
};

export const disconnectAll = async () => {
  if (chatHub)  { await chatHub.stop();  chatHub  = null; }
  if (notifHub) { await notifHub.stop(); notifHub = null; }
  console.log('🔌 Hubs disconnected');
};

export const getChatHub  = () => chatHub;
export const getNotifHub = () => notifHub;

// ── Chat room management ──────────────────────────────────────────────────────
export const joinMatchRoom  = (matchId) => chatHub?.invoke('JoinChat', matchId);
export const leaveMatchRoom = (matchId) => chatHub?.invoke('LeaveChat', matchId);
export const joinChat  = (chatId) => chatHub?.invoke('JoinChat', chatId);
export const leaveChat = (chatId) => chatHub?.invoke('LeaveChat', chatId);

// ── Typing ────────────────────────────────────────────────────────────────────
export const emitTyping  = (chatId, isTyping) => chatHub?.invoke('Typing', chatId, isTyping);
export const sendTyping  = (chatId, isTyping) => chatHub?.invoke('Typing', chatId, isTyping);
export const emitMarkRead = (chatId) => chatHub?.invoke('MarkRead', chatId);

// ── Call signalling ───────────────────────────────────────────────────────────
export const sendCallSignal = (targetUserId, signalType, signalData) =>
  chatHub?.invoke('CallSignal', targetUserId, signalType, signalData);

// ══════════════════════════════════════════════════════════
// SERVER → CLIENT event subscriptions
// ══════════════════════════════════════════════════════════
const on = (hub, event, cb) => { hub?.on(event, cb); return () => hub?.off(event, cb); };

// Matches
export const onNewMatch         = (cb) => on(chatHub, 'NewMatch',          cb);
export const onUnmatched        = (cb) => on(chatHub, 'Unmatched',         cb);

// Online status — FIX: properly subscribed
export const onUserOnlineStatus = (cb) => on(chatHub, 'UserOnlineStatus',  cb);

// Messages — note: backend sends matchId (not chatId) in payload
export const onNewMessage       = (cb) => on(chatHub, 'NewMessage',        cb);
export const onMessagesRead     = (cb) => on(chatHub, 'MessagesRead',      cb);
export const onMessageDeleted   = (cb) => on(chatHub, 'MessageDeleted',    cb);

// Typing
export const onTyping           = (cb) => on(chatHub, 'Typing',            cb);

// Calls
export const onIncomingCall     = (cb) => on(chatHub, 'IncomingCall',      cb);
export const onCallAnswered     = (cb) => on(chatHub, 'CallAnswered',      cb);
export const onCallDeclined     = (cb) => on(chatHub, 'CallDeclined',      cb);
export const onCallEnded        = (cb) => on(chatHub, 'CallEnded',         cb);
export const onCallSignal       = (cb) => on(chatHub, 'CallSignal',        cb);

// SuperChat
export const onNewSuperChat         = (cb) => on(chatHub, 'NewSuperChat',         cb);
export const onSuperChatResponded   = (cb) => on(chatHub, 'SuperChatResponded',   cb);

// Notifications (via notifHub)
export const onNewNotification  = (cb) => on(notifHub, 'NewNotification',   cb);
