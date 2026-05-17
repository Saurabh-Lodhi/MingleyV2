import React from 'react';
import {
  View, Text, StyleSheet,
  TouchableOpacity, Dimensions, Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Image as FastImage } from 'react-native';
import { LinearGradient } from '../../../components/compat/LinearGradient';
import { Ionicons as Icon } from '@expo/vector-icons';

const { width, height } = Dimensions.get('window');

const PERKS = [
  { icon: 'heart-outline', label: 'Unlimited Likes' },
  { icon: 'eye-off-outline', label: 'Browse Invisibly' },
  { icon: 'star-outline', label: 'Top Picks Daily' },
  { icon: 'options-outline', label: 'Advanced Filters' },
];

export const SubscriptionIntroScreen = ({ navigation }) => {
  return (
    <SafeAreaView style={styles.container}>
      <LinearGradient
        colors={['#1a0a2e', '#2d1456', '#E94057']}
        start={{ x: 0.2, y: 0 }} end={{ x: 0.8, y: 1 }}
        style={StyleSheet.absoluteFillObject}
      />

      {/* Back button */}
      <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
        <Icon name="chevron-back" size={22} color="#fff" />
      </TouchableOpacity>

      {/* Hero illustration */}
      <View style={styles.heroWrap}>
        <FastImage
          source={{ uri: 'https://img.icons8.com/fluency/96/sparkling.png' }}
          style={styles.heroImg}
          resizeMode="contain"
        />
      </View>

      {/* Bottom card */}
      <View style={styles.card}>
        <Text style={styles.eyebrow}>MINGLEY PREMIUM</Text>
        <Text style={styles.title}>Say Hello to{'\n'}Premium!</Text>
        <Text style={styles.subtitle}>
          Get full access to all features and find your perfect match without limits.
        </Text>

        {/* Perks grid */}
        <View style={styles.perksGrid}>
          {PERKS.map((p) => (
            <View key={p.label} style={styles.perkItem}>
              <View style={styles.perkIcon}>
                <Icon name={p.icon} size={18} color="#E94057" />
              </View>
              <Text style={styles.perkLabel}>{p.label}</Text>
            </View>
          ))}
        </View>

        {/* CTA */}
        <TouchableOpacity
          style={styles.ctaWrap}
          onPress={() => navigation.navigate('SubscriptionPlans')}
          activeOpacity={0.85}
        >
          <LinearGradient
            colors={['#E94057', '#8A2387']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.ctaBtn}
          >
            <Text style={styles.ctaText}>View Plans</Text>
            <Icon name="arrow-forward" size={18} color="#fff" />
          </LinearGradient>
        </TouchableOpacity>

        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.skipBtn}>
          <Text style={styles.skipText}>Maybe later</Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

const FONT = Platform.OS === 'ios' ? 'Avenir Next' : 'sans-serif';
const FONT_MED = Platform.OS === 'ios' ? 'AvenirNext-Medium' : 'sans-serif-medium';

const styles = StyleSheet.create({
  container: { flex: 1 },
  backBtn: {
    position: 'absolute', top: 56, left: 20, zIndex: 10,
    width: 42, height: 42, borderRadius: 14,
    backgroundColor: 'rgba(255,255,255,0.15)',
    justifyContent: 'center', alignItems: 'center',
  },
  heroWrap: {
    flex: 1, justifyContent: 'center', alignItems: 'center',
    paddingTop: 60,
  },
  heroImg: { width: width * 0.7, height: width * 0.7 },
  card: {
    backgroundColor: '#fff',
    borderTopLeftRadius: 32, borderTopRightRadius: 32,
    paddingHorizontal: 24, paddingTop: 28, paddingBottom: 36,
  },
  eyebrow: {
    fontSize: 11, fontWeight: '800', color: '#E94057',
    letterSpacing: 1.5, textAlign: 'center', marginBottom: 8,
  },
  title: {
    fontSize: 28, fontWeight: '800', color: '#1a0a2e',
    fontFamily: FONT_MED, textAlign: 'center', lineHeight: 36,
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 14, color: '#666', textAlign: 'center',
    lineHeight: 21, fontFamily: FONT, marginBottom: 20,
  },
  perksGrid: {
    flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginBottom: 24,
  },
  perkItem: {
    width: (width - 48 - 12) / 2,
    flexDirection: 'row', alignItems: 'center', gap: 8,
    backgroundColor: '#FFF0F3', borderRadius: 14,
    paddingHorizontal: 12, paddingVertical: 10,
  },
  perkIcon: {
    width: 32, height: 32, borderRadius: 10,
    backgroundColor: '#fff',
    justifyContent: 'center', alignItems: 'center',
    shadowColor: '#E94057', shadowOpacity: 0.15, shadowRadius: 4, elevation: 2,
  },
  perkLabel: { fontSize: 12, fontWeight: '700', color: '#2d1456', fontFamily: FONT_MED },
  ctaWrap: { borderRadius: 20, overflow: 'hidden', marginBottom: 12 },
  ctaBtn: {
    height: 56, flexDirection: 'row',
    justifyContent: 'center', alignItems: 'center', gap: 10,
  },
  ctaText: { fontSize: 16, fontWeight: '800', color: '#fff', fontFamily: FONT_MED },
  skipBtn: { alignItems: 'center', paddingVertical: 6 },
  skipText: { fontSize: 13, color: '#AAA', fontFamily: FONT },
});
