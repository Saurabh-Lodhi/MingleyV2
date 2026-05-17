import React, { useState, useEffect } from 'react';
import { Modal, View, Text, StyleSheet, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons as Icon } from '@expo/vector-icons';
import { privacyService } from '../services/api';

export const PrivacyPolicyModal = ({ visible, matchId, onAccept, onClose }) => {
  const [policy, setPolicy] = useState(null);
  const [loading, setLoading] = useState(true);
  const [accepting, setAccepting] = useState(false);

  useEffect(() => {
    if (visible) {
      privacyService.getPolicy()
        .then(res => setPolicy(res.data.data))
        .catch(() => setPolicy({ title: 'Match Agreement', content: 'Do not share personal contact info. Be safe and respectful.' }))
        .finally(() => setLoading(false));
    }
  }, [visible]);

  const handleAccept = async () => {
    setAccepting(true);
    try {
      if (matchId) await privacyService.acceptForMatch(matchId);
    } catch (e) { console.log('Privacy accept error:', e?.response?.data); }
    finally { setAccepting(false); onAccept?.(); }
  };

  return (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
      <View style={styles.overlay}>
        <View style={styles.modal}>
          <View style={styles.handleBar} />
          <View style={styles.iconWrap}>
            <Icon name="shield-checkmark-outline" size={40} color="#E94057" />
          </View>
          <Text style={styles.title}>{policy?.title || 'Safety Agreement'}</Text>

          {loading
            ? <ActivityIndicator color="#E94057" style={{ margin: 20 }} />
            : (
              <ScrollView style={styles.scroll} showsVerticalScrollIndicator={false}>
                <Text style={styles.content}>{policy?.content}</Text>
              </ScrollView>
            )
          }

          <TouchableOpacity style={styles.acceptBtn} onPress={handleAccept} disabled={accepting}>
            {accepting
              ? <ActivityIndicator color="#fff" />
              : <Text style={styles.acceptText}>I Understand & Accept ✓</Text>
            }
          </TouchableOpacity>
          <TouchableOpacity style={styles.closeBtn} onPress={onClose}>
            <Text style={styles.closeText}>Read Later</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modal: { backgroundColor: '#fff', borderTopLeftRadius: 28, borderTopRightRadius: 28, paddingHorizontal: 24, paddingBottom: 40, maxHeight: '85%' },
  handleBar: { width: 40, height: 4, backgroundColor: '#E0E0E0', borderRadius: 2, alignSelf: 'center', marginTop: 12, marginBottom: 20 },
  iconWrap: { alignItems: 'center', marginBottom: 12 },
  title: { fontSize: 20, fontWeight: '800', color: '#111', textAlign: 'center', marginBottom: 16 },
  scroll: { maxHeight: 280, marginBottom: 20 },
  content: { fontSize: 13, color: '#444', lineHeight: 20 },
  acceptBtn: { backgroundColor: '#E94057', borderRadius: 14, paddingVertical: 14, alignItems: 'center', marginBottom: 10 },
  acceptText: { color: '#fff', fontSize: 15, fontWeight: '700' },
  closeBtn: { alignItems: 'center', paddingVertical: 8 },
  closeText: { color: '#999', fontSize: 13 },
});
