import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING } from '../../../constants/theme';
import { Chip } from '../components/Chip';
import { Button } from '../../../components/common/Button';
import { useProfileSetupStore } from '../store/useProfileSetupStore';
import { userService, interestService } from '../../../services/api';

export const InterestsSelectionScreen = ({ navigation }) => {
  const { interests, toggleInterest } = useProfileSetupStore();
  const [allInterests, setAllInterests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    interestService.getAll()
      .then(res => setAllInterests(res.data.data?.interests || []))
      .catch(() => {
        // Fallback hardcoded if API fails
        setAllInterests([
          { id: '1', name: 'Music',       icon: 'musical-notes-outline' },
          { id: '2', name: 'Travel',      icon: 'airplane-outline' },
          { id: '3', name: 'Gym',         icon: 'barbell-outline' },
          { id: '4', name: 'Movies',      icon: 'film-outline' },
          { id: '5', name: 'Cooking',     icon: 'restaurant-outline' },
          { id: '6', name: 'Art',         icon: 'color-palette-outline' },
          { id: '7', name: 'Dancing',     icon: 'body-outline' },
          { id: '8', name: 'Photography', icon: 'camera-outline' },
          { id: '9', name: 'Yoga',        icon: 'body-outline' },
          { id: '10', name: 'Reading',    icon: 'book-outline' },
          { id: '11', name: 'Shopping',   icon: 'bag-handle-outline' },
          { id: '12', name: 'Video games',icon: 'game-controller-outline' },
        ]);
      })
      .finally(() => setLoading(false));
  }, []);

  const handleContinue = async () => {
    setSaving(true);
    try {
      if (interests.length > 0) await userService.updateInterests(interests);
    } catch (e) { console.log('Interests error:', e?.response?.data); }
    finally { setSaving(false); navigation.navigate('ContactsPermission'); }
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
        <Text style={styles.title}>Your interests</Text>
        <Text style={styles.subtitle}>Select what you're passionate about. We'll show you better matches.</Text>
        {loading
          ? <ActivityIndicator size="large" color="#E94057" style={{ marginTop: 40 }} />
          : (
            <ScrollView contentContainerStyle={styles.chips} showsVerticalScrollIndicator={false}>
              {allInterests.map(item => (
                <Chip key={item.id} label={item.name} icon={item.icon}
                  selected={interests.includes(item.name)}
                  onPress={() => toggleInterest(item.name)} />
              ))}
            </ScrollView>
          )
        }
        <Button title={saving ? 'Saving...' : `Continue (${interests.length} selected)`}
          onPress={handleContinue} style={styles.btn} textStyle={styles.btnText}
          variant="solid" disabled={saving} />
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  back: { width: 48, height: 48, borderRadius: 12, borderWidth: 1, borderColor: '#F0F0F0', justifyContent: 'center', alignItems: 'center' },
  skip: { fontSize: 16, fontWeight: 'bold', color: '#E94057' },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 30 },
  title: { fontSize: 32, fontWeight: '700', color: '#000', marginBottom: 8 },
  subtitle: { fontSize: 14, color: '#666', marginBottom: 24, lineHeight: 20 },
  chips: { flexDirection: 'row', flexWrap: 'wrap', paddingBottom: 20 },
  btn: { marginBottom: 24, borderRadius: 16, height: 52, backgroundColor: '#E94057' },
  btnText: { color: '#fff', fontSize: 17, fontWeight: '700' },
});
