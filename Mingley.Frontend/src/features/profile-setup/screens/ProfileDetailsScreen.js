import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, KeyboardAvoidingView, Platform, Keyboard, TouchableWithoutFeedback, Alert, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Image } from 'react-native';
import { Ionicons as Icon } from '@expo/vector-icons';
import { COLORS, SPACING, TYPOGRAPHY } from '../../../constants/theme';
import { CardInput } from '../components/CardInput';
import { BottomSheetDatePicker } from '../components/BottomSheetDatePicker';
import { Button } from '../../../components/common/Button';
import { useProfileSetupStore } from '../store/useProfileSetupStore';
import { userService } from '../../../services/api';
import { useAuthStore } from '../../../store/useAuthStore';

export const ProfileDetailsScreen = ({ navigation }) => {
  const { profileDetails, setProfileDetails } = useProfileSetupStore();
  const updateUser = useAuthStore(s => s.updateUser);
  const [isDatePickerVisible, setDatePickerVisible] = useState(false);
  const [saving, setSaving] = useState(false);

  const handleConfirm = async () => {
    setSaving(true);
    try {
      const fullName = `${profileDetails.firstName || ''} ${profileDetails.lastName || ''}`.trim();
      await userService.updateProfile({
        fullName: fullName || undefined,
        dateOfBirth: profileDetails.birthday
          ? new Date(profileDetails.birthday).toISOString()
          : undefined,
      });
      updateUser({ fullName });
    } catch (e) {
      console.log('Profile update error:', e?.response?.data);
    } finally {
      setSaving(false);
      navigation.navigate('GenderSelection');
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
        <View style={{ flex: 1 }}>
          <View style={styles.header}>
            <TouchableOpacity onPress={() => navigation.navigate('Home')}>
              <Text style={styles.skipText}>Skip</Text>
            </TouchableOpacity>
          </View>
          <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={styles.content}>
            <Text style={styles.title}>Profile details</Text>
            <View style={styles.avatarContainer}>
              <View style={styles.avatarWrapper}>
                <Image source={{ uri: 'https://randomuser.me/api/portraits/lego/1.jpg' }} style={styles.avatar} resizeMode="cover" />
                <TouchableOpacity style={styles.cameraIcon}>
                  <Icon name="camera" size={16} color="#FFFFFF" />
                </TouchableOpacity>
              </View>
            </View>
            <View style={styles.formContainer}>
              <CardInput label="First name" value={profileDetails.firstName} onChangeText={(t) => setProfileDetails({ firstName: t })} placeholder="Peter" />
              <CardInput label="Last name" value={profileDetails.lastName} onChangeText={(t) => setProfileDetails({ lastName: t })} placeholder="Parker" />
              <TouchableOpacity style={styles.birthdayButton} onPress={() => setDatePickerVisible(true)} activeOpacity={0.8}>
                <Icon name="calendar-outline" size={24} color="#E94057" style={styles.calendarIcon} />
                <Text style={[styles.birthdayText, !profileDetails.birthday && styles.birthdayPlaceholder]}>
                  {profileDetails.birthday || 'Choose birthday date'}
                </Text>
              </TouchableOpacity>
            </View>
            <Button title={saving ? 'Saving...' : 'Confirm'} onPress={handleConfirm} style={styles.confirmButton} textStyle={styles.buttonText} variant="solid" disabled={saving} />
          </KeyboardAvoidingView>
        </View>
      </TouchableWithoutFeedback>
      <BottomSheetDatePicker visible={isDatePickerVisible} onClose={() => setDatePickerVisible(false)} selectedDate={profileDetails.birthday} onSelectDate={(d) => setProfileDetails({ birthday: d })} />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFFFFF' },
  header: { alignItems: 'flex-end', paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  skipText: { fontSize: 16, fontWeight: 'bold', color: '#E94057' },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 30 },
  title: { fontSize: 34, fontWeight: 'bold', color: '#000000', marginBottom: 40 },
  avatarContainer: { alignItems: 'center', marginBottom: 40 },
  avatarWrapper: { width: 130, height: 140, borderRadius: 40, backgroundColor: '#F0F0F0', position: 'relative' },
  avatar: { width: '100%', height: '100%', borderRadius: 40 },
  cameraIcon: { position: 'absolute', bottom: -5, right: -5, width: 36, height: 36, borderRadius: 18, backgroundColor: '#E94057', justifyContent: 'center', alignItems: 'center', borderWidth: 3, borderColor: '#FFFFFF' },
  formContainer: { gap: SPACING.m, marginBottom: 40 },
  birthdayButton: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFF0F3', height: 60, borderRadius: 16, paddingHorizontal: SPACING.m, marginTop: 10 },
  calendarIcon: { marginRight: 12 },
  birthdayText: { fontSize: 16, fontWeight: '600', color: '#E94057' },
  birthdayPlaceholder: { color: '#E94057', opacity: 0.6 },
  confirmButton: { marginTop: 'auto', marginBottom: 60, borderRadius: 16, height: 52, backgroundColor: '#E94057' },
  buttonText: { color: '#FFFFFF', fontSize: 18, fontWeight: '600' },
});
