import { create } from 'zustand';
import { discoverService, userService, walletService, subscriptionService } from '../services/api';

export const useUserStore = create((set, get) => ({
  feed:           [],
  feedPage:       1,
  feedHasNext:    true,
  feedLoading:    false,
  matches:        [],
  currentProfile: null,
  onlineStatuses: {},  // { [userId]: boolean }

  // ── Discover feed ─────────────────────────────────────────
  loadFeed: async (reset = false, filters = {}) => {
    const page = reset ? 1 : get().feedPage;
    if (!reset && !get().feedHasNext) return;
    set({ feedLoading: true });
    try {
      const res = await discoverService.getFeed(page, 20, filters);
      const { users = [], pagination = {} } = res.data.data;
      set(state => ({
        feed:        reset ? users : [...state.feed, ...users],
        feedPage:    (pagination.page || page) + 1,
        feedHasNext: pagination.hasNext !== false,
      }));
    } catch (err) {
      console.log('Feed error:', err?.response?.data);
    } finally {
      set({ feedLoading: false });
    }
  },

  swipe: async (targetId, action) => {
    try {
      const res = await discoverService.swipe(targetId, action);
      // Don't remove from feed — let DiscoverScreen handle looping
      return res.data.data;
    } catch (err) {
      throw err?.response?.data || { message: 'Swipe failed' };
    }
  },

  // ── Online status (real-time from SignalR) ────────────────
  // FIX: update online status received from socket
  updateOnlineStatus: (userId, isOnline, lastSeen) => {
    set(state => ({
      onlineStatuses: { ...state.onlineStatuses, [userId]: isOnline },
      matches: state.matches.map(m =>
        (m.user?.id === userId || m.matchedUser?.id === userId)
          ? { ...m, user: m.user?.id === userId ? { ...m.user, isOnline } : m.user }
          : m
      ),
    }));
  },

  getOnlineStatus: (userId) => {
    const statuses = get().onlineStatuses;
    return statuses.hasOwnProperty(userId) ? statuses[userId] : null;
  },

  // ── Matches ───────────────────────────────────────────────
  loadMatches: async () => {
    try {
      const res = await discoverService.getMatches();
      set({ matches: res.data.data.matches || [] });
    } catch (err) {
      console.log('Matches error:', err?.response?.data);
    }
  },

  addMatch: (matchData) => {
    set(state => ({ matches: [matchData, ...state.matches] }));
  },

  removeMatch: (matchId) => {
    set(state => ({ matches: state.matches.filter(m => m.matchId !== matchId) }));
  },

  unmatch: async (matchId) => {
    try {
      await discoverService.unmatch(matchId);
      set(state => ({ matches: state.matches.filter(m => m.matchId !== matchId) }));
    } catch (err) {
      console.log('Unmatch error:', err?.response?.data);
    }
  },

  getLikes: async () => {
    try {
      const res = await discoverService.getLikes();
      return res.data.data.users || [];
    } catch {
      return [];
    }
  },

  getProfile: async (userId) => {
    try {
      const res = await userService.getUser(userId);
      set({ currentProfile: res.data.data });
      return res.data.data;
    } catch { return null; }
  },

  updateProfile: async (data) => {
    try {
      const res = await userService.updateProfile(data);
      return { success: true, data: res.data.data };
    } catch (err) {
      return { success: false, error: err?.response?.data?.message || 'Failed to update' };
    }
  },

  updatePreferences: async (data) => {
    try {
      await userService.updatePreferences(data);
      return { success: true };
    } catch (err) {
      return { success: false, error: err?.response?.data?.message || 'Failed' };
    }
  },

  updateLocation: async (data) => {
    try {
      await userService.updateLocation(data);
      return { success: true };
    } catch { return { success: false }; }
  },

  addImage: async (url, isPrimary) => {
    try {
      await userService.addImage(url, isPrimary);
      return { success: true };
    } catch { return { success: false }; }
  },

  block: async (id) => {
    try {
      await userService.block(id);
      return { success: true };
    } catch { return { success: false }; }
  },

  report: async (id, reason, desc) => {
    try {
      await userService.report(id, reason, desc);
      return { success: true };
    } catch { return { success: false }; }
  },
}));
