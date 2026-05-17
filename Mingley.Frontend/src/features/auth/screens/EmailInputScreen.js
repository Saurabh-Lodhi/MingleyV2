import React, { useState } from 'react';
import {
  View, Text, StyleSheet, KeyboardAvoidingView, Platform,
  TouchableOpacity, Alert, ScrollView,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING } from '../../../constants/theme';
import { CustomInput } from '../../../components/common/CustomInput';
import { Button } from '../../../components/common/Button';
import { useAuthStore } from '../../../store/useAuthStore';

const phoneRegex = /^[+]?[0-9]{10,13}$/;

const schema = yup.object().shape({
  fullName: yup.string().min(2, 'Name too short').required('Full name is required'),
  email: yup.string().email('Invalid email format').required('Email is required'),
  phone: yup
    .string()
    .matches(phoneRegex, 'Enter a valid phone number (10-13 digits)')
    .required('Phone number is required'),
  password: yup.string().min(8, 'Min 8 characters').required('Password is required'),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Passwords must match')
    .required('Confirm your password'),
});

export const EmailInputScreen = ({ navigation }) => {
  const { control, handleSubmit, formState: { errors } } = useForm({ resolver: yupResolver(schema) });
  const register = useAuthStore(s => s.register);
  const isLoading = useAuthStore(s => s.isLoading);
  const [showPwd, setShowPwd] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const onSubmit = async (data) => {
    const result = await register({
      email: data.email.trim().toLowerCase(),
      phone: data.phone.trim(),
      password: data.password,
      confirmPassword: data.confirmPassword,
      fullName: data.fullName.trim(),
      gender: null,
      dateOfBirth: null,
    });
    if (result.success) {
      navigation.navigate('OTPVerification', {
        type: 'email',
        value: data.email,
        userId: result.userId,
        devOtp: result.devOtp,
        purpose: 'registration',
      });
    } else {
      Alert.alert('Registration Failed', result.error || 'Please try again.');
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={styles.flex}>
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <TouchableOpacity style={styles.back} onPress={() => navigation.goBack()}>
            <Icon name="chevron-back" size={24} color="#000" />
          </TouchableOpacity>
          <View style={styles.content}>
            <Text style={styles.title}>Create Account</Text>
            <Text style={styles.subtitle}>Fill in your details to get started</Text>

            <CustomInput
              control={control}
              name="fullName"
              placeholder="Full name"
              autoCapitalize="words"
              error={errors.fullName?.message}
            />
            <CustomInput
              control={control}
              name="email"
              placeholder="Email address"
              keyboardType="email-address"
              autoCapitalize="none"
              error={errors.email?.message}
            />
            <CustomInput
              control={control}
              name="phone"
              placeholder="Phone number (e.g. 9876543210)"
              keyboardType="phone-pad"
              error={errors.phone?.message}
            />

            <View style={styles.passwordWrap}>
              <CustomInput
                control={control}
                name="password"
                placeholder="Password (min 8 chars)"
                secureTextEntry={!showPwd}
                error={errors.password?.message}
              />
              <TouchableOpacity style={styles.eyeBtn} onPress={() => setShowPwd(v => !v)}>
                <Icon name={showPwd ? 'eye-off-outline' : 'eye-outline'} size={22} color="#888" />
              </TouchableOpacity>
            </View>

            <View style={styles.passwordWrap}>
              <CustomInput
                control={control}
                name="confirmPassword"
                placeholder="Confirm password"
                secureTextEntry={!showConfirm}
                error={errors.confirmPassword?.message}
              />
              <TouchableOpacity style={styles.eyeBtn} onPress={() => setShowConfirm(v => !v)}>
                <Icon name={showConfirm ? 'eye-off-outline' : 'eye-outline'} size={22} color="#888" />
              </TouchableOpacity>
            </View>

            <Button
              title={isLoading ? 'Creating account...' : 'Continue'}
              onPress={handleSubmit(onSubmit)}
              style={styles.btn}
              textStyle={styles.btnText}
              variant="solid"
              disabled={isLoading}
            />
          </View>
          <TouchableOpacity onPress={() => navigation.navigate('Login')} style={styles.login}>
            <Text style={styles.loginText}>
              Already have an account? <Text style={styles.loginBold}>Login</Text>
            </Text>
          </TouchableOpacity>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  flex: { flex: 1 },
  scroll: { flexGrow: 1, paddingBottom: 40 },
  back: { paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  content: { paddingHorizontal: SPACING.xl, paddingTop: 20 },
  title: { fontSize: 32, fontWeight: '700', color: '#000', marginBottom: 8 },
  subtitle: { fontSize: 15, color: '#666', marginBottom: 24, lineHeight: 22 },
  passwordWrap: { position: 'relative', width: '100%' },
  eyeBtn: {
    position: 'absolute', right: 14, top: 0, bottom: 0,
    justifyContent: 'center', zIndex: 10,
  },
  btn: { borderRadius: 16, height: 52, backgroundColor: '#E94057', marginTop: 24 },
  btnText: { color: '#fff', fontSize: 17, fontWeight: '700' },
  login: { alignItems: 'center', paddingBottom: 32, paddingTop: 16 },
  loginText: { fontSize: 14, color: '#666' },
  loginBold: { color: '#E94057', fontWeight: '700' },
});
