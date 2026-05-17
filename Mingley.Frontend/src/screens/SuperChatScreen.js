// ─── SuperChatScreen.js ───────────────────────────────────────────────────────
import React, { useEffect, useState } from 'react';
import {
  View, Text, FlatList, TouchableOpacity, TextInput,
  StyleSheet, SafeAreaView, Alert, ActivityIndicator, Image,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from '../components/compat/LinearGradient';
import { superChatService } from '../services/api';
import { useChatStore } from '../store/useChatStore';

export const SuperChatScreen = ({ route, navigation }) => {
  const { targetUser } = route.params || {};
  const { coinBalance, updateCoinBalance } = useChatStore();
  const [received, setReceived] = useState([]);
  const [sent, setSent] = useState([]);
  const [message, setMessage] = useState('');
  const [sending, setSending] = useState(false);
  const [tab, setTab] = useState(targetUser ? 'compose' : 'received');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!targetUser) {
      Promise.all([
        superChatService.getReceived().then(r => setReceived(r.data.data.superChats || [])),
        superChatService.getSent().then(r => setSent(r.data.data.superChats || [])),
      ]).finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, []);

  const handleSend = async () => {
    if (!message.trim()) return Alert.alert('Error', 'Enter a message');
    if (coinBalance < 500) return Alert.alert('Insufficient Coins', 'You need 500 coins to send a SuperChat.');

    setSending(true);
    try {
      await superChatService.send(targetUser.id, message.trim());
      updateCoinBalance(-500);
      Alert.alert('⭐ SuperChat Sent!', `Your message has been highlighted and sent to ${targetUser.fullName}!\n\nIf they respond, you'll be matched automatically.`, [
        { text: 'OK', onPress: () => navigation.goBack() },
      ]);
    } catch (err) {
      Alert.alert('Error', err?.response?.data?.message || 'Failed to send');
    } finally {
      setSending(false);
    }
  };

  const handleRespond = async (id, fromUser) => {
    Alert.alert('Respond to SuperChat', `Responding will match you with ${fromUser}. Continue?`, [
      {
        text: 'Respond & Match!', onPress: async () => {
          const res = await superChatService.respond(id).catch(err => {
            Alert.alert('Error', err?.response?.data?.message); return null;
          });
          if (res) {
            Alert.alert('🎉 Matched!', 'You are now matched! Start chatting.');
            const r = await superChatService.getReceived();
            setReceived(r.data.data.superChats || []);
          }
        },
      },
      { text: 'Later', style: 'cancel' },
    ]);
  };

  return (
    <SafeAreaView style={sc.container}>
      <View style={sc.header}>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="chevron-back" size={24} color="#000" />
        </TouchableOpacity>
        <Text style={sc.title}>⭐ SuperChat</Text>
        <View style={sc.coinBadge}>
          <Text style={sc.coinText}>{coinBalance} 🪙</Text>
        </View>
      </View>

      {/* Compose mode */}
      {targetUser ? (
        <View style={sc.compose}>
          <Image source={{ uri: targetUser.avatar }} style={sc.avatar} />
          <Text style={sc.composeName}>{targetUser.fullName}</Text>
          <View style={sc.costInfo}>
            <Ionicons name="star" size={18} color="#F5A623" />
            <Text style={sc.costText}>500 coins · Message gets highlighted</Text>
          </View>
          <TextInput
            style={sc.input}
            value={message}
            onChangeText={setMessage}
            placeholder="Write your special message..."
            multiline
            maxLength={200}
            placeholderTextColor="#999"
          />
          <Text style={sc.charCount}>{message.length}/200</Text>
          <TouchableOpacity onPress={handleSend} disabled={sending}>
            <LinearGradient colors={['#F5A623', '#E6940F']} style={sc.sendBtn}>
              {sending ? <ActivityIndicator color="#FFF" /> : (
                <>
                  <Ionicons name="star" size={18} color="#FFF" />
                  <Text style={sc.sendText}>Send SuperChat (500 🪙)</Text>
                </>
              )}
            </LinearGradient>
          </TouchableOpacity>
          <Text style={sc.hint}>If they respond → you both get matched instantly!</Text>
        </View>
      ) : (
        <>
          {/* Tabs */}
          <View style={sc.tabs}>
            {['received', 'sent'].map(t => (
              <TouchableOpacity key={t} style={[sc.tab, tab === t && sc.activeTab]} onPress={() => setTab(t)}>
                <Text style={[sc.tabText, tab === t && sc.activeTabText]}>{t === 'received' ? 'Received' : 'Sent'}</Text>
              </TouchableOpacity>
            ))}
          </View>

          {loading ? <ActivityIndicator color="#F5A623" style={{ marginTop: 40 }} /> : (
            <FlatList
              data={tab === 'received' ? received : sent}
              keyExtractor={i => i.id}
              contentContainerStyle={{ padding: 16, gap: 12 }}
              renderItem={({ item }) => (
                <View style={sc.scCard}>
                  <View style={sc.scHeader}>
                    <Image
                      source={{ uri: (tab === 'received' ? item.fromUser : item.toUser)?.avatar }}
                      style={sc.scAvatar}
                    />
                    <View style={{ flex: 1 }}>
                      <Text style={sc.scName}>{(tab === 'received' ? item.fromUser : item.toUser)?.fullName}</Text>
                      <Text style={sc.scTime}>{new Date(item.createdAt).toLocaleDateString()}</Text>
                    </View>
                    <View style={[sc.scBadge, item.isResponded && sc.scBadgeGreen]}>
                      <Text style={sc.scBadgeText}>{item.isResponded ? '✅ Matched' : '⏳ Pending'}</Text>
                    </View>
                  </View>
                  <Text style={sc.scMsg}>"{item.message}"</Text>
                  {tab === 'received' && !item.isResponded && (
                    <TouchableOpacity
                      onPress={() => handleRespond(item.id, item.fromUser?.fullName)}
                      style={sc.respondBtn}
                    >
                      <LinearGradient colors={['#E94057', '#8A2387']} style={sc.respondGrad}>
                        <Text style={{ color: '#FFF', fontWeight: '700' }}>Respond & Match 🎉</Text>
                      </LinearGradient>
                    </TouchableOpacity>
                  )}
                </View>
              )}
              ListEmptyComponent={
                <View style={sc.empty}>
                  <Text style={{ fontSize: 36 }}>⭐</Text>
                  <Text style={sc.emptyText}>No SuperChats {tab} yet</Text>
                </View>
              }
            />
          )}
        </>
      )}
    </SafeAreaView>
  );
};

const sc = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFF' },
  header: {
    flexDirection: 'row', alignItems: 'center', paddingHorizontal: 20,
    paddingVertical: 14, borderBottomWidth: 1, borderBottomColor: '#F0F0F0', gap: 12,
  },
  title: { flex: 1, fontSize: 20, fontWeight: '700' },
  coinBadge: { backgroundColor: '#FFF8E1', paddingHorizontal: 12, paddingVertical: 5, borderRadius: 14 },
  coinText: { fontWeight: '700', color: '#F5A623' },
  compose: { flex: 1, padding: 24, alignItems: 'center', gap: 14 },
  avatar: { width: 80, height: 80, borderRadius: 40 },
  composeName: { fontSize: 20, fontWeight: '700', color: '#000' },
  costInfo: { flexDirection: 'row', alignItems: 'center', gap: 6, backgroundColor: '#FFF8E1', paddingHorizontal: 14, paddingVertical: 8, borderRadius: 16 },
  costText: { color: '#F5A623', fontWeight: '600', fontSize: 13 },
  input: {
    width: '100%', borderWidth: 1.5, borderColor: '#F0F0F0', borderRadius: 16,
    padding: 16, fontSize: 15, minHeight: 120, textAlignVertical: 'top',
    backgroundColor: '#FAFAFA',
  },
  charCount: { alignSelf: 'flex-end', color: '#AAA', fontSize: 12 },
  sendBtn: { width: '100%', flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8, padding: 16, borderRadius: 16 },
  sendText: { color: '#FFF', fontSize: 16, fontWeight: '700' },
  hint: { color: '#888', fontSize: 12, textAlign: 'center', fontStyle: 'italic' },
  tabs: { flexDirection: 'row', borderBottomWidth: 1, borderBottomColor: '#F0F0F0' },
  tab: { flex: 1, paddingVertical: 14, alignItems: 'center' },
  activeTab: { borderBottomWidth: 2, borderBottomColor: '#F5A623' },
  tabText: { color: '#888', fontWeight: '600' },
  activeTabText: { color: '#F5A623' },
  scCard: { backgroundColor: '#FAFAFA', borderRadius: 16, padding: 16, borderWidth: 1, borderColor: '#F0F0F0', borderLeftWidth: 4, borderLeftColor: '#F5A623' },
  scHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: 10, gap: 12 },
  scAvatar: { width: 40, height: 40, borderRadius: 20 },
  scName: { fontSize: 15, fontWeight: '600', color: '#000' },
  scTime: { fontSize: 11, color: '#AAA', marginTop: 2 },
  scBadge: { backgroundColor: '#FFF3E0', paddingHorizontal: 10, paddingVertical: 4, borderRadius: 12 },
  scBadgeGreen: { backgroundColor: '#E8F5E9' },
  scBadgeText: { fontSize: 11, fontWeight: '600', color: '#F5A623' },
  scMsg: { color: '#555', fontSize: 14, lineHeight: 20, fontStyle: 'italic', marginBottom: 10 },
  respondBtn: { marginTop: 4 },
  respondGrad: { padding: 12, borderRadius: 12, alignItems: 'center' },
  empty: { alignItems: 'center', paddingTop: 60, gap: 10 },
  emptyText: { color: '#CCC', fontSize: 16 },
});
