import React, { useState, useMemo, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING, TYPOGRAPHY } from '../../../constants/theme';
import { MatchesGridItem } from '../components/MatchesGridItem';
import { useUserStore } from '../../../store/useUserStore';

export const MatchesScreen = () => {
  const { matches, loadMatches, removeMatch } = useUserStore();
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => { loadMatches(); }, []);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadMatches();
    setRefreshing(false);
  }, []);

  const { today, yesterday } = useMemo(() => ({
    today: matches.filter(m => m.section === 'Today'),
    yesterday: matches.filter(m => m.section !== 'Today'),
  }), [matches]);

  const handleAccept = (match) => {
    Alert.alert('Matched!', `Say hello to ${match.name}!`, [{ text: 'OK' }]);
  };

  const handleReject = (match) => {
    Alert.alert('Remove Match', `Remove ${match.name} from your matches?`, [
      { text: 'Remove', style: 'destructive', onPress: () => removeMatch(match.id) },
      { text: 'Cancel', style: 'cancel' },
    ]);
  };

  const SectionSeparator = ({ title, count }) => (
    <View style={styles.sectionHeader}>
      <View style={styles.line} />
      <Text style={styles.sectionTitle}>{title} {count > 0 ? `· ${count}` : ''}</Text>
      <View style={styles.line} />
    </View>
  );

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#E94057" />}
      >
        <View style={styles.headerContainer}>
          <View style={styles.headerTop}>
            <Text style={styles.title}>Matches</Text>
            <TouchableOpacity style={styles.sortButton}>
              <Icon name="swap-vertical" size={22} color="#E94057" />
            </TouchableOpacity>
          </View>
          <Text style={styles.subtitle}>People who have liked you and your matches.</Text>
        </View>

        {today.length > 0 && (
          <>
            <SectionSeparator title="Today" count={today.length} />
            <View style={styles.grid}>
              {today.map(item => (
                <View key={item.id} style={styles.gridCell}>
                  <MatchesGridItem match={item} onPress={() => {}} onAccept={handleAccept} onReject={handleReject} />
                </View>
              ))}
            </View>
          </>
        )}

        {yesterday.length > 0 && (
          <>
            <SectionSeparator title="Earlier" count={yesterday.length} />
            <View style={styles.grid}>
              {yesterday.map(item => (
                <View key={item.id} style={styles.gridCell}>
                  <MatchesGridItem match={item} onPress={() => {}} onAccept={handleAccept} onReject={handleReject} />
                </View>
              ))}
            </View>
          </>
        )}

        {matches.length === 0 && !refreshing && (
          <View style={styles.emptyState}>
            <Icon name="heart-dislike-outline" size={48} color="#DDD" />
            <Text style={styles.emptyText}>No matches yet</Text>
            <Text style={styles.emptySubText}>Keep swiping to find your match!</Text>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFFFFF' },
  scrollContent: { paddingBottom: 30 },
  headerContainer: { marginTop: SPACING.l, marginBottom: SPACING.m, paddingHorizontal: SPACING.l },
  headerTop: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SPACING.xs },
  title: { ...TYPOGRAPHY.h1, color: '#000', fontSize: 36 },
  sortButton: { width: 42, height: 42, borderRadius: 14, borderWidth: 1, borderColor: '#F0F0F0', justifyContent: 'center', alignItems: 'center' },
  subtitle: { ...TYPOGRAPHY.body, color: '#888', lineHeight: 20, fontSize: 13 },
  sectionHeader: { flexDirection: 'row', alignItems: 'center', marginVertical: SPACING.m, paddingHorizontal: SPACING.l },
  line: { flex: 1, height: 1, backgroundColor: '#F0F0F0' },
  sectionTitle: { fontSize: 12, color: '#AAA', marginHorizontal: 12, fontWeight: '600' },
  grid: { flexDirection: 'row', flexWrap: 'wrap', paddingHorizontal: SPACING.s },
  gridCell: { width: '50%' },
  emptyState: { alignItems: 'center', paddingTop: 60, paddingBottom: 40 },
  emptyText: { fontSize: 18, fontWeight: '700', color: '#CCC', marginTop: 16 },
  emptySubText: { fontSize: 13, color: '#CCC', marginTop: 6 },
});
