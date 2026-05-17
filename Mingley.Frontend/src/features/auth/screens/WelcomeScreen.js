import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { LinearGradient } from '../../../components/compat/LinearGradient';
import { COLORS, SPACING, TYPOGRAPHY } from '../../../constants/theme';
import { Button } from '../../../components/common/Button';

export const WelcomeScreen = ({ navigation }) => {
  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[COLORS.gradientStart, COLORS.gradientEnd]}
        style={styles.gradient}
      >
        <View style={styles.content}>
          <View style={styles.headerContainer}>
            <Text style={styles.title}>Mingley</Text>
            <Text style={styles.subtitle}>Find your perfect match</Text>
          </View>
          
          <View style={styles.buttonContainer}>
            <Button
              title="Create Account"
              onPress={() => {}}
              style={styles.button}
              textStyle={styles.buttonTextPrimary}
              variant="custom" // Not primary to avoid gradient within gradient
            />
            <Button
              title="Sign In"
              onPress={() => navigation.navigate('Login')}
              variant="outline"
              style={styles.signInButton}
              textStyle={styles.signInText}
            />
          </View>
        </View>
      </LinearGradient>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  gradient: {
    flex: 1,
  },
  content: {
    flex: 1,
    paddingHorizontal: SPACING.xl,
    paddingTop: 100,
    paddingBottom: 60,
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  headerContainer: {
    justifyContent: 'center',
    alignItems: 'center',
  },
  title: {
    fontSize: 52,
    fontWeight: 'bold',
    color: '#FFFFFF',
    fontFamily: 'sans-serif-medium',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 18,
    color: '#FFFFFF',
    fontFamily: 'sans-serif',
    opacity: 0.9,
  },
  buttonContainer: {
    width: '100%',
  },
  button: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    height: 56,
  },
  buttonTextPrimary: {
    color: '#E94057',
    fontSize: 18,
    fontWeight: '600',
    fontFamily: 'sans-serif-medium',
  },
  signInButton: {
    borderColor: '#FFFFFF',
    borderWidth: 1.5,
    borderRadius: 16,
    height: 56,
    marginTop: 16,
    backgroundColor: 'transparent',
  },
  signInText: {
    color: '#FFFFFF',
    fontSize: 18,
    fontWeight: '600',
    fontFamily: 'sans-serif-medium',
  },
});
