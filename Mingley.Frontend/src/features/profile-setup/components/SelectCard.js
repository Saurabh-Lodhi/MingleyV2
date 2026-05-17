import React from 'react';
import { TouchableOpacity, Text, StyleSheet } from 'react-native';
import { Ionicons as Icon } from '@expo/vector-icons';
import { TYPOGRAPHY, SPACING } from '../../../constants/theme';

export const SelectCard = ({ label, selected, onPress }) => {
  return (
    <TouchableOpacity 
      style={[styles.container, selected && styles.containerSelected]}
      onPress={onPress}
      activeOpacity={0.8}
    >
      <Text style={[styles.label, selected && styles.labelSelected]}>{label}</Text>
      
      {selected ? (
        <Icon name="checkmark" size={24} color="#FFFFFF" />
      ) : (
         <Icon name="chevron-forward" size={20} color="#A0A0A0" style={styles.iconUnselected} />
      )}
    </TouchableOpacity>
  );
};

// Re-using common Select Card but making it responsive to 'Choose Another' which has no checkmark logic naturally
const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: '#E8E8E8',
    borderRadius: 16,
    height: 64,
    paddingHorizontal: SPACING.xl,
    marginBottom: SPACING.m,
    backgroundColor: '#FFFFFF',
  },
  containerSelected: {
    backgroundColor: '#E94057',
    borderColor: '#E94057',
  },
  label: {
    ...TYPOGRAPHY.body,
    fontWeight: '600',
    color: '#000000',
  },
  labelSelected: {
    color: '#FFFFFF',
  },
  iconUnselected: {
    opacity: 0.5,
  },
});
