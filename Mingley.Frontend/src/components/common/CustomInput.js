import React from 'react';
import { View, TextInput, Text, StyleSheet, TouchableOpacity, Platform, Image } from 'react-native';
import { Controller } from 'react-hook-form';
import { LinearGradient } from '../compat/LinearGradient';
import { COLORS, SPACING, TYPOGRAPHY } from '../../constants/theme';

export const CustomInput = ({
  control, name, rules, placeholder,
  keyboardType = 'default', showCountryCode = false,
  isGradientBorder = false, autoCapitalize, secureTextEntry, error: externalError,
}) => {
  return (
    <Controller
      control={control} name={name} rules={rules}
      render={({ field: { onChange, onBlur, value }, fieldState: { error } }) => (
        <View style={styles.wrapper}>
          {isGradientBorder && !error ? (
            <LinearGradient colors={['#E94057', '#8A2387']} start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }} style={styles.gradientBorder}>
              <View style={styles.innerContainer}>
                <TextInput style={styles.input} onBlur={onBlur} onChangeText={onChange} value={value}
                  placeholder={placeholder} placeholderTextColor="#A0A0A0" keyboardType={keyboardType}
                  autoCapitalize={autoCapitalize} secureTextEntry={secureTextEntry} />
              </View>
            </LinearGradient>
          ) : (
            <View style={[styles.container, error && styles.errorContainer]}>
              {showCountryCode && (
                <>
                  <TouchableOpacity style={styles.countryCodeContainer} activeOpacity={0.7}>
                    <Text style={styles.countryCode}>(+91)</Text>
                    <Text style={styles.dropdownArrow}>▼</Text>
                  </TouchableOpacity>
                  <View style={styles.divider} />
                </>
              )}
              <TextInput style={styles.input} onBlur={onBlur} onChangeText={onChange} value={value}
                placeholder={placeholder} placeholderTextColor="#A0A0A0" keyboardType={keyboardType}
                autoCapitalize={autoCapitalize} secureTextEntry={secureTextEntry} maxLength={200} />
            </View>
          )}
          {(error || externalError) && <Text style={styles.errorText}>{error?.message || externalError}</Text>}
        </View>
      )}
    />
  );
};

const FONT = Platform.OS === 'ios' ? 'System' : 'sans-serif';
const base = { flexDirection: 'row', alignItems: 'center', height: 56, borderRadius: 28, paddingHorizontal: SPACING.m, backgroundColor: '#FFFFFF' };

const styles = StyleSheet.create({
  wrapper: { marginVertical: SPACING.s, width: '100%' },
  gradientBorder: { padding: 1.5, borderRadius: 29.5, width: '100%' },
  innerContainer: { ...base },
  container: { ...base, borderWidth: 1, borderColor: '#E8E8E8' },
  errorContainer: { borderColor: COLORS.error },
  countryCodeContainer: { flexDirection: 'row', alignItems: 'center' },
  countryCode: { fontSize: 15, color: '#333333', fontWeight: '500' },
  dropdownArrow: { fontSize: 8, color: '#AAAAAA', marginLeft: 5 },
  divider: { height: 24, width: 1, backgroundColor: '#E8E8E8', marginHorizontal: SPACING.s },
  input: { flex: 1, fontSize: 16, color: '#333333', height: '100%' },
  errorText: { ...TYPOGRAPHY.caption, color: COLORS.error, marginTop: SPACING.xs, marginLeft: SPACING.m },
});
