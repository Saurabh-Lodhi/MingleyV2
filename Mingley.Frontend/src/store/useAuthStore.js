import { create } from 'zustand';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authService } from '../services/api';

export const useAuthStore = create((set, get) => ({
  user:            null,
  isAuthenticated: false,
  isLoading:       false,
  error:           null,
  pendingUserId:   null,

  // ── Restore session on app start (called ONCE) ──────────────
  restoreSession: async () => {
    set({ isLoading: true });
    try {
      const token = await AsyncStorage.getItem('accessToken');
      if (!token) return set({ isLoading: false, isAuthenticated: false });
      const res = await authService.getMe();
      set({ user: res.data.data, isAuthenticated: true });
    } catch {
      await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'userId']);
      set({ isAuthenticated: false });
    } finally {
      set({ isLoading: false });
    }
  },

  // ── Register → backend returns { userId, devOtp } ──────────
  register: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const res = await authService.register(data);
      const { userId, devOtp } = res.data.data;
      await AsyncStorage.setItem('pendingUserId', userId);
      set({ pendingUserId: userId, isLoading: false });
      return { success: true, userId, devOtp };
    } catch (err) {
      const msg = err.response?.data?.message || 'Registration failed';
      set({ error: msg, isLoading: false });
      return { success: false, error: msg };
    }
  },

  // ── Verify OTP ──────────────────────────────────────────────
  verifyOtp: async (userId, otp, purpose = 'registration') => {
    set({ isLoading: true, error: null });
    try {
      const res = await authService.verifyOtp(userId, otp, purpose);
      const { accessToken, refreshToken, user } = res.data.data;
      await AsyncStorage.multiSet([
        ['accessToken', accessToken],
        ['refreshToken', refreshToken],
        ['userId', user.id],
      ]);
      set({ user, isAuthenticated: true, isLoading: false, pendingUserId: null });
      return { success: true };
    } catch (err) {
      const msg = err.response?.data?.message || 'Invalid OTP';
      set({ error: msg, isLoading: false });
      return { success: false, error: msg };
    }
  },

  // ── Resend OTP ──────────────────────────────────────────────
  resendOtp: async (userId, purpose = 'registration') => {
    try {
      await authService.resendOtp(userId, purpose);
      return { success: true };
    } catch (err) {
      return { success: false, error: err.response?.data?.message || 'Failed to resend' };
    }
  },

  // ── Login ────────────────────────────────────────────────────
  // Backend now returns 200 with { requiresVerification: true, userId, devOtp }
  // for unverified accounts instead of throwing error.
  login: async (identifier, password) => {
    set({ isLoading: true, error: null });
    try {
      const res = await authService.login(identifier, password);
      const data = res.data.data;

      // Check if account needs OTP verification
      if (data?.requiresVerification) {
        const { userId, devOtp } = data;
        await AsyncStorage.setItem('pendingUserId', userId);
        set({ pendingUserId: userId, isLoading: false });
        return { success: false, requiresVerification: true, userId, devOtp };
      }

      // Normal login success
      const { accessToken, refreshToken, user } = data;
      await AsyncStorage.multiSet([
        ['accessToken', accessToken],
        ['refreshToken', refreshToken],
        ['userId', user.id],
      ]);
      set({ user, isAuthenticated: true, isLoading: false });
      return { success: true };
    } catch (err) {
      const msg = err.response?.data?.message || 'Login failed';

      // Fallback: still handle old UNVERIFIED format just in case
      if (msg.startsWith('UNVERIFIED:')) {
        const parts  = msg.split(':');
        const userId = parts[1];
        const devOtp = parts[2] || null;
        await AsyncStorage.setItem('pendingUserId', userId);
        set({ pendingUserId: userId, isLoading: false });
        return { success: false, requiresVerification: true, userId, devOtp };
      }

      set({ error: msg, isLoading: false });
      return { success: false, error: msg };
    }
  },

  logout: async () => {
    try { await authService.logout(); } catch {}
    await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'userId', 'pendingUserId']);
    set({ user: null, isAuthenticated: false, error: null, pendingUserId: null });
  },

  updateUser: (updates) => set(state => ({ user: { ...state.user, ...updates } })),

  forgotPassword: async (identifier) => {
    try {
      await authService.forgotPassword(identifier);
      return { success: true };
    } catch (err) {
      return { success: false, error: err.response?.data?.message || 'Failed' };
    }
  },

  resetPassword: async (userId, otp, newPassword) => {
    try {
      await authService.resetPassword({ userId, otp, newPassword });
      return { success: true };
    } catch (err) {
      return { success: false, error: err.response?.data?.message || 'Failed' };
    }
  },

  changePassword: async (currentPassword, newPassword) => {
    try {
      await authService.changePassword({ currentPassword, newPassword });
      return { success: true };
    } catch (err) {
      return { success: false, error: err.response?.data?.message || 'Failed' };
    }
  },
}));
