import { create } from 'zustand';
import { chatService, walletService, subscriptionService } from '../services/api';

export const useChatStore = create((set, get) => ({
  conversations: [],  // alias for chats
  chats:         [],
  messages:      {},  // { [chatId]: [msg] }
  isLoading:     false,
  coinBalance:   0,
  isPremium:     false,
  subscription:  null,

  // ── Conversations (for ConversationsScreen) ────────────────
  loadConversations: async () => {
    set({ isLoading: true });
    try {
      const res = await chatService.getChats();
      const chats = res.data.data.chats || [];
      set({ chats, conversations: chats });
    } catch (err) {
      console.log('Chats error:', err?.response?.data);
    } finally {
      set({ isLoading: false });
    }
  },

  loadChats: async () => {
    return get().loadConversations();
  },

  // ── Messages ──────────────────────────────────────────────
  loadMessages: async (chatId, page = 1) => {
    try {
      const res = await chatService.getMessages(chatId, page);
      const msgs = res.data.data.messages || [];
      const ordered = [...msgs].reverse();
      set(state => ({
        messages: {
          ...state.messages,
          [chatId]: page === 1 ? ordered : [...ordered, ...(state.messages[chatId] || [])],
        },
        chats: state.chats.map(c => c.matchId === chatId ? { ...c, unreadCount: 0 } : c),
        conversations: state.conversations.map(c => c.matchId === chatId ? { ...c, unreadCount: 0 } : c),
      }));
      return ordered;
    } catch (err) {
      console.log('Messages error:', err?.response?.data);
      return [];
    }
  },

  sendMessage: async (chatId, text, type = 'TEXT', imageUrl = null, replyToMessageId = null) => {
    try {
      const res = await chatService.sendMessage(chatId, text, type, imageUrl, replyToMessageId);
      const result = res.data.data;
      if (result.newBalance !== undefined)
        set({ coinBalance: result.newBalance });
      // Add message to local state immediately
      const msg = result.message || {
        id: result.id || Date.now().toString(),
        content: text,
        messageType: type,
        senderId: null,
        createdAt: new Date().toISOString(),
        isRead: false,
      };
      set(state => ({
        messages: {
          ...state.messages,
          [chatId]: [...(state.messages[chatId] || []), msg],
        },
      }));
      return { success: true, ...result };
    } catch (err) {
      return { success: false, error: err?.response?.data?.message || 'Failed to send' };
    }
  },

  markRead: async (chatId) => {
    try {
      await chatService.markRead(chatId);
      set(state => ({
        chats: state.chats.map(c => c.matchId === chatId ? { ...c, unreadCount: 0 } : c),
        conversations: state.conversations.map(c => c.matchId === chatId ? { ...c, unreadCount: 0 } : c),
      }));
    } catch {}
  },

  deleteMessage: async (chatId, messageId) => {
    try {
      await chatService.deleteMessage(chatId, messageId);
      set(state => ({
        messages: {
          ...state.messages,
          [chatId]: (state.messages[chatId] || []).map(m =>
            m.id === messageId ? { ...m, isDeleted: true, content: null, type: 'deleted' } : m
          ),
        },
      }));
      return { success: true };
    } catch (err) {
      return { success: false };
    }
  },

  getQuota: async (chatId) => {
    try {
      const res = await chatService.getQuota(chatId);
      return res.data.data;
    } catch { return null; }
  },

  // ── Socket-driven handlers ────────────────────────────────
  receiveMessage: (chatId, message) => {
    set(state => ({
      messages: {
        ...state.messages,
        [chatId]: [...(state.messages[chatId] || []), message],
      },
      chats: state.chats.map(c =>
        c.matchId === chatId
          ? { ...c, lastMessage: message, unreadCount: (c.unreadCount || 0) + 1 }
          : c
      ),
      conversations: state.conversations.map(c =>
        c.matchId === chatId
          ? { ...c, lastMessage: message, unreadCount: (c.unreadCount || 0) + 1 }
          : c
      ),
    }));
  },

  onMessagesRead: (chatId) => {
    set(state => ({
      messages: {
        ...state.messages,
        [chatId]: (state.messages[chatId] || []).map(m => ({ ...m, isRead: true, readAt: new Date().toISOString() })),
      },
    }));
  },

  onMessageDeleted: (chatId, messageId) => {
    set(state => ({
      messages: {
        ...state.messages,
        [chatId]: (state.messages[chatId] || []).map(m =>
          m.id === messageId ? { ...m, isDeleted: true, content: null, type: 'deleted' } : m
        ),
      },
    }));
  },

  // ── Wallet ────────────────────────────────────────────────
  loadWallet: async () => {
    try {
      const [walletRes, subRes] = await Promise.allSettled([
        walletService.getBalance(),
        subscriptionService.getStatus(),
      ]);
      if (walletRes.status === 'fulfilled')
        set({ coinBalance: walletRes.value.data.data.coinBalance || 0 });
      if (subRes.status === 'fulfilled') {
        const sub = subRes.value.data.data;
        set({ isPremium: !!sub?.isActive, subscription: sub });
      }
    } catch (err) {
      console.log('Wallet error:', err?.response?.data);
    }
  },

  updateCoinBalance: (amount) => {
    // Can be absolute value or relative change
    if (typeof amount === 'number' && Math.abs(amount) < 10000) {
      // Relative delta
      set(state => ({ coinBalance: Math.max(0, state.coinBalance + amount) }));
    } else {
      set({ coinBalance: amount });
    }
  },

  upgradeToPremium: () => set({ isPremium: true }),

  getTotalUnread: () =>
    (get().conversations || get().chats || []).reduce((sum, c) => sum + (c.unreadCount || 0), 0),
}));
