import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  Alert, ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { LinearGradient } from '../../../components/compat/LinearGradient';
import { Ionicons as Icon } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { subscriptionService } from '../../../services/api';
import { useChatStore } from '../../../store/useChatStore';

export const SubscriptionPlansScreen = () => {
  const navigation = useNavigation();
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedPlan, setSelectedPlan] = useState(null);

  useEffect(() => { loadPlans(); }, []);

  const loadPlans = async () => {
    try {
      const res = await subscriptionService.getPlans();
      const fetchedPlans = res.data.data.plans || [];
      setPlans(fetchedPlans);
      // Do NOT auto-select — user must choose
    } catch (e) {
      console.log('Plans error:', e?.response?.data);
      Alert.alert('Error', 'Failed to load plans. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleContinue = () => {
    if (!selectedPlan) {
      Alert.alert('Select a Plan', 'Please select a subscription plan to continue.');
      return;
    }
    // Navigate to payment screen with the chosen plan
    navigation.navigate('Payment', { plan: selectedPlan });
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <ActivityIndicator size="large" color="#E94057" style={{ flex: 1 }} />
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
          <Icon name="chevron-back" size={24} color="#000" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Choose Your Plan</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={styles.subtitle}>Unlock unlimited swipes, super likes, and more</Text>

        {plans.map(plan => {
          const features = (() => {
            try { return JSON.parse(plan.features || plan.featuresJson || '[]'); }
            catch { return []; }
          })();
          const isSelected = selectedPlan?.id === plan.id;
          const isPopular = plan.isPopular;

          return (
            <TouchableOpacity
              key={plan.id}
              style={[styles.card, isSelected && styles.cardSelected, isPopular && !isSelected && styles.cardPopular]}
              onPress={() => setSelectedPlan(plan)}
              activeOpacity={0.85}
            >
              {isPopular && (
                <View style={styles.popularBadge}>
                  <Text style={styles.popularBadgeText}>⭐ Most Popular</Text>
                </View>
              )}
              {isSelected && (
                <View style={styles.selectedBadge}>
                  <Icon name="checkmark-circle" size={20} color="#E94057" />
                  <Text style={styles.selectedBadgeText}> Selected</Text>
                </View>
              )}
              <View style={styles.cardHeader}>
                <Text style={styles.planName}>{plan.name}</Text>
                <View style={styles.priceWrap}>
                  <Text style={styles.price}>₹{plan.price}</Text>
                  <Text style={styles.duration}>/{plan.durationDays} days</Text>
                </View>
              </View>
              {features.slice(0, 5).map((f, i) => (
                <View key={i} style={styles.featureRow}>
                  <Icon name="checkmark-circle" size={16} color="#E94057" />
                  <Text style={styles.featureText}>{f}</Text>
                </View>
              ))}
            </TouchableOpacity>
          );
        })}
      </ScrollView>

      <View style={styles.footer}>
        <TouchableOpacity
          onPress={handleContinue}
          activeOpacity={0.9}
          style={[styles.continueBtn, !selectedPlan && styles.continueBtnDisabled]}
        >
          <LinearGradient
            colors={selectedPlan ? ['#E94057', '#8A2387'] : ['#CCC', '#AAA']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.continueGrad}
          >
            <Text style={styles.continueBtnText}>
              {selectedPlan
                ? `Continue with ${selectedPlan.name} – ₹${selectedPlan.price}`
                : 'Select a Plan to Continue'
              }
            </Text>
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F7F7F7' },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: 20, paddingVertical: 12,
    backgroundColor: '#fff', borderBottomWidth: 1, borderBottomColor: '#F0F0F0',
  },
  backBtn: { width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: 18, fontWeight: '700', color: '#000' },
  scroll: { padding: 16, paddingBottom: 20 },
  subtitle: { fontSize: 14, color: '#888', textAlign: 'center', marginBottom: 20, lineHeight: 20 },
  card: {
    backgroundColor: '#fff', borderRadius: 20, padding: 20, marginBottom: 16,
    shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.05, shadowRadius: 8, elevation: 3,
    borderWidth: 2, borderColor: 'transparent',
  },
  cardSelected: { borderColor: '#E94057', backgroundColor: '#FFF7F8' },
  cardPopular: { borderColor: '#E94057', borderWidth: 2 },
  popularBadge: {
    backgroundColor: '#FFF0F3', paddingHorizontal: 12, paddingVertical: 4,
    borderRadius: 20, alignSelf: 'flex-start', marginBottom: 12,
  },
  popularBadgeText: { fontSize: 12, fontWeight: '700', color: '#E94057' },
  selectedBadge: {
    flexDirection: 'row', alignItems: 'center',
    backgroundColor: '#FFF0F3', paddingHorizontal: 12, paddingVertical: 4,
    borderRadius: 20, alignSelf: 'flex-start', marginBottom: 12,
  },
  selectedBadgeText: { fontSize: 12, fontWeight: '700', color: '#E94057' },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: 16 },
  planName: { fontSize: 20, fontWeight: '800', color: '#111' },
  priceWrap: { flexDirection: 'row', alignItems: 'flex-end' },
  price: { fontSize: 24, fontWeight: '800', color: '#E94057' },
  duration: { fontSize: 12, color: '#888', marginBottom: 3, marginLeft: 2 },
  featureRow: { flexDirection: 'row', alignItems: 'center', gap: 8, marginBottom: 8 },
  featureText: { fontSize: 13, color: '#444', flex: 1 },
  footer: { padding: 16, backgroundColor: '#fff', borderTopWidth: 1, borderTopColor: '#F0F0F0' },
  continueBtn: { borderRadius: 16 },
  continueBtnDisabled: { opacity: 0.8 },
  continueGrad: { paddingVertical: 16, alignItems: 'center', borderRadius: 16 },
  continueBtnText: { fontSize: 15, fontWeight: '700', color: '#fff' },
});
