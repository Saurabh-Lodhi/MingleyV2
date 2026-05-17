/**
 * Web-compatible LinearGradient shim.
 * On web: uses CSS background gradient via style prop.
 * On native: delegates to expo-linear-gradient.
 */
import { Platform } from 'react-native';

let LinearGradient;

if (Platform.OS === 'web') {
  const { View } = require('react-native');
  LinearGradient = ({ colors = [], style, children, start, end, ...props }) => {
    const angle = (() => {
      if (!start || !end) return '180deg';
      const dx = (end.x || 0) - (start.x || 0);
      const dy = (end.y || 1) - (start.y || 0);
      return `${Math.round(Math.atan2(dx, -dy) * (180 / Math.PI))}deg`;
    })();
    const gradient = `linear-gradient(${angle}, ${colors.join(', ')})`;
    return (
      <View
        {...props}
        style={[style, { background: gradient }]}
      >
        {children}
      </View>
    );
  };
} else {
  LinearGradient = require('expo-linear-gradient').LinearGradient;
}

export { LinearGradient };
