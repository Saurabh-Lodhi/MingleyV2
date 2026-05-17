import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity,
  StyleSheet, Modal, Alert, KeyboardAvoidingView, Platform
} from 'react-native';
import { Ionicons as Icon } from '@expo/vector-icons';
import { useChatStore } from '../store/useChatStore';
import { BottomSheetContainer } from './common/BottomSheetContainer';

export const DepositModal = ({ visible, onClose }) => {
  const [utrIdText, setUtrIdText] = useState('');

  const handleDepositSubmit = () => {
    if (!utrIdText.trim()) { Alert.alert('Error', 'Please enter your UTR ID.'); return; }
    Alert.alert('Screenshot Upload', 'Please upload a screenshot of your payment.', [
      { text: 'Mock Upload', onPress: () => {
          Alert.alert('Success', 'Deposit request submitted. Your coins will reflect soon.');
          onClose();
          setUtrIdText('');
      }},
      { text: 'Cancel', style: 'cancel' }
    ]);
  };

  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <BottomSheetContainer onClose={onClose} height={500}>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : ''} style={{ width: '100%', flex: 1 }}>
          <Text style={styles.modalTitle}>Deposit Coins</Text>
          <Text style={styles.depositNote}>1. Scan QR or transfer to bank details.</Text>
          <Text style={styles.depositNote}>2. Submit UTR ID and upload screenshot.</Text>
          <View style={styles.qrContainer}>
            <Icon name="qr-code-outline" size={80} color="#333" />
            <Text style={styles.bankDetailsText}>Acc: 1234567890</Text>
            <Text style={styles.bankDetailsText}>IFSC: MING999 • UPI: mingley@axl</Text>
          </View>
          <TextInput style={styles.amountInput} placeholder="Enter Payment UTR ID" placeholderTextColor="#A0A0A0" value={utrIdText} onChangeText={setUtrIdText} />
          <TouchableOpacity style={[styles.modalActionBtn, !utrIdText && styles.modalActionBtnDisabled]} onPress={handleDepositSubmit} disabled={!utrIdText}>
            <Text style={styles.modalActionBtnText}>Upload Screenshot & Submit</Text>
          </TouchableOpacity>
        </KeyboardAvoidingView>
      </BottomSheetContainer>
    </Modal>
  );
};

export const CashoutModal = ({ visible, onClose }) => {
  const [cashoutInputText, setCashoutInputText] = useState('');
  const [bankOrUpiText, setBankOrUpiText] = useState('');
  const wallet = useChatStore((s) => s.wallet);
  const withdrawCoins = useChatStore((s) => s.withdrawCoins);

  const handleWithdraw = () => {
    const amount = parseInt(cashoutInputText, 10);
    if (!bankOrUpiText.trim()) { Alert.alert('Error', 'Please enter your Bank Details or UPI ID.'); return; }
    const result = withdrawCoins(amount);
    if (!result.ok) { Alert.alert('Withdrawal failed', result.reason); return; }
    onClose();
    setCashoutInputText('');
    setBankOrUpiText('');
    Alert.alert('Success', `${amount} coins withdrawn to ${bankOrUpiText}.`);
  };

  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <BottomSheetContainer onClose={onClose} height={460}>
        <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : ''} style={{ width: '100%', flex: 1 }}>
          <Text style={styles.modalTitle}>Withdraw Coins</Text>
          <Text style={styles.modalSub}>Available: <Text style={styles.modalSubBold}>{wallet.coins} coins</Text></Text>
          <Text style={styles.cashoutNote}>Rs 1 per coin will be credited within 3-5 business days.</Text>
          <TextInput style={styles.amountInput} placeholder="Bank Details or UPI ID" placeholderTextColor="#A0A0A0" value={bankOrUpiText} onChangeText={setBankOrUpiText} />
          <TextInput style={styles.amountInput} placeholder="Enter coins to withdraw" placeholderTextColor="#A0A0A0" keyboardType="numeric" value={cashoutInputText} onChangeText={setCashoutInputText} />
          {cashoutInputText !== '' && parseInt(cashoutInputText, 10) > wallet.coins && (
            <Text style={styles.cashoutError}>Insufficient coins.</Text>
          )}
          <TouchableOpacity
            style={[styles.modalActionBtn, styles.cashoutBtn, (!cashoutInputText || parseInt(cashoutInputText, 10) > wallet.coins || !bankOrUpiText) && styles.modalActionBtnDisabled]}
            onPress={handleWithdraw}
            disabled={!cashoutInputText || parseInt(cashoutInputText, 10) > wallet.coins || !bankOrUpiText}
          >
            <Icon name="cash-outline" size={16} color="#fff" style={{ marginRight: 6 }} />
            <Text style={styles.modalActionBtnText}>Withdraw</Text>
          </TouchableOpacity>
        </KeyboardAvoidingView>
      </BottomSheetContainer>
    </Modal>
  );
};

const styles = StyleSheet.create({
  modalTitle: { fontSize: 20, fontWeight: '700', color: '#111', marginBottom: 4 },
  modalSub: { fontSize: 13, color: '#777', marginBottom: 20 },
  modalSubBold: { fontWeight: '700', color: '#E94057' },
  amountInput: {
    borderWidth: 1.5, borderColor: '#F0F0F0', borderRadius: 14,
    paddingHorizontal: 16, paddingVertical: 12, fontSize: 16, color: '#111',
    marginBottom: 16, backgroundColor: '#FAFAFA',
  },
  modalActionBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    backgroundColor: '#E94057', paddingVertical: 14, borderRadius: 14,
  },
  modalActionBtnDisabled: { backgroundColor: '#D0D0D0' },
  modalActionBtnText: { fontSize: 15, fontWeight: '700', color: '#fff' },
  cashoutBtn: { backgroundColor: '#059669' },
  cashoutNote: {
    fontSize: 12, color: '#6B7280', marginBottom: 16, lineHeight: 18,
    backgroundColor: '#F0FDF4', padding: 10, borderRadius: 10,
  },
  cashoutError: { fontSize: 12, color: '#DC2626', marginBottom: 8, marginTop: -8 },
  depositNote: {
    fontSize: 13, color: '#666', textAlign: 'center', marginBottom: 4, paddingHorizontal: 10,
  },
  qrContainer: {
    alignItems: 'center', backgroundColor: '#F9F9F9', padding: 20,
    borderRadius: 12, marginVertical: 16, width: '100%'
  },
  bankDetailsText: { fontSize: 12, color: '#555', marginTop: 6, fontWeight: '600' },
});
