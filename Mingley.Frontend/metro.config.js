const { getDefaultConfig } = require('expo/metro-config');

const config = getDefaultConfig(__dirname);

// Enable web support
config.resolver.platforms = ['web', 'ios', 'android', 'native'];

// Resolve platform-specific extensions properly for web
config.resolver.sourceExts = [
  'web.js', 'web.jsx', 'web.ts', 'web.tsx',
  'js', 'jsx', 'ts', 'tsx', 'json', 'cjs', 'mjs'
];

module.exports = config;
