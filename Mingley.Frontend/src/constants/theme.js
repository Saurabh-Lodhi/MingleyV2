export const COLORS = {
  primary: '#E94057',
  secondary: '#8B4367',
  gradientStart: '#E94057',
  gradientEnd: '#8A2387',
  background: '#1A1A1A',
  surface: '#2C2C2C',
  text: '#FFFFFF',
  textSecondary: '#A0A0A0',
  error: '#FF4C4C',
  success: '#4CD964',
  white: '#FFFFFF',
  transparent: 'transparent',
  border: '#333333',
};

export const SPACING = {
  xs: 4,
  s: 8,
  m: 16,
  l: 24,
  xl: 32,
  xxl: 40,
};

import { Platform } from 'react-native';

const fontFamily = Platform.OS === 'ios' ? 'Avenir Next' : 'sans-serif';
const fontFamilyBold = Platform.OS === 'ios' ? 'AvenirNext-Bold' : 'sans-serif-medium';
const fontFamilyMedium = Platform.OS === 'ios' ? 'AvenirNext-Medium' : 'sans-serif-medium';

export const TYPOGRAPHY = {
  h1: {
    fontFamily: fontFamilyMedium,
    fontSize: 32,
    fontWeight: '600',
    color: COLORS.text,
  },
  h2: {
    fontFamily: fontFamilyMedium,
    fontSize: 24,
    fontWeight: '600',
    color: COLORS.text,
  },
  h3: {
    fontFamily: fontFamilyMedium,
    fontSize: 20,
    fontWeight: '600',
    color: COLORS.text,
  },
  body: {
    fontFamily,
    fontSize: 16,
    color: COLORS.text,
  },
  bodySecondary: {
    fontFamily,
    fontSize: 14,
    color: COLORS.textSecondary,
  },
  caption: {
    fontFamily,
    fontSize: 12,
    color: COLORS.textSecondary,
  },
};

export const SHADOWS = {
  small: {
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
    elevation: 2,
  },
  medium: {
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 4.65,
    elevation: 8,
  },
};
