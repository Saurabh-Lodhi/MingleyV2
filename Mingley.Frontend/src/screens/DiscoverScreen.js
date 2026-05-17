// DiscoverScreen.js — infinite loop when no more users + filter support
import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, Image, TouchableOpacity,
  ActivityIndicator, Dimensions, Alert, SafeAreaView, Modal, ScrollView,
} from 'react-native';
import { LinearGradient } from '../components/compat/LinearGradient';
import { Ionicons } from '@expo/vector-icons';
import { useUserStore } from '../store/useUserStore';
import { useAuthStore } from '../store/useAuthStore';
import { superChatService } from '../services/api';

const { width, height } = Dimensions.get('window');

const FILTER_DEFAULTS = { minAge: 18, maxAge: 45, gender: '', maxDistance: 100 };

export const DiscoverScreen = ({ navigation }) => {
  const { feed, feedLoading, loadFeed, swipe } = useUserStore();
  const { user } = useAuthStore();
  const [currentIdx, setCurrentIdx] = useState(0);
  const [actionLoading, setActionLoading] = useState(false);
  const [showFilter, setShowFilter] = useState(false);
  const [filters, setFilters] = useState(FILTER_DEFAULTS);
  const [activeFilters, setActiveFilters] = useState(FILTER_DEFAULTS);

  useEffect(() => { loadFeed(true, activeFilters); }, []);

  // ── Infinite loop: when we reach end, wrap around ──────────
  const totalFeed = feed.length;
  const safeIdx = totalFeed > 0 ? currentIdx % totalFeed : 0;
  const currentUser = totalFeed > 0 ? feed[safeIdx] : null;

  const handleSwipe = async (action) => {
    if (!currentUser || actionLoading) return;
    setActionLoading(true);
    try {
      const result = await swipe(currentUser.id, action);
      if (result.isMatch) {
        Alert.alert(
          "🎉 It's a Match!",
          `You and ${currentUser.fullName} liked each other!`,
          [
            {
              text: 'Chat Now',
              onPress: () => navigation.navigate('Chat', {
                matchId: result.matchId,
                userId: currentUser.id,
                userName: currentUser.fullName,
                userAvatar: currentUser.avatar,
                isOnline: currentUser.isOnline,
              }),
            },
            { text: 'Keep Swiping', style: 'cancel' },
          ]
        );
      }
      // Move to next, wrapping around if needed
      setCurrentIdx(i => (i + 1) % Math.max(totalFeed, 1));
    } catch (err) {
      Alert.alert('Error', err.message || 'Swipe failed');
    } finally {
      setActionLoading(false);
    }
  };

  const handleSuperChat = () => {
    if (!currentUser) return;
    navigation.navigate('SuperChat', { targetUser: currentUser });
  };

  const applyFilters = () => {
    setActiveFilters({ ...filters });
    setShowFilter(false);
    setCurrentIdx(0);
    loadFeed(true, filters);
  };

  const resetFilters = () => {
    setFilters(FILTER_DEFAULTS);
    setActiveFilters(FILTER_DEFAULTS);
    setShowFilter(false);
    setCurrentIdx(0);
    loadFeed(true, FILTER_DEFAULTS);
  };

  if (feedLoading && totalFeed === 0) {
    return (
      <View style={styles.loading}>
        <ActivityIndicator size="large" color="#E94057" />
        <Text style={{ color: '#E94057', marginTop: 12 }}>Finding matches...</Text>
      </View>
    );
  }

  if (!currentUser && !feedLoading) {
    return (
      <SafeAreaView style={styles.empty}>
        <Text style={{ fontSize: 40 }}>😔</Text>
        <Text style={styles.emptyTitle}>No profiles yet</Text>
        <Text style={styles.emptySub}>Check back later or adjust your filters!</Text>
        <TouchableOpacity onPress={() => { setCurrentIdx(0); loadFeed(true, activeFilters); }} style={styles.refreshBtn}>
          <LinearGradient colors={['#E94057', '#8A2387']} style={styles.refreshGrad}>
            <Text style={{ color: '#FFF', fontWeight: '700' }}>Refresh</Text>
          </LinearGradient>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  const age = currentUser?.dateOfBirth || currentUser?.age
    ? (currentUser?.age || Math.floor((Date.now() - new Date(currentUser.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000)))
    : '';

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.logo}>Mingley</Text>
        <View style={styles.headerRight}>
          <TouchableOpacity onPress={() => setShowFilter(true)} style={styles.filterBtn}>
            <Ionicons name="options-outline" size={22} color="#E94057" />
          </TouchableOpacity>
          <TouchableOpacity onPress={() => navigation.navigate('Notifications')}>
            <Ionicons name="notifications-outline" size={24} color="#E94057" />
          </TouchableOpacity>
        </View>
      </View>

      {/* Profile Card */}
      {currentUser && (
        <View style={styles.card}>
          <Image
            source={{ uri: currentUser.avatar || 'https://i.pravatar.cc/400?u=' + currentUser.id }}
            style={styles.cardImage}
          />
          <LinearGradient
            colors={['transparent', 'rgba(0,0,0,0.85)']}
            style={styles.cardGradient}
          />
          <View style={styles.cardInfo}>
            <Text style={styles.cardName}>
              {currentUser.fullName}{age ? `, ${age}` : ''}
            </Text>
            {currentUser.location && (
              <View style={styles.locationRow}>
                <Ionicons name="location-outline" size={14} color="#FFF" />
                <Text style={styles.locationText}> {currentUser.location}</Text>
              </View>
            )}
            {currentUser.interests?.length > 0 && (
              <View style={styles.interestRow}>
                {currentUser.interests.slice(0, 3).map((i, idx) => (
                  <View key={idx} style={styles.interestChip}>
                    <Text style={styles.interestText}>{i}</Text>
                  </View>
                ))}
              </View>
            )}
          </View>

          {/* Loop indicator */}
          {totalFeed > 0 && (
            <View style={styles.loopIndicator}>
              <Text style={styles.loopText}>{(safeIdx + 1)}/{totalFeed}</Text>
            </View>
          )}
        </View>
      )}

      {/* Action Buttons */}
      <View style={styles.actions}>
        <TouchableOpacity
          style={[styles.actionBtn, styles.passBtn]}
          onPress={() => handleSwipe('pass')}
          disabled={actionLoading}
        >
          <Ionicons name="close" size={32} color="#FF4E6A" />
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.actionBtn, styles.superBtn]}
          onPress={handleSuperChat}
          disabled={actionLoading}
        >
          <Ionicons name="star" size={24} color="#FFD700" />
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.actionBtn, styles.likeBtn]}
          onPress={() => handleSwipe('like')}
          disabled={actionLoading}
        >
          {actionLoading
            ? <ActivityIndicator size="small" color="#FFF" />
            : <Ionicons name="heart" size={32} color="#FFF" />
          }
        </TouchableOpacity>
      </View>

      {/* Filter Modal */}
      <Modal visible={showFilter} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={styles.filterModal}>
            <View style={styles.filterHeader}>
              <Text style={styles.filterTitle}>Filters</Text>
              <TouchableOpacity onPress={() => setShowFilter(false)}>
                <Ionicons name="close" size={24} color="#000" />
              </TouchableOpacity>
            </View>
            <ScrollView>
              <Text style={styles.filterLabel}>Gender</Text>
              <View style={styles.filterRow}>
                {['', 'male', 'female', 'other'].map(g => (
                  <TouchableOpacity
                    key={g}
                    style={[styles.filterChip, filters.gender === g && styles.filterChipActive]}
                    onPress={() => setFilters(f => ({ ...f, gender: g }))}
                  >
                    <Text style={[styles.filterChipText, filters.gender === g && styles.filterChipTextActive]}>
                      {g === '' ? 'All' : g.charAt(0).toUpperCase() + g.slice(1)}
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>

              <Text style={styles.filterLabel}>Age Range: {filters.minAge} – {filters.maxAge}</Text>
              <View style={styles.filterRow}>
                {[18, 21, 25].map(a => (
                  <TouchableOpacity
                    key={a}
                    style={[styles.filterChip, filters.minAge === a && styles.filterChipActive]}
                    onPress={() => setFilters(f => ({ ...f, minAge: a }))}
                  >
                    <Text style={[styles.filterChipText, filters.minAge === a && styles.filterChipTextActive]}>
                      {a}+
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>
            </ScrollView>

            <View style={styles.filterActions}>
              <TouchableOpacity style={styles.filterResetBtn} onPress={resetFilters}>
                <Text style={styles.filterResetText}>Reset</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.filterApplyBtn} onPress={applyFilters}>
                <LinearGradient colors={['#E94057', '#8A2387']} style={styles.filterApplyGrad}>
                  <Text style={styles.filterApplyText}>Apply</Text>
                </LinearGradient>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F5F5' },
  loading: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  empty: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: 12, padding: 24 },
  emptyTitle: { fontSize: 24, fontWeight: '700', color: '#000' },
  emptySub: { fontSize: 14, color: '#888', textAlign: 'center' },
  refreshBtn: { marginTop: 8 },
  refreshGrad: { paddingHorizontal: 32, paddingVertical: 14, borderRadius: 24 },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: 20, paddingVertical: 12, backgroundColor: '#FFF',
    borderBottomWidth: 1, borderBottomColor: '#F0F0F0',
  },
  headerRight: { flexDirection: 'row', alignItems: 'center', gap: 16 },
  filterBtn: { padding: 4 },
  logo: { fontSize: 24, fontWeight: '800', color: '#E94057' },
  card: {
    margin: 16, borderRadius: 24, overflow: 'hidden',
    height: height * 0.55, backgroundColor: '#EEE',
    shadowColor: '#000', shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15, shadowRadius: 12, elevation: 8,
  },
  cardImage: { width: '100%', height: '100%' },
  cardGradient: { position: 'absolute', left: 0, right: 0, bottom: 0, height: '50%' },
  cardInfo: { position: 'absolute', bottom: 0, left: 0, right: 0, padding: 20 },
  cardName: { fontSize: 28, fontWeight: '800', color: '#FFF', marginBottom: 4 },
  locationRow: { flexDirection: 'row', alignItems: 'center', marginBottom: 8 },
  locationText: { color: '#FFF', fontSize: 13 },
  interestRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 6 },
  interestChip: { backgroundColor: 'rgba(255,255,255,0.25)', paddingHorizontal: 10, paddingVertical: 4, borderRadius: 20 },
  interestText: { color: '#FFF', fontSize: 12, fontWeight: '600' },
  loopIndicator: {
    position: 'absolute', top: 12, right: 12,
    backgroundColor: 'rgba(0,0,0,0.45)', paddingHorizontal: 10, paddingVertical: 4, borderRadius: 20,
  },
  loopText: { color: '#FFF', fontSize: 12, fontWeight: '600' },
  actions: {
    flexDirection: 'row', justifyContent: 'center',
    alignItems: 'center', gap: 20, paddingVertical: 20,
  },
  actionBtn: {
    width: 64, height: 64, borderRadius: 32,
    justifyContent: 'center', alignItems: 'center',
    shadowColor: '#000', shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15, shadowRadius: 8, elevation: 6,
  },
  passBtn: { backgroundColor: '#FFF', borderWidth: 2, borderColor: '#FF4E6A' },
  superBtn: { width: 52, height: 52, borderRadius: 26, backgroundColor: '#FFF', borderWidth: 2, borderColor: '#FFD700' },
  likeBtn: { backgroundColor: '#E94057' },
  // Filter Modal
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  filterModal: {
    backgroundColor: '#FFF', borderTopLeftRadius: 24, borderTopRightRadius: 24,
    padding: 24, maxHeight: '70%',
  },
  filterHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 },
  filterTitle: { fontSize: 20, fontWeight: '800', color: '#000' },
  filterLabel: { fontSize: 14, fontWeight: '600', color: '#555', marginBottom: 10, marginTop: 16 },
  filterRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 10 },
  filterChip: {
    paddingHorizontal: 16, paddingVertical: 8, borderRadius: 20,
    borderWidth: 1, borderColor: '#E0E0E0', backgroundColor: '#F9F9F9',
  },
  filterChipActive: { borderColor: '#E94057', backgroundColor: '#FFF0F3' },
  filterChipText: { fontSize: 14, color: '#666' },
  filterChipTextActive: { color: '#E94057', fontWeight: '700' },
  filterActions: { flexDirection: 'row', gap: 12, marginTop: 24 },
  filterResetBtn: {
    flex: 1, paddingVertical: 14, borderRadius: 14,
    borderWidth: 1, borderColor: '#E0E0E0', alignItems: 'center',
  },
  filterResetText: { fontSize: 15, color: '#666', fontWeight: '600' },
  filterApplyBtn: { flex: 2 },
  filterApplyGrad: { paddingVertical: 14, borderRadius: 14, alignItems: 'center' },
  filterApplyText: { fontSize: 15, color: '#FFF', fontWeight: '700' },
});
