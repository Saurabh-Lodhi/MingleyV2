import React, { useEffect, useRef } from 'react';
import { View, StyleSheet, Dimensions, Platform, Animated, PanResponder } from 'react-native';

const { width, height: SCREEN_HEIGHT } = Dimensions.get('window');

export const BottomSheetContainer = ({ children, containerStyle, height = 505, onClose }) => {
  const panY = useRef(new Animated.Value(SCREEN_HEIGHT)).current;

  useEffect(() => {
    Animated.spring(panY, { toValue: 0, useNativeDriver: true, damping: 20, stiffness: 120 }).start();
  }, []);

  const handleDismiss = () => {
    Animated.timing(panY, { toValue: SCREEN_HEIGHT, duration: 300, useNativeDriver: true }).start(() => {
      if (onClose) onClose();
    });
  };

  const panResponder = useRef(PanResponder.create({
    onStartShouldSetPanResponder: () => true,
    onMoveShouldSetPanResponder: (_, gs) => gs.dy > 10,
    onPanResponderMove: (_, gs) => { if (gs.dy > 0) panY.setValue(gs.dy); },
    onPanResponderRelease: (_, gs) => {
      if (gs.dy > 100 || gs.vy > 1) {
        handleDismiss();
      } else {
        Animated.spring(panY, { toValue: 0, useNativeDriver: true, damping: 20, stiffness: 120 }).start();
      }
    },
  })).current;

  return (
    <View style={[styles.wrapper, containerStyle]}>
      <Animated.View style={[styles.containerWrapper, { transform: [{ translateY: panY }] }]}>
        <View {...panResponder.panHandlers} style={styles.indicatorContainer}>
          <View style={styles.handle} />
        </View>
        <View style={[styles.container, { height }]}>
          <View style={styles.content}>{children}</View>
        </View>
      </Animated.View>
    </View>
  );
};

const styles = StyleSheet.create({
  wrapper: { width: '100%', alignItems: 'center', height: '100%', justifyContent: 'flex-end', backgroundColor: 'rgba(0,0,0,0.6)' },
  containerWrapper: { width: '100%', alignItems: 'center' },
  indicatorContainer: { width: '100%', alignItems: 'center', paddingVertical: 8 },
  handle: { width: 40, height: 4, backgroundColor: '#E0E0E0', borderRadius: 2 },
  container: { width: '100%', backgroundColor: '#FFFFFF', borderTopLeftRadius: 30, borderTopRightRadius: 30, overflow: 'hidden' },
  content: { width: '100%', flex: 1, paddingTop: 20, paddingHorizontal: 25, paddingBottom: Platform.OS === 'ios' ? 40 : 24 },
});
