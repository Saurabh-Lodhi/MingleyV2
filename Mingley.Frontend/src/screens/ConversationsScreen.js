import React, { useEffect, useCallback, useRef } from 'react';
import {
  View, Text, FlatList, TouchableOpacity, StyleSheet,
  Image, RefreshControl, SafeAreaView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from '../components/compat/LinearGradient';
import { useChatStore } from '../store/useChatStore';
import { useAuthStore } from '../store/useAuthStore';
import { onNewMessage } from '../services/socket';

export const ConversationsScreen = ({ navigation }) => {
  const { conversations, loadConversations, receiveMessage, isLoading, getTotalUnread } = useChatStore();
  const { user } = useAuthStore();
  const mountedRef = useRef(true);

  useEffect(() => {
    mountedRef.current = true;
    loadConversations();

    // FIX: Only update conversations on new message — do NOT reload entire list
    // This was causing the auto-refresh flicker
    const off = onNewMessage(({ matchId, message }) => {
      if (mountedRef.current) receiveMessage(matchId, message);
    });

    return () => {
      mountedRef.current = false;
      off();
    };
  }, []);

  const onRefresh = useCallback(async () => {
    await loadConversations();
  }, []);

  const renderConversation = ({ item }) => {
    const isFromMe = item.lastMessage?.senderId === user?.id;
    const lastMsgText = item.lastMessage?.content
      ? (isFromMe ? `You: ${item.lastMessage.content}` : item.lastMessage.content)
      : 'Tap to start chatting!';

    const displayUser = item.user || item.participant || {};
    const chatId = item.matchId || item.chatId;

    return (
      <TouchableOpacity
        style={styles.item}
        onPress={() =>
          navigation.navigate('Chat', {
            matchId: chatId,
            userId: displayUser.id,
            userName: displayUser.fullName,
            userAvatar: displayUser.avatar,
            isOnline: displayUser.isOnline,
          })
        }
        activeOpacity={0.75}
      >
        <View style={styles.avatarWrap}>
          <Image
            source={{ uri: displayUser.avatar || 'https://i.pravatar.cc/100?u=' + displayUser.id }}
            style={styles.avatar}
          />
          {displayUser.isOnline && <View style={styles.onlineDot} />}
        </View>
        <View style={styles.info}>
          <Text style={styles.name}>{displayUser.fullName || 'User'}</Text>
          <Text
            style={[styles.lastMsg, item.unreadCount > 0 && styles.unreadMsg]}
            numberOfLines={1}
          >
            {item.lastMessage?.messageType === 'IMAGE' ? '📷 Photo' :
             item.lastMessage?.messageType === 'GIFT' ? '🎁 Gift' :
             lastMsgText}
          </Text>
        </View>
        <View style={styles.right}>
          <Text style={styles.time}>
            {item.lastMessage?.createdAt
              ? new Date(item.lastMessage.createdAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })
              : ''}
          </Text>
          {item.unreadCount > 0 && (
            <View style={styles.badge}>
              <Text style={styles.badgeText}>{item.unreadCount > 99 ? '99+' : item.unreadCount}</Text>
            </View>
          )}
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Messages</Text>
        <View style={styles.headerBadge}>
          {getTotalUnread() > 0 && (
            <View style={styles.totalBadge}>
              <Text style={styles.totalBadgeText}>{getTotalUnread()}</Text>
            </View>
          )}
        </View>
      </View>

      <FlatList
        data={conversations}
        keyExtractor={item => item.matchId || item.chatId || String(Math.random())}
        renderItem={renderConversation}
        refreshControl={<RefreshControl refreshing={isLoading} onRefresh={onRefresh} colors={['#E94057']} />}
        contentContainerStyle={conversations.length === 0 && styles.emptyContainer}
        ListEmptyComponent={
          <View style={styles.empty}>
            <Ionicons name="chatbubbles-outline" size={64} color="#E0E0E0" />
            <Text style={styles.emptyTitle}>No conversations yet</Text>
            <Text style={styles.emptySub}>Match with someone and start chatting!</Text>
          </View>
        }
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFF' },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: 20, paddingVertical: 16,
    borderBottomWidth: 1, borderBottomColor: '#F0F0F0',
  },
  title: { fontSize: 24, fontWeight: '800', color: '#000' },
  headerBadge: { flexDirection: 'row', alignItems: 'center' },
  totalBadge: {
    backgroundColor: '#E94057', paddingHorizontal: 8, paddingVertical: 3, borderRadius: 12,
  },
  totalBadgeText: { color: '#FFF', fontSize: 12, fontWeight: '700' },
  item: {
    flexDirection: 'row', alignItems: 'center',
    paddingHorizontal: 20, paddingVertical: 14,
    borderBottomWidth: 1, borderBottomColor: '#F8F8F8',
  },
  avatarWrap: { position: 'relative', marginRight: 14 },
  avatar: { width: 56, height: 56, borderRadius: 28 },
  onlineDot: {
    position: 'absolute', bottom: 2, right: 2,
    width: 14, height: 14, borderRadius: 7,
    backgroundColor: '#4CAF50', borderWidth: 2, borderColor: '#FFF',
  },
  info: { flex: 1 },
  name: { fontSize: 16, fontWeight: '700', color: '#000', marginBottom: 4 },
  lastMsg: { fontSize: 13, color: '#888' },
  unreadMsg: { color: '#000', fontWeight: '600' },
  right: { alignItems: 'flex-end', gap: 6 },
  time: { fontSize: 11, color: '#BBB' },
  badge: {
    backgroundColor: '#E94057', minWidth: 20, height: 20,
    borderRadius: 10, justifyContent: 'center', alignItems: 'center', paddingHorizontal: 4,
  },
  badgeText: { color: '#FFF', fontSize: 11, fontWeight: '700' },
  emptyContainer: { flex: 1 },
  empty: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: 12, paddingTop: 80 },
  emptyTitle: { fontSize: 20, fontWeight: '700', color: '#000' },
  emptySub: { fontSize: 14, color: '#888', textAlign: 'center', paddingHorizontal: 40 },
});
