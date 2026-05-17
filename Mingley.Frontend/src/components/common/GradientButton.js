import React from 'react';
import { TouchableOpacity, Text, StyleSheet, ActivityIndicator } from 'react-native';
import { LinearGradient } from '../compat/LinearGradient';
import { COLORS, SPACING, TYPOGRAPHY } from '../../constants/theme';

export const GradientButton = ({
  title,
  onPress,
  disabled = false,
  loading = false,
  style,
  textStyle,
  colors = [COLORS.gradientStart, COLORS.gradientEnd], // Allow overriding colors for Submit button vs Continue button
}) => {
  return (
    <TouchableOpacity
      onPress={onPress}
      disabled={disabled || loading}
      style={[styles.container, style]}
      activeOpacity={0.8}
    >
      <LinearGradient
        colors={disabled ? [COLORS.surface, COLORS.surface] : colors}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 0 }}
        style={styles.gradient}
      >
        {loading ? (
          <ActivityIndicator color={COLORS.white} />
        ) : (
          <Text style={[styles.text, textStyle, disabled && styles.textDisabled]}>
            {title}
          </Text>
        )}
      </LinearGradient>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: {
    height: 56,
    borderRadius: 28,
    overflow: 'hidden',
    width: '100%',
    marginVertical: SPACING.s,
  },
  gradient: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  text: {
    ...TYPOGRAPHY.h3,
    color: COLORS.white,
    fontWeight: 'bold',
  },
  textDisabled: {
    color: COLORS.textSecondary,
  },
});
