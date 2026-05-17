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
import { COLORS, SPACING, TYPOGRAPHY } from '../../../constants/theme';
import { CustomInput } from '../../../components/common/CustomInput';
import { Button } from '../../../components/common/Button';
import { useAuthStore } from '../../../store/useAuthStore';

const loginSchema = yup.object().shape({
  identifier: yup.string().required('Email or phone required'),
  password: yup.string().min(6, 'Min 6 characters').required('Password required'),
});

export const LoginScreen = ({ navigation }) => {
  const { control, handleSubmit } = useForm({
    resolver: yupResolver(loginSchema),
    defaultValues: { identifier: '', password: '' },
  });
  const login = useAuthStore(state => state.login);
  const isLoading = useAuthStore(state => state.isLoading);
  const [showPassword, setShowPassword] = useState(false);

  const onSubmit = async (data) => {
    const result = await login(data.identifier, data.password);
    if (!result.success) {
      // requiresVerification: user registered but OTP not verified yet
      if (result.requiresVerification) {
        navigation.navigate('OTPVerification', {
          userId: result.userId,
          devOtp: result.devOtp,
          type: 'login',
          purpose: 'registration',
        });
        return;
      }
      Alert.alert('Login Failed', result.error || 'Invalid credentials');
    }
    // On success, AppNavigator automatically switches to Main
  };

  return (
    <SafeAreaView style={styles.container}>
      <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()} activeOpacity={0.7}>
        <Icon name="chevron-back" size={24} color="#000" />
      </TouchableOpacity>

      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'} style={styles.keyboard}>
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <View style={styles.content}>
            <View style={styles.header}>
              <Text style={styles.title}>Login</Text>
              <Text style={styles.subtitle}>Enter your email or phone and password to continue</Text>
            </View>

            <View style={styles.formContainer}>
              <CustomInput
                control={control}
                name="identifier"
                placeholder="Email or phone"
                keyboardType="email-address"
                autoCapitalize="none"
                isGradientBorder={false}
              />

              {/* Password with eye icon */}
              <View style={styles.passwordWrap}>
                <CustomInput
                  control={control}
                  name="password"
                  placeholder="Password"
                  secureTextEntry={!showPassword}
                  isGradientBorder={false}
                  containerStyle={styles.passwordInput}
                />
                <TouchableOpacity style={styles.eyeBtn} onPress={() => setShowPassword(v => !v)}>
                  <Icon name={showPassword ? 'eye-off-outline' : 'eye-outline'} size={22} color="#888" />
                </TouchableOpacity>
              </View>

              <TouchableOpacity
                onPress={() => navigation.navigate('ForgotPassword')}
                style={styles.forgotRow}
              >
                <Text style={styles.forgotText}>Forgot password?</Text>
              </TouchableOpacity>

              <Button
                title={isLoading ? 'Logging in...' : 'Login'}
                onPress={handleSubmit(onSubmit)}
                style={styles.button}
                textStyle={styles.buttonText}
                variant="solid"
                disabled={isLoading}
              />
            </View>

            <TouchableOpacity onPress={() => navigation.navigate('SignupOptions')} style={styles.signupLink}>
              <Text style={styles.signupText}>
                Don't have an account? <Text style={styles.signupBold}>Sign up</Text>
              </Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#FFFFFF' },
  keyboard: { flex: 1 },
  scroll: { flexGrow: 1 },
  backButton: { paddingHorizontal: SPACING.xl, paddingTop: SPACING.m },
  content: { flex: 1, paddingHorizontal: SPACING.xl, paddingTop: 30, alignItems: 'center' },
  header: { alignItems: 'center', marginBottom: 40 },
  title: { fontSize: 40, color: '#000000', fontWeight: '600', marginBottom: 10 },
  subtitle: { fontSize: 16, color: '#5b5b5b', textAlign: 'center', paddingHorizontal: 10, lineHeight: 24 },
  formContainer: { width: '100%', marginBottom: 40 },
  passwordWrap: { position: 'relative', width: '100%' },
  passwordInput: { width: '100%' },
  eyeBtn: {
    position: 'absolute', right: 14, top: 0, bottom: 0,
    justifyContent: 'center', zIndex: 10,
  },
  forgotRow: { alignSelf: 'flex-end', marginTop: 8, marginBottom: 4 },
  forgotText: { color: '#E94057', fontSize: 13, fontWeight: '600' },
  button: {
    borderRadius: 16, height: 52, backgroundColor: '#E94057',
    justifyContent: 'center', alignItems: 'center', marginTop: 20,
  },
  buttonText: { color: '#FFFFFF', fontSize: 18, fontWeight: '700' },
  signupLink: { alignItems: 'center', marginTop: 16 },
  signupText: { fontSize: 14, color: '#5b5b5b' },
  signupBold: { color: '#E94057', fontWeight: '700' },
});
