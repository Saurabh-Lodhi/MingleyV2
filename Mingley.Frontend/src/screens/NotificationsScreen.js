// NotificationsScreen.js
import React, { useEffect, useState } from 'react';
import {
  View, Text, FlatList, TouchableOpacity,
  StyleSheet, SafeAreaView, ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { notificationService } from '../services/api';

const TYPE_CONFIG = {
  match:     { icon: '🎉', color: '#E94057' },
  like:      { icon: '❤️', color: '#E94057' },
  message:   { icon: '💬', color: '#4A90D9' },
  superchat: { icon: '⭐', color: '#F5A623' },
  superchat_response: { icon: '🎊', color: '#E94057' },
  coins:     { icon: '💰', color: '#F5A623' },
  subscription: { icon: '💎', color: '#8A2387' },
  system:    { icon: '🔔', color: '#888' },
};

export const NotificationsScreen = ({ navigation }) => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => { load(); }, []);

  const load = async () => {
    setLoading(true);
    try {
      const res = await notificationService.getAll();
      setNotifications(res.data.data.notifications || []);
      setUnreadCount(res.data.data.unreadCount || 0);
    } catch (e) {
      console.log(e?.response?.data);
    } finally {
      setLoading(false);
    }
  };

  const markRead = async (id) => {
    await notificationService.markRead(id).catch(() => {});
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    setUnreadCount(prev => Math.max(0, prev - 1));
  };

  const markAllRead = async () => {
    await notificationService.markAllRead().catch(() => {});
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    setUnreadCount(0);
  };

  const cfg = (type) => TYPE_CONFIG[type] || TYPE_CONFIG.system;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="chevron-back" size={24} color="#000" />
        </TouchableOpacity>
        <Text style={styles.title}>Notifications</Text>
        {unreadCount > 0 && (
          <TouchableOpacity onPress={markAllRead} style={styles.markAllBtn}>
            <Text style={styles.markAllText}>Mark all read</Text>
          </TouchableOpacity>
        )}
      </View>

      {loading ? (
        <ActivityIndicator size="large" color="#E94057" style={{ marginTop: 40 }} />
      ) : (
        <FlatList
          data={notifications}
          keyExtractor={i => i.id}
          contentContainerStyle={{ paddingBottom: 20 }}
          renderItem={({ item }) => {
            const c = cfg(item.type);
            return (
              <TouchableOpacity
                style={[styles.item, !item.isRead && styles.unread]}
                onPress={() => markRead(item.id)}
                activeOpacity={0.75}
              >
                <View style={[styles.iconCircle, { backgroundColor: c.color + '20' }]}>
                  <Text style={{ fontSize: 22 }}>{c.icon}</Text>
                </View>
                <View style={styles.info}>
                  <Text style={[styles.notifTitle, !item.isRead && styles.boldTitle]}>{item.title}</Text>
                  <Text style={styles.notifBody} numberOfLines={2}>{item.body}</Text>
                  <Text style={styles.time}>
                    {new Date(item.createdAt).toLocaleString('en-IN', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })}
                  </Text>
                </View>
                {!item.isRead && <View style={[styles.dot, { backgroundColor: c.color }]} />}
              </TouchableOpacity>
            );
          }}
          ListEmptyComponent={
            <View style={styles.empty}>
              <Text style={{ fontSize: 40 }}>🔕</Text>
              <Text style={styles.emptyText}>No notifications yet</Text>
            </View>
          }
        />
      )}
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFF' },
  header: {
    flexDirection: 'row', alignItems: 'center', paddingHorizontal: 20,
    paddingVertical: 14, borderBottomWidth: 1, borderBottomColor: '#F0F0F0', gap: 12,
  },
  title: { flex: 1, fontSize: 20, fontWeight: '700', color: '#000' },
  markAllBtn: { paddingHorizontal: 12, paddingVertical: 6, backgroundColor: '#FFF0F2', borderRadius: 12 },
  markAllText: { color: '#E94057', fontWeight: '600', fontSize: 12 },
  item: { flexDirection: 'row', alignItems: 'flex-start', padding: 16, borderBottomWidth: 1, borderBottomColor: '#F8F8F8' },
  unread: { backgroundColor: '#FFF8F8' },
  iconCircle: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginRight: 14 },
  info: { flex: 1 },
  notifTitle: { fontSize: 14, color: '#000', fontWeight: '500', marginBottom: 3 },
  boldTitle: { fontWeight: '700' },
  notifBody: { fontSize: 13, color: '#666', lineHeight: 18 },
  time: { fontSize: 11, color: '#AAA', marginTop: 5 },
  dot: { width: 8, height: 8, borderRadius: 4, marginTop: 4 },
  empty: { alignItems: 'center', paddingTop: 80, gap: 12 },
  emptyText: { fontSize: 16, color: '#CCC' },
});

