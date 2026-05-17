// ProfileScreen.js
import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  Image, Alert, SafeAreaView, Switch,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useAuthStore } from '../store/useAuthStore';
import { useChatStore } from '../store/useChatStore';
import { coinService, subscriptionService } from '../services/api';

export const ProfileScreen = ({ navigation }) => {
  const { user, logout } = useAuthStore();
  const { coinBalance, isPremium, loadWallet } = useChatStore();
  const [subInfo, setSubInfo] = useState(null);

  useEffect(() => {
    loadWallet();
    subscriptionService.getStatus().then(r => setSubInfo(r.data.data)).catch(() => {});
  }, []);

  const handleLogout = () => {
    Alert.alert('Logout', 'Are you sure?', [
      { text: 'Logout', style: 'destructive', onPress: () => logout() },
      { text: 'Cancel', style: 'cancel' },
    ]);
  };

  const age = user?.dateOfBirth
    ? Math.floor((Date.now() - new Date(user.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000))
    : '';

  const menuItems = [
    {
      icon: 'call-outline',
      label: 'Call Tester',
      sub: 'Dev tool — test voice & video calls',
      onPress: () => navigation.navigate('DevCallTest'),
      badge: '🧪',
    },
    { icon: 'star-outline', label: 'SuperChat', sub: 'Send highlighted messages', onPress: () => navigation.navigate('SuperChat') },
    { icon: 'diamond-outline', label: 'Premium', sub: isPremium ? `${subInfo?.daysRemaining || 0} days remaining` : 'Unlock all features', onPress: () => navigation.navigate('Subscription'), badge: isPremium ? '✅' : '👑' },
    { icon: 'wallet-outline', label: 'Coins', sub: `${coinBalance || 0} coins`, onPress: () => Alert.alert('Coins', `Balance: ${coinBalance}\nBuy more coming soon!`) },
    { icon: 'notifications-outline', label: 'Notifications', sub: 'View all notifications', onPress: () => navigation.navigate('Notifications') },
    { icon: 'settings-outline', label: 'Settings', sub: 'Privacy, account, help', onPress: () => Alert.alert('Settings', 'Settings coming soon') },
    { icon: 'information-circle-outline', label: 'About Mingley', sub: 'v1.0 — Dating made fun!', onPress: () => {} },
  ];

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Header */}
        <LinearGradient colors={['#E94057', '#8A2387']} style={styles.header}>
          <Image
            source={{ uri: user?.avatar || 'https://randomuser.me/api/portraits/men/1.jpg' }}
            style={styles.avatar}
          />
          <Text style={styles.name}>{user?.fullName || 'User'}{age ? `, ${age}` : ''}</Text>
          <Text style={styles.email}>{user?.email || user?.phone || ''}</Text>
          {isPremium && (
            <View style={styles.premiumBadge}>
              <Ionicons name="diamond" size={14} color="#F5A623" />
              <Text style={styles.premiumText}>
                {subInfo?.subscription?.plan?.name || 'Premium'} Member
              </Text>
            </View>
          )}
        </LinearGradient>

        {/* Coin Balance */}
        <TouchableOpacity style={styles.coinCard} onPress={() => navigation.navigate('Subscription')}>
          <Ionicons name="logo-bitcoin" size={22} color="#F5A623" />
          <Text style={styles.coinText}>{coinBalance || 0} Coins</Text>
          <Ionicons name="add-circle-outline" size={22} color="#E94057" />
        </TouchableOpacity>

        {/* Menu */}
        <View style={styles.menu}>
          {menuItems.map(item => (
            <TouchableOpacity key={item.label} style={styles.menuItem} onPress={item.onPress}>
              <View style={styles.menuIcon}>
                <Ionicons name={item.icon} size={22} color="#E94057" />
              </View>
              <View style={styles.menuInfo}>
                <Text style={styles.menuLabel}>{item.label}</Text>
                <Text style={styles.menuSub}>{item.sub}</Text>
              </View>
              {item.badge ? (
                <Text style={styles.menuBadge}>{item.badge}</Text>
              ) : (
                <Ionicons name="chevron-forward" size={18} color="#DDD" />
              )}
            </TouchableOpacity>
          ))}
        </View>

        {/* Logout */}
        <TouchableOpacity style={styles.logoutBtn} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={20} color="#E94057" />
          <Text style={styles.logoutText}>Logout</Text>
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F5F5' },
  header: { alignItems: 'center', paddingTop: 30, paddingBottom: 30, gap: 8 },
  avatar: { width: 90, height: 90, borderRadius: 45, borderWidth: 3, borderColor: 'rgba(255,255,255,0.5)' },
  name: { color: '#FFF', fontSize: 22, fontWeight: '700' },
  email: { color: 'rgba(255,255,255,0.8)', fontSize: 13 },
  premiumBadge: {
    flexDirection: 'row', alignItems: 'center', gap: 5,
    backgroundColor: 'rgba(255,255,255,0.2)',
    paddingHorizontal: 14, paddingVertical: 5, borderRadius: 20, marginTop: 4,
  },
  premiumText: { color: '#F5A623', fontWeight: '700', fontSize: 13 },
  coinCard: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    backgroundColor: '#FFF', margin: 16, borderRadius: 16,
    paddingHorizontal: 20, paddingVertical: 16,
    elevation: 2, shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.08,
  },
  coinText: { fontSize: 18, fontWeight: '700', color: '#000' },
  menu: { backgroundColor: '#FFF', marginHorizontal: 16, borderRadius: 16, overflow: 'hidden' },
  menuItem: {
    flexDirection: 'row', alignItems: 'center', paddingHorizontal: 18, paddingVertical: 16,
    borderBottomWidth: 1, borderBottomColor: '#F5F5F5',
  },
  menuIcon: {
    width: 40, height: 40, borderRadius: 12, backgroundColor: '#FFF0F2',
    justifyContent: 'center', alignItems: 'center', marginRight: 14,
  },
  menuInfo: { flex: 1 },
  menuLabel: { fontSize: 15, fontWeight: '600', color: '#000' },
  menuSub: { fontSize: 12, color: '#888', marginTop: 2 },
  menuBadge: { fontSize: 16 },
  logoutBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    margin: 16, padding: 16, backgroundColor: '#FFF', borderRadius: 16, gap: 10,
    borderWidth: 1.5, borderColor: '#FFD0D8',
  },
  logoutText: { color: '#E94057', fontWeight: '700', fontSize: 16 },
});