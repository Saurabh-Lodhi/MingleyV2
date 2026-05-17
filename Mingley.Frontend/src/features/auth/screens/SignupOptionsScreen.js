import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Image } from 'react-native';
import { Ionicons as Icon } from '@expo/vector-icons';
import { COLORS, SPACING, TYPOGRAPHY } from '../../../constants/theme';

export const SignupOptionsScreen = ({ navigation }) => {
  return (
    <SafeAreaView style={styles.container}>
      <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
        <Icon name="chevron-back" size={24} color="#000" />
      </TouchableOpacity>
      <View style={styles.content}>
        <View style={styles.logoContainer}>
          <Image source={{ uri: 'https://img.icons8.com/fluency/96/like.png' }} style={styles.logo} resizeMode="contain" />
        </View>
        <Text style={styles.title}>Sign up to continue</Text>
        <View style={styles.buttonsContainer}>
          <TouchableOpacity style={styles.primaryButton} onPress={() => navigation.navigate('EmailInput')} activeOpacity={0.8}>
            <Text style={styles.primaryButtonText}>Continue with email</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.secondaryButton} onPress={() => navigation.navigate('EmailInput')} activeOpacity={0.8}>
            <Text style={styles.secondaryButtonText}>Use phone number</Text>
          </TouchableOpacity>
        </View>
        <View style={styles.orContainer}>
          <View style={styles.divider} />
          <Text style={styles.orText}>or sign up with</Text>
          <View style={styles.divider} />
        </View>
        <View style={styles.socialContainer}>
          <TouchableOpacity style={styles.socialButton}><Icon name="logo-google" size={24} color="#000" /></TouchableOpacity>
          <TouchableOpacity style={styles.socialButton}><Icon name="logo-apple" size={24} color="#000" /></TouchableOpacity>
          <TouchableOpacity style={styles.socialButton}><Icon name="logo-instagram" size={24} color="#000" /></TouchableOpacity>
        </View>
      </View>
      <View style={styles.footer}>
        <TouchableOpacity><Text style={styles.footerLink}>Terms of use</Text></TouchableOpacity>
        <TouchableOpacity><Text style={styles.footerLink}>Privacy Policy</Text></TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFFFFF' },
  backButton: { paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 30, alignItems: 'center' },
  logoContainer: { marginBottom: 40, justifyContent: 'center', alignItems: 'center' },
  logo: { width: 100, height: 100 },
  title: { fontSize: 24, color: '#000000', fontWeight: 'bold', fontFamily: 'sans-serif-medium', marginBottom: 40 },
  buttonsContainer: { width: '100%', marginBottom: 40 },
  primaryButton: { backgroundColor: '#E94057', height: 56, borderRadius: 16, justifyContent: 'center', alignItems: 'center', marginBottom: 16 },
  primaryButtonText: { fontSize: 18, fontWeight: '600', color: '#FFFFFF' },
  secondaryButton: { backgroundColor: '#FFFFFF', borderWidth: 1, borderColor: '#F0F0F0', height: 56, borderRadius: 16, justifyContent: 'center', alignItems: 'center' },
  secondaryButtonText: { fontSize: 18, fontWeight: '600', color: '#E94057' },
  orContainer: { flexDirection: 'row', alignItems: 'center', marginBottom: 30, width: '100%' },
  divider: { flex: 1, height: 1, backgroundColor: '#F0F0F0' },
  orText: { marginHorizontal: 15, color: '#000000', fontSize: 14 },
  socialContainer: { flexDirection: 'row', gap: 20 },
  socialButton: { width: 65, height: 55, borderRadius: 12, borderWidth: 1, borderColor: '#E8E8E8', justifyContent: 'center', alignItems: 'center', backgroundColor: '#FFFFFF' },
  footer: { flexDirection: 'row', justifyContent: 'center', gap: 30, paddingBottom: 40 },
  footerLink: { fontSize: 14, color: '#E94057' },
});
