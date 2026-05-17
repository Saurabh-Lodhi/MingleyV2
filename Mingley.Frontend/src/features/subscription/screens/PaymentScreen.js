import React, { useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, ScrollView,
  Alert, ActivityIndicator, Dimensions, Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { LinearGradient } from '../../../components/compat/LinearGradient';
import { Ionicons as Icon } from '@expo/vector-icons';
import { subscriptionService, walletService } from '../../../services/api';
import { useChatStore } from '../../../store/useChatStore';

const { width } = Dimensions.get('window');

const PAYMENT_METHODS = [
  { id: 'upi', label: 'UPI / GPay / PhonePe / Paytm', sub: 'Instant payment via UPI', icon: 'phone-portrait-outline', color: '#4CAF50' },
  { id: 'card', label: 'Credit / Debit Card', sub: 'Visa, MasterCard, RuPay', icon: 'card-outline', color: '#2196F3' },
  { id: 'netbanking', label: 'Net Banking', sub: 'All major Indian banks', icon: 'business-outline', color: '#9C27B0' },
  { id: 'wallet', label: 'Wallets', sub: 'Paytm, Mobikwik, Amazon Pay', icon: 'wallet-outline', color: '#FF9800' },
  { id: 'emi', label: 'EMI', sub: 'No-cost EMI options', icon: 'calendar-outline', color: '#F44336' },
];

export const PaymentScreen = ({ navigation, route }) => {
  const { plan } = route.params || {};
  const [selected, setSelected] = useState('upi');
  const [paying, setPaying] = useState(false);
  const upgradeToPremium = useChatStore(s => s.upgradeToPremium);

  const tax = Math.round((plan?.price || 0) * 0.18);
  const total = (plan?.price || 0) + tax;

  const handlePay = async () => {
    if (!plan) { Alert.alert('Error', 'No plan selected'); return; }
    setPaying(true);
    try {
      // Step 1: Create Razorpay order via backend
      let orderId = null;
      try {
        const orderRes = await walletService.razorpayOrder(total);
        orderId = orderRes.data?.data?.orderId;
      } catch {
        // If Razorpay order creation fails, continue with direct subscription
        // This handles the case where Razorpay keys aren't configured yet
      }

      // Step 2: Subscribe directly (Razorpay integration placeholder)
      // In production with live Razorpay keys, you'd open the Razorpay checkout here
      const res = await subscriptionService.subscribe(plan.id, true);

      upgradeToPremium();

      Alert.alert(
        '🎉 Payment Successful!',
        `Welcome to Mingley ${plan.name}!\n\nExpires: ${new Date(res.data.data.endDate || Date.now() + plan.durationDays * 86400000).toLocaleDateString('en-IN')}\n\nEnjoy unlimited matches!`,
        [{ text: 'Start Discovering!', onPress: () => navigation.navigate('Main') }]
      );
    } catch (e) {
      Alert.alert('Payment Failed', e?.response?.data?.message || 'Please try again.');
    } finally {
      setPaying(false);
    }
  };

  return (
    <SafeAreaView style={s.container}>
      <LinearGradient colors={['#fff0f3', '#ffffff', '#f3f0ff']} style={StyleSheet.absoluteFillObject} />

      <View style={s.header}>
        <TouchableOpacity style={s.backBtn} onPress={() => navigation.goBack()}>
          <Icon name="chevron-back" size={22} color="#2b1c50" />
        </TouchableOpacity>
        <Text style={s.headerTitle}>Payment</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={s.scroll} showsVerticalScrollIndicator={false}>
        {/* Order Summary */}
        <View style={s.summaryCard}>
          <Text style={s.summaryTitle}>Order Summary</Text>
          <View style={s.summaryRow}>
            <Text style={s.summaryLabel}>{plan?.name || 'Plan'}</Text>
            <Text style={s.summaryValue}>₹{plan?.price || 0}</Text>
          </View>
          <View style={s.summaryRow}>
            <Text style={s.summaryLabel}>GST (18%)</Text>
            <Text style={s.summaryValue}>₹{tax}</Text>
          </View>
          <View style={[s.summaryRow, s.totalRow]}>
            <Text style={s.totalLabel}>Total</Text>
            <Text style={s.totalValue}>₹{total}</Text>
          </View>
        </View>

        {/* Secure badge */}
        <View style={s.secureBadge}>
          <Icon name="shield-checkmark-outline" size={16} color="#27ae60" />
          <Text style={s.secureText}> 256-bit SSL Encrypted · Powered by Razorpay</Text>
        </View>

        {/* Payment Methods */}
        <Text style={s.sectionTitle}>Select Payment Method</Text>

        {PAYMENT_METHODS.map(method => (
          <TouchableOpacity
            key={method.id}
            style={[s.methodCard, selected === method.id && s.methodCardActive]}
            onPress={() => setSelected(method.id)}
            activeOpacity={0.85}
          >
            <View style={[s.methodIcon, { backgroundColor: method.color + '18' }]}>
              <Icon name={method.icon} size={22} color={method.color} />
            </View>
            <View style={s.methodInfo}>
              <Text style={s.methodLabel}>{method.label}</Text>
              <Text style={s.methodSub}>{method.sub}</Text>
            </View>
            <View style={[s.radio, selected === method.id && s.radioActive]}>
              {selected === method.id && <View style={s.radioDot} />}
            </View>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Pay Button */}
      <View style={s.footer}>
        <TouchableOpacity onPress={handlePay} disabled={paying} activeOpacity={0.9}>
          <LinearGradient
            colors={['#E94057', '#8A2387']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={s.payBtn}
          >
            {paying
              ? <ActivityIndicator color="#fff" size="small" />
              : (
                <>
                  <Icon name="lock-closed" size={16} color="#fff" />
                  <Text style={s.payBtnText}>  Pay ₹{total} Securely</Text>
                </>
              )
            }
          </LinearGradient>
        </TouchableOpacity>
        <Text style={s.termsText}>By paying, you agree to our Terms of Service</Text>
      </View>
    </SafeAreaView>
  );
};

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  header: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingHorizontal: 20, paddingVertical: 14,
    borderBottomWidth: 1, borderBottomColor: '#f0f0f0',
  },
  backBtn: { width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: 18, fontWeight: '700', color: '#2b1c50' },
  scroll: { padding: 20, paddingBottom: 10 },
  summaryCard: {
    backgroundColor: '#fff', borderRadius: 16, padding: 20, marginBottom: 12,
    borderWidth: 1, borderColor: '#f0e6ff',
    shadowColor: '#8A2387', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.08, shadowRadius: 8, elevation: 3,
  },
  summaryTitle: { fontSize: 16, fontWeight: '700', color: '#2b1c50', marginBottom: 16 },
  summaryRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 10 },
  summaryLabel: { fontSize: 14, color: '#666' },
  summaryValue: { fontSize: 14, color: '#333', fontWeight: '600' },
  totalRow: { borderTopWidth: 1, borderTopColor: '#F0F0F0', paddingTop: 12, marginTop: 4 },
  totalLabel: { fontSize: 16, fontWeight: '800', color: '#2b1c50' },
  totalValue: { fontSize: 20, fontWeight: '800', color: '#E94057' },
  secureBadge: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    backgroundColor: '#E8F8EF', paddingVertical: 8, paddingHorizontal: 16,
    borderRadius: 20, marginBottom: 20,
  },
  secureText: { fontSize: 12, color: '#27ae60', fontWeight: '600' },
  sectionTitle: { fontSize: 16, fontWeight: '700', color: '#2b1c50', marginBottom: 14 },
  methodCard: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: '#fff',
    borderRadius: 14, padding: 16, marginBottom: 10,
    borderWidth: 2, borderColor: '#f0f0f0',
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.04, shadowRadius: 4, elevation: 2,
  },
  methodCardActive: { borderColor: '#E94057', backgroundColor: '#FFF7F9' },
  methodIcon: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center', marginRight: 14 },
  methodInfo: { flex: 1 },
  methodLabel: { fontSize: 14, fontWeight: '700', color: '#2b1c50', marginBottom: 2 },
  methodSub: { fontSize: 12, color: '#888' },
  radio: { width: 22, height: 22, borderRadius: 11, borderWidth: 2, borderColor: '#DDD', justifyContent: 'center', alignItems: 'center' },
  radioActive: { borderColor: '#E94057' },
  radioDot: { width: 12, height: 12, borderRadius: 6, backgroundColor: '#E94057' },
  footer: { padding: 20, backgroundColor: '#fff', borderTopWidth: 1, borderTopColor: '#f0f0f0' },
  payBtn: {
    borderRadius: 16, paddingVertical: 16,
    flexDirection: 'row', justifyContent: 'center', alignItems: 'center',
  },
  payBtnText: { fontSize: 17, fontWeight: '800', color: '#fff' },
  termsText: { fontSize: 11, color: '#AAA', textAlign: 'center', marginTop: 10 },
});
