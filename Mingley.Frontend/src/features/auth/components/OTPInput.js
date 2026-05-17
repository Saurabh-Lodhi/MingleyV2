import React, { useRef, useState } from 'react';
import { View, TextInput, StyleSheet } from 'react-native';
import { Controller } from 'react-hook-form';
import { COLORS, SPACING } from '../../../constants/theme';

const NUM_DIGITS = 6;

export const OTPInput = ({ control, name }) => {
  const [code, setCode] = useState(Array(NUM_DIGITS).fill(''));
  const inputs = useRef([]);

  return (
    <Controller
      control={control} name={name}
      render={({ field: { onChange } }) => {
        const handleChange = (text, index) => {
          const newCode = [...code];
          newCode[index] = text;
          setCode(newCode);
          onChange(newCode.join(''));
          if (text && index < NUM_DIGITS - 1) inputs.current[index + 1]?.focus();
        };
        const handleKey = (e, index) => {
          if (e.nativeEvent.key === 'Backspace' && !code[index] && index > 0) {
            inputs.current[index - 1]?.focus();
          }
        };
        return (
          <View style={styles.container}>
            {code.map((digit, i) => (
              <View key={i} style={[styles.inputContainer, digit ? styles.inputFilled : styles.inputEmpty]}>
                <TextInput
                  ref={r => (inputs.current[i] = r)}
                  style={[styles.input, digit ? styles.textFilled : styles.textEmpty]}
                  keyboardType="number-pad" maxLength={1} value={digit}
                  onChangeText={t => handleChange(t, i)} onKeyPress={e => handleKey(e, i)}
                  placeholder="0" placeholderTextColor="#E8E8E8"
                />
              </View>
            ))}
          </View>
        );
      }}
    />
  );
};

const styles = StyleSheet.create({
  container: { flexDirection: 'row', justifyContent: 'center', gap: SPACING.s, marginVertical: SPACING.xl },
  inputContainer: { width: 50, height: 55, borderRadius: 14, justifyContent: 'center', alignItems: 'center', overflow: 'hidden' },
  inputFilled: { backgroundColor: '#E94057' },
  inputEmpty: { backgroundColor: '#FFFFFF', borderWidth: 1, borderColor: '#E8E8E8' },
  input: { fontSize: 28, fontWeight: 'bold', textAlign: 'center', width: '100%', height: '100%' },
  textFilled: { color: '#FFFFFF' },
  textEmpty: { color: '#E94057' },
});
