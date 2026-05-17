import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING } from '../../../constants/theme';
import { SelectCard } from '../components/SelectCard';
import { Button } from '../../../components/common/Button';
import { useProfileSetupStore } from '../store/useProfileSetupStore';
import { userService } from '../../../services/api';
import { useAuthStore } from '../../../store/useAuthStore';

export const GenderSelectionScreen = ({ navigation }) => {
  const { gender, setGender } = useProfileSetupStore();
  const updateUser = useAuthStore(s => s.updateUser);
  const [saving, setSaving] = useState(false);

  const handleContinue = async () => {
    if (!gender) return;
    setSaving(true);
    try {
      const genderValue = gender === 'Woman' ? 'female' : gender === 'Man' ? 'male' : 'other';
      
      // Save gender to profile
      await userService.updateProfile({ gender: genderValue });
      updateUser({ gender: genderValue });

      // Set default "interestedIn" based on gender (male → girls, female → boys)
      const interestedIn = genderValue === 'male' ? 'girls' : 'boys';
      await userService.updatePreferences({ interestedIn, minAge: 18, maxAge: 40, maxDistance: 100 });

    } catch (e) { console.log('Gender save error:', e?.response?.data); }
    finally { setSaving(false); navigation.navigate('InterestsSelection'); }
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.back} onPress={() => navigation.goBack()}>
          <Icon name="chevron-back" size={24} color="#E94057" />
        </TouchableOpacity>
        <TouchableOpacity onPress={() => navigation.navigate('Home')}>
          <Text style={styles.skip}>Skip</Text>
        </TouchableOpacity>
      </View>
      <View style={styles.content}>
        <Text style={styles.title}>I am a</Text>
        <Text style={styles.subtitle}>This helps us show you the right profiles.</Text>
        <View style={styles.options}>
          <SelectCard label="Woman" selected={gender === 'Woman'} onPress={() => setGender('Woman')} />
          <SelectCard label="Man"   selected={gender === 'Man'}   onPress={() => setGender('Man')} />
        </View>
        <Button title={saving ? 'Saving...' : 'Continue'}
          onPress={handleContinue} style={styles.btn} textStyle={styles.btnText}
          disabled={!gender || saving} variant="solid" />
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  back: { width: 48, height: 48, borderRadius: 12, borderWidth: 1, borderColor: '#F0F0F0', justifyContent: 'center', alignItems: 'center' },
  skip: { fontSize: 16, fontWeight: 'bold', color: '#E94057' },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 50 },
  title: { fontSize: 34, fontWeight: '700', color: '#000', marginBottom: 8 },
  subtitle: { fontSize: 14, color: '#666', marginBottom: 40 },
  options: { gap: 14, marginBottom: 40 },
  btn: { marginTop: 'auto', marginBottom: 40, borderRadius: 16, height: 52, backgroundColor: '#E94057' },
  btnText: { color: '#fff', fontSize: 17, fontWeight: '700' },
});
