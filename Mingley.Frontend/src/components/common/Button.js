import React from 'react';
import { TouchableOpacity, Text, StyleSheet, ActivityIndicator } from 'react-native';
import { LinearGradient } from '../compat/LinearGradient';
import { COLORS, SPACING, TYPOGRAPHY } from '../../constants/theme';

export const Button = ({
  title, onPress, variant = 'primary', disabled = false,
  loading = false, style, textStyle,
}) => {
  const isPrimary = variant === 'primary';

  const Content = () => (
    <>
      {loading
        ? <ActivityIndicator color={isPrimary ? COLORS.white : COLORS.primary} />
        : <Text style={[styles.text, !isPrimary && styles.textOutline, disabled && styles.textDisabled, textStyle]}>{title}</Text>
      }
    </>
  );

  if (isPrimary && !disabled) {
    return (
      <TouchableOpacity onPress={onPress} disabled={disabled || loading} style={[styles.container, style]} activeOpacity={0.8}>
        <LinearGradient colors={[COLORS.gradientStart, COLORS.gradientEnd]} start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }} style={styles.gradient}>
          <Content />
        </LinearGradient>
      </TouchableOpacity>
    );
  }

  return (
    <TouchableOpacity onPress={onPress} disabled={disabled || loading}
      style={[styles.container, styles.solidBackground, !isPrimary && styles.outlineContainer, disabled && styles.disabledContainer, style]}
      activeOpacity={0.8}
    >
      <Content />
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: { height: 56, borderRadius: 28, overflow: 'hidden', width: '100%', marginVertical: SPACING.s },
  gradient: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  solidBackground: { justifyContent: 'center', alignItems: 'center', backgroundColor: COLORS.primary },
  outlineContainer: { backgroundColor: 'transparent', borderWidth: 2, borderColor: COLORS.primary },
  disabledContainer: { backgroundColor: COLORS.surface, borderColor: COLORS.border },
  text: { ...TYPOGRAPHY.h3, color: COLORS.white },
  textOutline: { color: COLORS.primary },
  textDisabled: { color: COLORS.textSecondary },
});
