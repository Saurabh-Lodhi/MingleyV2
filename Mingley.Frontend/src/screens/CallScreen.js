// CallScreen.js — Full Agora RTC voice + video call
import React, { useState, useEffect, useRef } from 'react';
import {
  View, Text, TouchableOpacity, Image, StyleSheet,
  Alert, Vibration, Platform, SafeAreaView, Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from '../components/compat/LinearGradient';
import { callService } from '../services/api';
import { onCallEnded, onCallDeclined as onCallRejected } from '../services/socket';

const { width, height } = Dimensions.get('window');

export const CallScreen = ({ route, navigation }) => {
  const {
    callId,
    channelName: paramChannel,
    token: paramToken,
    uid: paramUid,
    agoraAppId: paramAppId,
    isInitiator = true,
    callType = 'VOICE',
    targetUser,
    action, // 'answer' | 'decline' for incoming
    caller, // for incoming call
  } = route.params || {};

  const displayUser = targetUser || caller;

  const [callStatus, setCallStatus] = useState(
    action === 'answer' ? 'answered' :
    action === 'decline' ? 'declined' :
    isInitiator ? 'calling' : 'incoming'
  );
  const [callDuration, setCallDuration] = useState(0);
  const [isMuted, setIsMuted] = useState(false);
  const [isSpeaker, setIsSpeaker] = useState(false);
  const [isVideoOff, setIsVideoOff] = useState(false);
  const [agoraReady, setAgoraReady] = useState(false);

  // Agora token/channel (may come from params or from API)
  const [channelName, setChannelName] = useState(paramChannel || `call_${callId}`);
  const [agoraToken, setAgoraToken] = useState(paramToken || null);
  const [agoraAppId, setAgoraAppId] = useState(paramAppId || null);
  const [agoraUid, setAgoraUid] = useState(paramUid || Math.floor(Math.random() * 999999) + 1);

  const timerRef = useRef(null);
  const engineRef = useRef(null);

  useEffect(() => {
    // Decline immediately if that's the action
    if (action === 'decline') {
      handleDecline();
      return;
    }

    // Answer incoming call
    if (action === 'answer') {
      handleAnswer();
      return;
    }

    // Initiate outgoing call
    if (isInitiator) initCall();

    // Vibrate for incoming
    if (!isInitiator && action !== 'answer') Vibration.vibrate([0, 500, 500, 500], true);

    const offEnded = onCallEnded(({ callId: cId }) => {
      if (cId === callId) endCallLocal('Remote ended call');
    });
    const offRejected = onCallRejected(({ callId: cId }) => {
      if (cId === callId) {
        setCallStatus('rejected');
        setTimeout(() => navigation.goBack(), 2000);
      }
    });

    return () => {
      offEnded();
      offRejected();
      Vibration.cancel();
      clearInterval(timerRef.current);
      cleanupAgora();
    };
  }, []);

  // Fetch Agora token from backend if not provided
  const fetchAgoraToken = async () => {
    if (agoraToken && agoraAppId) return { token: agoraToken, appId: agoraAppId, uid: agoraUid };
    try {
      const res = await callService.getAgoraToken(callId);
      const { token, appId, channelName: ch, uid } = res.data.data;
      setAgoraToken(token);
      setAgoraAppId(appId);
      if (ch) setChannelName(ch);
      if (uid) setAgoraUid(uid);
      return { token, appId, channelName: ch || channelName, uid: uid || agoraUid };
    } catch (e) {
      console.log('Token fetch error:', e.message);
      return null;
    }
  };

  const initAgora = async () => {
    try {
      const tokenData = await fetchAgoraToken();
      if (!tokenData?.appId) {
        console.log('No Agora AppId — running in UI-only mode');
        setAgoraReady(false);
        return;
      }

      // Dynamically require react-native-agora
      let AgoraModule;
      try { AgoraModule = require('react-native-agora'); }
      catch { console.log('react-native-agora not installed'); setAgoraReady(false); return; }

      const { createAgoraRtcEngine } = AgoraModule;
      const engine = createAgoraRtcEngine();
      engineRef.current = engine;

      await engine.initialize({ appId: tokenData.appId });
      engine.enableAudio();
      if (callType === 'VIDEO') engine.enableVideo();

      // Register event handlers
      engine.addListener('onUserJoined', (connection, remoteUid) => {
        console.log('Remote user joined:', remoteUid);
        setCallStatus('active');
      });
      engine.addListener('onUserOffline', (connection, remoteUid) => {
        console.log('Remote user offline:', remoteUid);
        endCallLocal('Remote user left');
      });
      engine.addListener('onError', (err, msg) => {
        console.log('Agora error:', err, msg);
      });

      await engine.joinChannel(
        tokenData.token,
        tokenData.channelName || channelName,
        tokenData.uid || agoraUid,
        {
          channelProfile: 0, // Communication
          clientRoleType: 1, // Broadcaster
          publishMicrophoneTrack: true,
          publishCameraTrack: callType === 'VIDEO',
          autoSubscribeAudio: true,
          autoSubscribeVideo: callType === 'VIDEO',
        }
      );

      setAgoraReady(true);
    } catch (e) {
      console.log('Agora init error:', e.message);
      setAgoraReady(false);
    }
  };

  const cleanupAgora = () => {
    try {
      engineRef.current?.leaveChannel();
      engineRef.current?.release();
      engineRef.current = null;
    } catch (e) {}
  };

  const startTimer = () => {
    timerRef.current = setInterval(() => {
      setCallDuration(prev => prev + 1);
    }, 1000);
  };

  const initCall = async () => {
    setCallStatus('ringing');
    await initAgora();
    startTimer();
    setCallStatus('active');
  };

  const handleAnswer = async () => {
    Vibration.cancel();
    try {
      await callService.answer(callId);
      await initAgora();
      startTimer();
      setCallStatus('active');
    } catch (e) {
      Alert.alert('Error', 'Failed to answer call');
      navigation.goBack();
    }
  };

  const handleDecline = async () => {
    try {
      await callService.decline(callId);
    } catch {}
    navigation.goBack();
  };

  const endCallLocal = async (reason = '') => {
    clearInterval(timerRef.current);
    cleanupAgora();
    try { await callService.end(callId); } catch {}
    setCallStatus('ended');
    setTimeout(() => navigation.goBack(), 1500);
  };

  const toggleMute = () => {
    setIsMuted(m => {
      const next = !m;
      try { engineRef.current?.muteLocalAudioStream(next); } catch {}
      return next;
    });
  };

  const toggleSpeaker = () => {
    setIsSpeaker(s => {
      const next = !s;
      try { engineRef.current?.setEnableSpeakerphone(next); } catch {}
      return next;
    });
  };

  const toggleVideo = () => {
    setIsVideoOff(v => {
      const next = !v;
      try { engineRef.current?.muteLocalVideoStream(next); } catch {}
      return next;
    });
  };

  const formatDuration = (secs) => {
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  const isVideo = callType === 'VIDEO';

  const statusText = {
    calling: 'Calling...',
    ringing: 'Ringing...',
    incoming: `Incoming ${isVideo ? 'Video' : 'Voice'} Call`,
    answered: 'Connecting...',
    active: formatDuration(callDuration),
    rejected: 'Call Declined',
    ended: 'Call Ended',
  }[callStatus] || '...';

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={isVideo ? ['#1a1a2e', '#16213e', '#0f3460'] : ['#2b0a3d', '#E94057', '#2b0a3d']}
        style={StyleSheet.absoluteFillObject}
      />

      <SafeAreaView style={styles.safe}>
        {/* Back button */}
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <Ionicons name="chevron-back" size={24} color="#FFF" />
        </TouchableOpacity>

        {/* User info */}
        <View style={styles.userSection}>
          <View style={styles.avatarWrap}>
            <Image
              source={{ uri: displayUser?.avatar || displayUser?.userAvatar || 'https://i.pravatar.cc/150' }}
              style={styles.avatar}
            />
            {callStatus === 'active' && (
              <View style={styles.activePulse} />
            )}
          </View>
          <Text style={styles.userName}>
            {displayUser?.fullName || displayUser?.userName || 'User'}
          </Text>
          <Text style={styles.callStatus}>{statusText}</Text>
          {agoraReady && callStatus === 'active' && (
            <View style={styles.qualityBadge}>
              <Ionicons name="wifi" size={12} color="#4CAF50" />
              <Text style={styles.qualityText}> HD Quality</Text>
            </View>
          )}
        </View>

        {/* Controls */}
        <View style={styles.controls}>
          {callStatus === 'incoming' && action !== 'answer' ? (
            // Incoming call — answer/decline buttons
            <View style={styles.incomingBtns}>
              <TouchableOpacity style={styles.declineBtn} onPress={handleDecline}>
                <Ionicons name="call" size={32} color="#FFF" style={{ transform: [{ rotate: '135deg' }] }} />
              </TouchableOpacity>
              <TouchableOpacity style={styles.answerBtn} onPress={handleAnswer}>
                <Ionicons name="call" size={32} color="#FFF" />
              </TouchableOpacity>
            </View>
          ) : (
            // Active call controls
            <View style={styles.activeControls}>
              <View style={styles.controlRow}>
                <TouchableOpacity
                  style={[styles.ctrlBtn, isMuted && styles.ctrlBtnActive]}
                  onPress={toggleMute}
                >
                  <Ionicons name={isMuted ? 'mic-off' : 'mic'} size={24} color="#FFF" />
                  <Text style={styles.ctrlLabel}>{isMuted ? 'Unmute' : 'Mute'}</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[styles.ctrlBtn, isSpeaker && styles.ctrlBtnActive]}
                  onPress={toggleSpeaker}
                >
                  <Ionicons name={isSpeaker ? 'volume-high' : 'volume-medium'} size={24} color="#FFF" />
                  <Text style={styles.ctrlLabel}>Speaker</Text>
                </TouchableOpacity>

                {isVideo && (
                  <TouchableOpacity
                    style={[styles.ctrlBtn, isVideoOff && styles.ctrlBtnActive]}
                    onPress={toggleVideo}
                  >
                    <Ionicons name={isVideoOff ? 'videocam-off' : 'videocam'} size={24} color="#FFF" />
                    <Text style={styles.ctrlLabel}>{isVideoOff ? 'Camera Off' : 'Camera'}</Text>
                  </TouchableOpacity>
                )}
              </View>

              <TouchableOpacity style={styles.endBtn} onPress={() => endCallLocal('User ended call')}>
                <Ionicons name="call" size={32} color="#FFF" style={{ transform: [{ rotate: '135deg' }] }} />
              </TouchableOpacity>
            </View>
          )}
        </View>
      </SafeAreaView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1 },
  safe: { flex: 1, justifyContent: 'space-between', paddingVertical: 16 },
  backBtn: { padding: 16 },
  userSection: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: 12 },
  avatarWrap: { position: 'relative' },
  avatar: { width: 140, height: 140, borderRadius: 70, borderWidth: 4, borderColor: 'rgba(255,255,255,0.3)' },
  activePulse: {
    position: 'absolute', top: -6, left: -6, right: -6, bottom: -6,
    borderRadius: 76, borderWidth: 2, borderColor: 'rgba(255,255,255,0.3)',
  },
  userName: { fontSize: 28, fontWeight: '800', color: '#FFF', marginTop: 8 },
  callStatus: { fontSize: 16, color: 'rgba(255,255,255,0.8)', fontWeight: '500' },
  qualityBadge: {
    flexDirection: 'row', alignItems: 'center',
    backgroundColor: 'rgba(76,175,80,0.2)', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 20,
  },
  qualityText: { fontSize: 12, color: '#4CAF50', fontWeight: '600' },
  controls: { paddingHorizontal: 32, paddingBottom: 32 },
  incomingBtns: { flexDirection: 'row', justifyContent: 'space-around', alignItems: 'center' },
  declineBtn: { width: 72, height: 72, borderRadius: 36, backgroundColor: '#E94057', justifyContent: 'center', alignItems: 'center' },
  answerBtn: { width: 72, height: 72, borderRadius: 36, backgroundColor: '#4CAF50', justifyContent: 'center', alignItems: 'center' },
  activeControls: { alignItems: 'center', gap: 32 },
  controlRow: { flexDirection: 'row', justifyContent: 'center', gap: 24 },
  ctrlBtn: {
    width: 68, height: 68, borderRadius: 34,
    backgroundColor: 'rgba(255,255,255,0.15)',
    justifyContent: 'center', alignItems: 'center', gap: 4,
  },
  ctrlBtnActive: { backgroundColor: 'rgba(233,64,87,0.5)' },
  ctrlLabel: { fontSize: 10, color: 'rgba(255,255,255,0.8)', textAlign: 'center' },
  endBtn: { width: 72, height: 72, borderRadius: 36, backgroundColor: '#E94057', justifyContent: 'center', alignItems: 'center' },
});
