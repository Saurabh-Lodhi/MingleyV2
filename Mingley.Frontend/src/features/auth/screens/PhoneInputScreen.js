// PhoneInputScreen.js — Register with phone + email + password
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

// Reuse the same full registration form as EmailInputScreen
// This screen is reached via "Continue with Phone" on SignupOptions
export { EmailInputScreen as PhoneInputScreen } from './EmailInputScreen';
