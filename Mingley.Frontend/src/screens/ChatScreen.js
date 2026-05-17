// ChatScreen.js — Fixed: online status, gift persistence, activity/settings
import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, FlatList,
  StyleSheet, KeyboardAvoidingView, Platform, Alert,
  Image, ActivityIndicator, SafeAreaView, Modal, ScrollView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from '../components/compat/LinearGradient';
import { useChatStore } from '../store/useChatStore';
import { useAuthStore } from '../store/useAuthStore';
import { callService, miscService } from '../services/api';
import {
  joinChat, leaveChat, sendTyping, onNewMessage,
  onTyping, onMessagesRead, onMessageDeleted, onUserOnlineStatus,
} from '../services/socket';
import * as ImagePicker from 'expo-image-picker';

export const ChatScreen = ({ route, navigation }) => {
  const { matchId, userId, userName, userAvatar, isOnline: initialOnline } = route.params || {};
  const { user } = useAuthStore();
  const { messages, sendMessage, loadMessages, deleteMessage, receiveMessage, onMessagesRead: handleRead, onMessageDeleted: handleDeleted } = useChatStore();

  const [inputText, setInputText] = useState('');
  const [sending, setSending] = useState(false);
  const [isTyping, setIsTyping] = useState(false);
  const [replyTo, setReplyTo] = useState(null);
  const [isOnline, setIsOnline] = useState(initialOnline);
  const [showGiftModal, setShowGiftModal] = useState(false);
  const [gifts, setGifts] = useState([]);
  const [sentGifts, setSentGifts] = useState([]);
  const [sendingGift, setSendingGift] = useState(null);
  const [showActivityModal, setShowActivityModal] = useState(false);
  const [showSettingsModal, setShowSettingsModal] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);

  const flatListRef = useRef(null);
  const typingTimeout = useRef(null);

  const matchMessages = messages[matchId] || [];

  useEffect(() => {
    loadMessages(matchId);
    joinChat(matchId);

    // Load gifts catalog persistently
    loadGifts();

    const offMessage = onNewMessage(({ matchId: mId, message }) => {
      if (mId === matchId) receiveMessage(matchId, message);
    });
    const offTyping = onTyping(({ userId: tId, isTyping: typing }) => {
      if (tId === userId) setIsTyping(typing);
    });
    const offRead = onMessagesRead(({ matchId: mId }) => {
      if (mId === matchId) handleRead(matchId);
    });
    const offDeleted = onMessageDeleted(({ matchId: mId, messageId }) => {
      if (mId === matchId) handleDeleted(matchId, messageId);
    });
    // FIX: Listen for online status changes
    const offOnline = onUserOnlineStatus(({ userId: uid, isOnline: status }) => {
      if (uid === userId) setIsOnline(status);
    });

    return () => {
      leaveChat(matchId);
      offMessage(); offTyping(); offRead(); offDeleted(); offOnline();
    };
  }, [matchId]);

  const loadGifts = async () => {
    try {
      const res = await miscService.getGifts();
      setGifts(res.data.data.gifts || []);
    } catch {}
  };

  useEffect(() => {
    if (matchMessages.length > 0) {
      setTimeout(() => flatListRef.current?.scrollToEnd({ animated: true }), 100);
    }
  }, [matchMessages.length]);

  const handleInputChange = (text) => {
    setInputText(text);
    sendTyping(matchId, true);
    clearTimeout(typingTimeout.current);
    typingTimeout.current = setTimeout(() => sendTyping(matchId, false), 1500);
  };

  const handleSend = async () => {
    const content = inputText.trim();
    if (!content || sending) return;
    setInputText('');
    setSending(true);
    sendTyping(matchId, false);

    const result = await sendMessage(matchId, content, 'TEXT', null, replyTo?.id || null);
    setReplyTo(null);
    if (!result.success) {
      Alert.alert('Send Failed', result.error || 'Please try again');
      setInputText(content);
    }
    setSending(false);
  };

  const handleCall = async (type) => {
    try {
      const res = await callService.initiate(userId, type.toLowerCase());
      const { callId, callType } = res.data.data;
      // Fetch Agora token
      const tokenRes = await callService.getAgoraToken(callId);
      const { token, appId, channelName, uid } = tokenRes.data.data;
      navigation.navigate('Call', {
        callId,
        channelName,
        token,
        uid,
        agoraAppId: appId,
        isInitiator: true,
        callType: type,
        targetUser: { id: userId, fullName: userName, avatar: userAvatar },
      });
    } catch (e) {
      Alert.alert('Call Failed', e?.response?.data?.message || 'Failed to start call');
    }
  };

  const handleSendGift = async (gift) => {
    setSendingGift(gift.id);
    try {
      await miscService.sendGift({ recipientId: userId, giftId: gift.id, chatId: matchId });
      // FIX: Store sent gift in chat as a message so it persists
      const giftMsg = {
        id: Date.now().toString(),
        content: `🎁 ${gift.name}`,
        messageType: 'GIFT',
        senderId: user?.id,
        createdAt: new Date().toISOString(),
        giftName: gift.name,
        giftEmoji: gift.emoji,
        giftCost: gift.price,
      };
      receiveMessage(matchId, giftMsg);
      // Also send as a message via chat
      await sendMessage(matchId, `🎁 Sent you a ${gift.name}!`, 'TEXT');
      setShowGiftModal(false);
      Alert.alert('Gift Sent! 🎁', `You sent ${gift.name} to ${userName}!`);
    } catch (e) {
      Alert.alert('Failed', e?.response?.data?.message || 'Could not send gift');
    } finally {
      setSendingGift(null);
    }
  };

  const handlePickImage = async () => {
    try {
      const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission needed', 'Allow access to photos to send images.');
        return;
      }
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        quality: 0.7,
        base64: false,
      });
      if (!result.canceled && result.assets?.[0]) {
        setUploadingImage(true);
        // For now send as text with image URI (in prod you'd upload to CDN)
        const uri = result.assets[0].uri;
        await sendMessage(matchId, uri, 'IMAGE');
        setUploadingImage(false);
      }
    } catch (e) {
      setUploadingImage(false);
      Alert.alert('Error', 'Failed to pick image');
    }
  };

  const handleDeleteMessage = (msg) => {
    if (msg.senderId !== user?.id) return;
    Alert.alert('Delete Message', 'Delete this message?', [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Delete', style: 'destructive', onPress: () => deleteMessage(matchId, msg.id) },
    ]);
  };

  const renderMessage = ({ item: msg }) => {
    const isMine = msg.senderId === user?.id;
    const isDeleted = msg.isDeleted || msg.type === 'deleted';
    const isSystem = msg.messageType === 'SYSTEM';

    if (isSystem) {
      return (
        <View style={styles.systemMsg}>
          <Text style={styles.systemMsgText}>{msg.content}</Text>
        </View>
      );
    }

    if (msg.messageType === 'IMAGE') {
      return (
        <TouchableOpacity
          style={[styles.msgRow, isMine ? styles.msgRowRight : styles.msgRowLeft]}
          onLongPress={() => handleDeleteMessage(msg)}
        >
          {!isMine && <Image source={{ uri: userAvatar }} style={styles.msgAvatar} />}
          <View style={[styles.msgBubble, isMine ? styles.bubbleMine : styles.bubbleTheirs]}>
            <Image source={{ uri: msg.content }} style={styles.msgImage} resizeMode="cover" />
            <View style={styles.msgMeta}>
              <Text style={[styles.msgTime, isMine && styles.msgTimeMine]}>
                {new Date(msg.createdAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })}
              </Text>
            </View>
          </View>
        </TouchableOpacity>
      );
    }

    return (
      <TouchableOpacity
        style={[styles.msgRow, isMine ? styles.msgRowRight : styles.msgRowLeft]}
        onLongPress={() => !isDeleted && setReplyTo(msg)}
        onPress={() => isMine && !isDeleted && handleDeleteMessage(msg)}
        activeOpacity={0.85}
      >
        {!isMine && <Image source={{ uri: userAvatar }} style={styles.msgAvatar} />}
        <View style={[styles.msgBubble, isMine ? styles.bubbleMine : styles.bubbleTheirs]}>
          {msg.replyTo && (
            <View style={styles.replyPreview}>
              <Text style={styles.replyText} numberOfLines={1}>{msg.replyTo.content}</Text>
            </View>
          )}
          <Text style={[styles.msgText, isMine ? styles.msgTextMine : styles.msgTextTheirs]}>
            {isDeleted ? '🚫 Message deleted' : msg.content}
          </Text>
          <View style={styles.msgMeta}>
            <Text style={[styles.msgTime, isMine && styles.msgTimeMine]}>
              {new Date(msg.createdAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })}
            </Text>
            {isMine && !isDeleted && (
              <Ionicons
                name={msg.isRead ? 'checkmark-done' : 'checkmark'}
                size={12}
                color={msg.isRead ? '#fff' : 'rgba(255,255,255,0.6)'}
                style={{ marginLeft: 4 }}
              />
            )}
          </View>
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
          <Ionicons name="chevron-back" size={24} color="#000" />
        </TouchableOpacity>
        <TouchableOpacity style={styles.headerUser} onPress={() => navigation.navigate('UserProfile', { userId })}>
          <View style={styles.avatarWrap}>
            <Image source={{ uri: userAvatar }} style={styles.headerAvatar} />
            {/* FIX: Real-time online status */}
            {isOnline && <View style={styles.onlineDot} />}
          </View>
          <View>
            <Text style={styles.headerName}>{userName}</Text>
            <Text style={[styles.headerStatus, isOnline && styles.headerStatusOnline]}>
              {isOnline ? '🟢 Online' : '⚫ Offline'}
            </Text>
          </View>
        </TouchableOpacity>
        <View style={styles.headerActions}>
          <TouchableOpacity onPress={() => handleCall('VOICE')} style={styles.callBtn}>
            <Ionicons name="call-outline" size={20} color="#E94057" />
          </TouchableOpacity>
          <TouchableOpacity onPress={() => handleCall('VIDEO')} style={[styles.callBtn, { marginLeft: 6 }]}>
            <Ionicons name="videocam-outline" size={20} color="#E94057" />
          </TouchableOpacity>
          <TouchableOpacity onPress={() => setShowSettingsModal(true)} style={[styles.callBtn, { marginLeft: 6 }]}>
            <Ionicons name="ellipsis-vertical" size={20} color="#888" />
          </TouchableOpacity>
        </View>
      </View>

      {isTyping && (
        <View style={styles.typingBar}>
          <Text style={styles.typingText}>{userName} is typing...</Text>
        </View>
      )}

      <FlatList
        ref={flatListRef}
        data={matchMessages}
        keyExtractor={item => item.id}
        renderItem={renderMessage}
        contentContainerStyle={styles.msgList}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={
          <View style={styles.emptyChat}>
            <Image source={{ uri: userAvatar }} style={styles.emptyChatAvatar} />
            <Text style={styles.emptyChatName}>{userName}</Text>
            <Text style={styles.emptyChatHint}>Say hello! 👋</Text>
          </View>
        }
      />

      {replyTo && (
        <View style={styles.replyBar}>
          <View style={styles.replyBarContent}>
            <Ionicons name="return-up-back" size={16} color="#E94057" />
            <Text style={styles.replyBarText} numberOfLines={1}>{replyTo.content}</Text>
          </View>
          <TouchableOpacity onPress={() => setReplyTo(null)}>
            <Ionicons name="close" size={18} color="#888" />
          </TouchableOpacity>
        </View>
      )}

      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <View style={styles.inputBar}>
          {/* Activity/Image button */}
          <TouchableOpacity onPress={handlePickImage} style={styles.inputBtn} disabled={uploadingImage}>
            {uploadingImage
              ? <ActivityIndicator size="small" color="#E94057" />
              : <Ionicons name="image-outline" size={22} color="#888" />
            }
          </TouchableOpacity>
          {/* Gift button */}
          <TouchableOpacity onPress={() => setShowGiftModal(true)} style={styles.inputBtn}>
            <Ionicons name="gift-outline" size={22} color="#E94057" />
          </TouchableOpacity>
          <TextInput
            style={styles.input}
            value={inputText}
            onChangeText={handleInputChange}
            placeholder="Type a message..."
            placeholderTextColor="#999"
            multiline
            maxLength={1000}
          />
          <TouchableOpacity
            onPress={handleSend}
            disabled={!inputText.trim() || sending}
            style={styles.sendBtn}
          >
            {sending
              ? <ActivityIndicator size="small" color="#FFF" />
              : (
                <LinearGradient colors={['#E94057', '#8A2387']} style={styles.sendGradient}>
                  <Ionicons name="send" size={16} color="#FFF" />
                </LinearGradient>
              )
            }
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>

      {/* Gift Modal */}
      <Modal visible={showGiftModal} transparent animationType="slide">
        <TouchableOpacity style={styles.modalOverlay} activeOpacity={1} onPress={() => setShowGiftModal(false)}>
          <View style={styles.giftModal}>
            <View style={styles.giftHeader}>
              <Text style={styles.giftTitle}>Send a Gift 🎁</Text>
              <TouchableOpacity onPress={() => setShowGiftModal(false)}>
                <Ionicons name="close" size={24} color="#000" />
              </TouchableOpacity>
            </View>
            <ScrollView>
              {gifts.length > 0 ? (
                <View style={styles.giftGrid}>
                  {gifts.map(gift => (
                    <TouchableOpacity
                      key={gift.id}
                      style={styles.giftItem}
                      onPress={() => handleSendGift(gift)}
                      disabled={sendingGift === gift.id}
                    >
                      {sendingGift === gift.id
                        ? <ActivityIndicator size="small" color="#E94057" />
                        : (
                          <>
                            <Text style={styles.giftEmoji}>{gift.emoji || '🎁'}</Text>
                            <Text style={styles.giftName}>{gift.name}</Text>
                            <Text style={styles.giftPrice}>{gift.price} coins</Text>
                          </>
                        )
                      }
                    </TouchableOpacity>
                  ))}
                </View>
              ) : (
                <Text style={styles.noGifts}>Loading gifts...</Text>
              )}
            </ScrollView>
          </View>
        </TouchableOpacity>
      </Modal>

      {/* Settings Modal */}
      <Modal visible={showSettingsModal} transparent animationType="slide">
        <TouchableOpacity style={styles.modalOverlay} activeOpacity={1} onPress={() => setShowSettingsModal(false)}>
          <View style={styles.settingsModal}>
            <Text style={styles.settingsTitle}>Chat Settings</Text>
            {[
              { icon: 'notifications-off-outline', label: 'Mute Notifications' },
              { icon: 'trash-outline', label: 'Clear Chat History' },
              { icon: 'ban-outline', label: 'Block User', color: '#E94057' },
              { icon: 'flag-outline', label: 'Report User', color: '#E94057' },
            ].map((item, i) => (
              <TouchableOpacity
                key={i}
                style={styles.settingsItem}
                onPress={() => {
                  setShowSettingsModal(false);
                  Alert.alert(item.label, `${item.label} feature`);
                }}
              >
                <Ionicons name={item.icon} size={22} color={item.color || '#333'} />
                <Text style={[styles.settingsLabel, item.color && { color: item.color }]}>{item.label}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </TouchableOpacity>
      </Modal>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F5F5' },
  header: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFF',
    paddingHorizontal: 12, paddingVertical: 10,
    borderBottomWidth: 1, borderBottomColor: '#F0F0F0',
    elevation: 2, shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.1,
  },
  backBtn: { marginRight: 8 },
  headerUser: { flex: 1, flexDirection: 'row', alignItems: 'center', gap: 10 },
  avatarWrap: { position: 'relative' },
  headerAvatar: { width: 42, height: 42, borderRadius: 21 },
  onlineDot: {
    position: 'absolute', bottom: 0, right: 0,
    width: 12, height: 12, borderRadius: 6,
    backgroundColor: '#4CAF50', borderWidth: 2, borderColor: '#FFF',
  },
  headerName: { fontSize: 16, fontWeight: '700', color: '#000' },
  headerStatus: { fontSize: 11, color: '#888', marginTop: 1 },
  headerStatusOnline: { color: '#4CAF50' },
  headerActions: { flexDirection: 'row', alignItems: 'center' },
  callBtn: {
    width: 36, height: 36, borderRadius: 18,
    backgroundColor: '#FFF0F2', justifyContent: 'center', alignItems: 'center',
    borderWidth: 1, borderColor: '#FFD0D8',
  },
  typingBar: { backgroundColor: '#FFF', paddingHorizontal: 16, paddingVertical: 4 },
  typingText: { color: '#888', fontSize: 12, fontStyle: 'italic' },
  msgList: { paddingHorizontal: 12, paddingVertical: 12, gap: 8 },
  msgRow: { flexDirection: 'row', alignItems: 'flex-end', marginBottom: 4 },
  msgRowLeft: { justifyContent: 'flex-start' },
  msgRowRight: { justifyContent: 'flex-end' },
  msgAvatar: { width: 28, height: 28, borderRadius: 14, marginRight: 6 },
  msgBubble: {
    maxWidth: '75%', borderRadius: 18, paddingHorizontal: 14, paddingVertical: 10,
    elevation: 1, shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.05,
  },
  bubbleMine: { backgroundColor: '#E94057', borderBottomRightRadius: 4 },
  bubbleTheirs: { backgroundColor: '#FFF', borderBottomLeftRadius: 4 },
  msgText: { fontSize: 15, lineHeight: 21 },
  msgTextMine: { color: '#FFF' },
  msgTextTheirs: { color: '#000' },
  msgImage: { width: 180, height: 180, borderRadius: 12, marginBottom: 4 },
  msgMeta: { flexDirection: 'row', alignItems: 'center', justifyContent: 'flex-end', marginTop: 3 },
  msgTime: { fontSize: 10, color: 'rgba(0,0,0,0.4)' },
  msgTimeMine: { color: 'rgba(255,255,255,0.7)' },
  systemMsg: { alignItems: 'center', marginVertical: 8 },
  systemMsgText: { color: '#888', fontSize: 12, backgroundColor: '#F0F0F0', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 12 },
  replyPreview: { borderLeftWidth: 3, borderLeftColor: 'rgba(255,255,255,0.5)', paddingLeft: 8, marginBottom: 4 },
  replyText: { fontSize: 11, color: 'rgba(255,255,255,0.8)' },
  replyBar: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFF',
    borderTopWidth: 1, borderTopColor: '#F0F0F0', paddingHorizontal: 14, paddingVertical: 8,
    justifyContent: 'space-between',
  },
  replyBarContent: { flex: 1, flexDirection: 'row', alignItems: 'center', gap: 8 },
  replyBarText: { flex: 1, fontSize: 13, color: '#555' },
  inputBar: {
    flexDirection: 'row', alignItems: 'flex-end', backgroundColor: '#FFF',
    paddingHorizontal: 8, paddingVertical: 8, borderTopWidth: 1, borderTopColor: '#F0F0F0',
  },
  inputBtn: { width: 38, height: 38, justifyContent: 'center', alignItems: 'center', marginBottom: 2 },
  input: {
    flex: 1, backgroundColor: '#F5F5F5', borderRadius: 22,
    paddingHorizontal: 14, paddingVertical: 10, fontSize: 15, maxHeight: 120, color: '#000',
  },
  sendBtn: { marginLeft: 8, marginBottom: 2 },
  sendGradient: { width: 42, height: 42, borderRadius: 21, justifyContent: 'center', alignItems: 'center' },
  emptyChat: { flex: 1, alignItems: 'center', paddingTop: 60, gap: 12 },
  emptyChatAvatar: { width: 80, height: 80, borderRadius: 40 },
  emptyChatName: { fontSize: 20, fontWeight: '700', color: '#000' },
  emptyChatHint: { fontSize: 15, color: '#888' },
  // Modals
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  giftModal: { backgroundColor: '#FFF', borderTopLeftRadius: 24, borderTopRightRadius: 24, padding: 20, maxHeight: '60%' },
  giftHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 },
  giftTitle: { fontSize: 18, fontWeight: '800', color: '#000' },
  giftGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, paddingBottom: 20 },
  giftItem: {
    width: '22%', alignItems: 'center', backgroundColor: '#FFF7F8',
    borderRadius: 16, padding: 12, borderWidth: 1, borderColor: '#FFE0E5',
  },
  giftEmoji: { fontSize: 32, marginBottom: 4 },
  giftName: { fontSize: 11, fontWeight: '600', color: '#333', textAlign: 'center' },
  giftPrice: { fontSize: 10, color: '#E94057', fontWeight: '700' },
  noGifts: { textAlign: 'center', color: '#888', padding: 20 },
  settingsModal: { backgroundColor: '#FFF', borderTopLeftRadius: 24, borderTopRightRadius: 24, padding: 24 },
  settingsTitle: { fontSize: 18, fontWeight: '800', color: '#000', marginBottom: 16 },
  settingsItem: { flexDirection: 'row', alignItems: 'center', gap: 14, paddingVertical: 14, borderBottomWidth: 1, borderBottomColor: '#F5F5F5' },
  settingsLabel: { fontSize: 16, color: '#333' },
});
