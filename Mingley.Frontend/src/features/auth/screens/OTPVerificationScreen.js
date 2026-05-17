import React, { useState, useEffect } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, Alert, Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useForm } from 'react-hook-form';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING } from '../../../constants/theme';
import { OTPInput } from '../components/OTPInput';
import { useAuthStore } from '../../../store/useAuthStore';

export const OTPVerificationScreen = ({ navigation, route }) => {
  const { userId, devOtp, type, value, purpose = 'registration' } = route?.params || {};
  const verifyOtp = useAuthStore(state => state.verifyOtp);
  const resendOtp = useAuthStore(state => state.resendOtp);
  const pendingUserId = useAuthStore(state => state.pendingUserId);
  const isLoading = useAuthStore(state => state.isLoading);

  const resolvedUserId = userId || pendingUserId;

  const { control, watch, setValue } = useForm({ defaultValues: { otp: '' } });
  const otpValue = watch('otp');
  const [timer, setTimer] = useState(60);
  const [verified, setVerified] = useState(false);

  // Countdown timer
  useEffect(() => {
    const interval = setInterval(() => {
      setTimer(prev => (prev > 0 ? prev - 1 : 0));
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  // Auto-verify when 6 digits entered
  useEffect(() => {
    if (otpValue?.length === 6 && !verified && resolvedUserId) {
      handleVerify(otpValue);
    }
  }, [otpValue]);

  const handleVerify = async (otp) => {
    if (verified) return;
    setVerified(true);
    const result = await verifyOtp(resolvedUserId, otp, purpose);
    if (!result.success) {
      setVerified(false);
      setValue('otp', '');
      Alert.alert('Verification Failed', result.error || 'Invalid OTP. Please try again.');
    }
    // On success AppNavigator auto-switches to Main
  };

  const handleResend = async () => {
    if (timer > 0 || !resolvedUserId) return;
    const result = await resendOtp(resolvedUserId, purpose);
    if (result.success) {
      setTimer(60);
      Alert.alert('OTP Sent', 'A new OTP has been sent to your device.');
    } else {
      Alert.alert('Error', result.error);
    }
  };

  const formatTimer = (secs) => {
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Icon name="chevron-back" size={24} color="#E94057" />
        </TouchableOpacity>
      </View>

      <View style={styles.content}>
        <Text style={styles.timerText}>{formatTimer(timer)}</Text>
        <Text style={styles.subtitle}>
          Type the 6-digit verification code{type === 'email' && value ? ` sent to ${value}` : ''}
        </Text>

        {/* Always show OTP box prominently on screen in dev mode */}
        {devOtp ? (
          <View style={styles.devHint}>
            <Icon name="information-circle-outline" size={18} color="#856404" />
            <Text style={styles.devHintText}>  Dev OTP: <Text style={styles.devOtpCode}>{devOtp}</Text></Text>
          </View>
        ) : null}

        <OTPInput control={control} name="otp" />

        {isLoading && (
          <View style={styles.loadingRow}>
            <Icon name="reload-outline" size={16} color="#E94057" />
            <Text style={styles.loadingText}>  Verifying...</Text>
          </View>
        )}

        <TouchableOpacity
          style={[styles.resendContainer, timer > 0 && styles.resendDisabledWrap]}
          onPress={handleResend}
          disabled={timer > 0}
        >
          <Text style={[styles.resendText, timer > 0 && styles.resendDisabled]}>
            {timer > 0 ? `Resend code in ${timer}s` : 'Resend OTP'}
          </Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFFFFF' },
  header: { paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  backButton: {
    width: 48, height: 48, borderRadius: 12,
    borderWidth: 1, borderColor: '#F0F0F0',
    justifyContent: 'center', alignItems: 'center',
  },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 60, alignItems: 'center' },
  timerText: { fontSize: 40, fontWeight: 'bold', color: '#000000', marginBottom: 16 },
  subtitle: {
    fontSize: 16, color: '#333333', textAlign: 'center',
    maxWidth: 300, lineHeight: 24, marginBottom: 24,
  },
  devHint: {
    flexDirection: 'row', alignItems: 'center',
    backgroundColor: '#FFF3CD', paddingHorizontal: 16, paddingVertical: 12,
    borderRadius: 12, marginBottom: 24, borderWidth: 1, borderColor: '#FFEEBA',
  },
  devHintText: { fontSize: 15, color: '#856404', fontWeight: '600' },
  devOtpCode: { fontSize: 20, fontWeight: '800', letterSpacing: 4, color: '#533f03' },
  loadingRow: { flexDirection: 'row', alignItems: 'center', marginTop: 16 },
  loadingText: { marginTop: 0, color: '#E94057', fontSize: 14 },
  resendContainer: { marginTop: 'auto', marginBottom: 60, paddingVertical: 12, paddingHorizontal: 24 },
  resendDisabledWrap: {},
  resendText: { fontSize: 16, fontWeight: 'bold', color: '#E94057' },
  resendDisabled: { opacity: 0.4 },
});
