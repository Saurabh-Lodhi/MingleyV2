import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Animated } from 'react-native';
import { Image as FastImage } from 'react-native';
import { LinearGradient } from '../../../components/compat/LinearGradient';
import { Ionicons as Icon } from '@expo/vector-icons';
import { SPACING, TYPOGRAPHY } from '../../../constants/theme';

export const MatchesGridItem = ({ match, onPress, onAccept, onReject }) => {
  return (
    <TouchableOpacity
      style={styles.container}
      onPress={onPress}
      activeOpacity={0.92}
    >
      <FastImage source={{ uri: match.image }} style={styles.image} />
      <LinearGradient
        colors={['transparent', 'rgba(0,0,0,0.82)']}
        style={styles.gradient}
      />

      {/* Name & age */}
      <Text style={styles.name} numberOfLines={1}>{match.name}, {match.age}</Text>

      {/* Action row */}
      <View style={styles.actionsRow}>
        {/* Reject */}
        <TouchableOpacity
          style={[styles.actionButton, styles.rejectButton]}
          onPress={(e) => { e.stopPropagation?.(); onReject && onReject(match); }}
          activeOpacity={0.8}
          hitSlop={{ top: 6, bottom: 6, left: 6, right: 6 }}
        >
          <Icon name="close" size={18} color="#FF4D67" />
        </TouchableOpacity>

        {/* Accept */}
        <TouchableOpacity
          style={[styles.actionButton, styles.acceptButton]}
          onPress={(e) => { e.stopPropagation?.(); onAccept && onAccept(match); }}
          activeOpacity={0.8}
          hitSlop={{ top: 6, bottom: 6, left: 6, right: 6 }}
        >
          <Icon name="heart" size={18} color="#E94057" />
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    height: 240,
    margin: SPACING.s,
    borderRadius: 20,
    overflow: 'hidden',
    backgroundColor: '#EEE',
    position: 'relative',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.12,
    shadowRadius: 8,
    elevation: 5,
  },
  image: { width: '100%', height: '100%' },
  gradient: {
    position: 'absolute', bottom: 0, left: 0, right: 0, height: '60%',
  },
  name: {
    ...TYPOGRAPHY.h3,
    color: '#FFFFFF',
    position: 'absolute',
    bottom: 54,
    left: SPACING.m,
    right: SPACING.m,
    fontWeight: '700',
    fontSize: 14,
  },
  actionsRow: {
    position: 'absolute',
    bottom: 0, left: 0, right: 0,
    height: 48,
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
    paddingHorizontal: 12,
    backgroundColor: 'rgba(0,0,0,0.35)',
  },
  actionButton: {
    width: 36, height: 36, borderRadius: 18,
    justifyContent: 'center', alignItems: 'center',
  },
  rejectButton: {
    backgroundColor: 'rgba(255,255,255,0.9)',
  },
  acceptButton: {
    backgroundColor: 'rgba(255,255,255,0.9)',
  },
});
