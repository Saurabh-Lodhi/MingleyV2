import React, { useEffect, useRef } from 'react';
import { NavigationContainer }        from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator }   from '@react-navigation/bottom-tabs';
import { Ionicons }                   from '@expo/vector-icons';
import { Alert, View, ActivityIndicator } from 'react-native';

import { useAuthStore }  from './src/store/useAuthStore';
import { useChatStore }  from './src/store/useChatStore';
import { useUserStore }  from './src/store/useUserStore';
import {
  connectChatHub, connectNotifHub, disconnectAll,
  onNewMatch, onUnmatched, onUserOnlineStatus,
  onNewMessage, onMessagesRead, onMessageDeleted,
  onIncomingCall, onCallAnswered, onCallDeclined, onCallEnded,
  onNewSuperChat, onSuperChatResponded,
  onNewNotification,
} from './src/services/socket';

// ── Auth screens ───────────────────────────────────────────────────────────────
import { WelcomeScreen }            from './src/features/auth/screens/WelcomeScreen';
import { LoginScreen }              from './src/features/auth/screens/LoginScreen';
import { EmailInputScreen }         from './src/features/auth/screens/EmailInputScreen';
import { OTPVerificationScreen }    from './src/features/auth/screens/OTPVerificationScreen';
import { SignupOptionsScreen }      from './src/features/auth/screens/SignupOptionsScreen';
import { ProfileDetailsScreen }     from './src/features/profile-setup/screens/ProfileDetailsScreen';
import { GenderSelectionScreen }    from './src/features/profile-setup/screens/GenderSelectionScreen';
import { InterestsSelectionScreen } from './src/features/profile-setup/screens/InterestsSelectionScreen';

// ── Main screens ───────────────────────────────────────────────────────────────
import { MatchesScreen }            from './src/features/matches/screens/MatchesScreen';
import { SubscriptionPlansScreen }  from './src/features/subscription/screens/SubscriptionPlansScreen';
import { PaymentScreen }            from './src/features/subscription/screens/PaymentScreen';
import { ChatScreen }               from './src/screens/ChatScreen';
import { CallScreen }               from './src/screens/CallScreen';
import { ConversationsScreen }      from './src/screens/ConversationsScreen';
import { DiscoverScreen }           from './src/screens/DiscoverScreen';
import { ProfileScreen }            from './src/screens/ProfileScreen';
import { NotificationsScreen }      from './src/screens/NotificationsScreen';
import { SuperChatScreen }          from './src/screens/SuperChatScreen';

const Stack = createNativeStackNavigator();
const Tab   = createBottomTabNavigator();

function MainTabs() {
  const getTotalUnread = useChatStore(s => s.getTotalUnread);
  const unread = getTotalUnread();

  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarActiveTintColor:   '#E94057',
        tabBarInactiveTintColor: '#BBB',
        tabBarStyle: { backgroundColor: '#FFF', borderTopColor: '#F0F0F0', height: 60, paddingBottom: 8 },
        tabBarIcon: ({ color, size, focused }) => {
          const icons = { Discover: 'flame', Matches: 'heart', Messages: 'chatbubbles', Profile: 'person' };
          const name  = icons[route.name];
          return <Ionicons name={focused ? name : `${name}-outline`} size={size} color={color} />;
        },
        // FIX: Show unread badge on Messages tab
        tabBarBadge: route.name === 'Messages' && unread > 0 ? unread : undefined,
      })}
    >
      <Tab.Screen name="Discover"  component={DiscoverScreen} />
      <Tab.Screen name="Matches"   component={MatchesScreen}  />
      <Tab.Screen name="Messages"  component={ConversationsScreen} />
      <Tab.Screen name="Profile"   component={ProfileScreen}  />
    </Tab.Navigator>
  );
}

export default function App() {
  const { isAuthenticated, isLoading, restoreSession } = useAuthStore();
  const { receiveMessage, onMessagesRead: chatRead, onMessageDeleted: chatDeleted, loadWallet } = useChatStore();
  const { addMatch, removeMatch, updateOnlineStatus } = useUserStore();
  const navRef = useRef(null);

  // ── Restore session ONCE on mount ─────────────────────────
  // FIX: No interval — restoreSession is called once, not repeatedly
  useEffect(() => {
    restoreSession();
  }, []);

  // ── Connect SignalR after login ────────────────────────────
  useEffect(() => {
    if (!isAuthenticated) { disconnectAll(); return; }

    // Load wallet/subscription status
    loadWallet();

    let unsubs = [];
    (async () => {
      await connectChatHub();
      await connectNotifHub();

      unsubs.push(onNewMatch((data) => {
        addMatch(data);
        Alert.alert('🎉 New Match!', `You matched with ${data.user?.fullName}!`);
      }));

      unsubs.push(onUnmatched((data) => {
        removeMatch(data.matchId);
      }));

      // FIX: Update online status in real-time
      unsubs.push(onUserOnlineStatus((data) => {
        updateOnlineStatus(data.userId, data.isOnline, data.lastSeen);
      }));

      unsubs.push(onNewMessage((data) => {
        receiveMessage(data.matchId || data.chatId, data.message);
      }));

      unsubs.push(onMessagesRead((data) => {
        chatRead(data.matchId || data.chatId);
      }));

      unsubs.push(onMessageDeleted((data) => {
        chatDeleted(data.matchId || data.chatId, data.messageId);
      }));

      // ── Calls ────────────────────────────────────────────────
      unsubs.push(onIncomingCall((data) => {
        Alert.alert(
          `📞 Incoming ${data.callType === 'video' ? '📹 Video' : '🎙️ Voice'} Call`,
          `${data.caller?.fullName} is calling you`,
          [
            {
              text: '❌ Decline',
              style: 'destructive',
              onPress: () => navRef.current?.navigate('Call', {
                callId: data.callId, action: 'decline', callType: data.callType?.toUpperCase() || 'VOICE',
                caller: data.caller,
              }),
            },
            {
              text: '✅ Answer',
              onPress: () => navRef.current?.navigate('Call', {
                callId: data.callId, action: 'answer', callType: data.callType?.toUpperCase() || 'VOICE',
                caller: data.caller,
                isInitiator: false,
              }),
            },
          ]
        );
      }));

      unsubs.push(onCallAnswered((data) => {
        console.log('✅ Call answered:', data.callId);
      }));

      unsubs.push(onCallDeclined((data) => {
        Alert.alert('📵 Call Declined', 'The user declined your call.');
      }));

      unsubs.push(onCallEnded((data) => {
        if (data.newBalance !== undefined)
          useChatStore.getState().updateCoinBalance(data.newBalance);
      }));

      unsubs.push(onNewSuperChat((data) => {
        Alert.alert(
          '⭐ SuperChat Received!',
          `${data.fromName}: "${data.message}"`,
          [
            { text: 'Later' },
            { text: 'View & Respond', onPress: () => navRef.current?.navigate('SuperChat', { superChatId: data.superChatId }) },
          ]
        );
      }));

      unsubs.push(onSuperChatResponded((data) => {
        addMatch({ matchId: data.matchId, chatId: data.chatId, matchedUser: data.user, matchedAt: new Date().toISOString() });
        Alert.alert('🎉 SuperChat Match!', `${data.user?.fullName} responded! You're now matched.`);
      }));

      unsubs.push(onNewNotification((data) => {
        console.log('🔔 Notification:', data.title);
      }));

    })().catch(console.error);

    return () => { unsubs.forEach(fn => fn && fn()); };
  }, [isAuthenticated]);

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#FFF' }}>
        <ActivityIndicator size="large" color="#E94057" />
      </View>
    );
  }

  return (
    <NavigationContainer ref={navRef}>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {isAuthenticated ? (
          <>
            <Stack.Screen name="Main"          component={MainTabs} />
            <Stack.Screen name="Chat"          component={ChatScreen} />
            <Stack.Screen name="Call"          component={CallScreen} />
            <Stack.Screen name="SuperChat"     component={SuperChatScreen} />
            <Stack.Screen name="Subscription"  component={SubscriptionPlansScreen} />
            <Stack.Screen name="Payment"       component={PaymentScreen} />
            <Stack.Screen name="Notifications" component={NotificationsScreen} />
          </>
        ) : (
          <>
            <Stack.Screen name="Welcome"          component={WelcomeScreen} />
            <Stack.Screen name="SignupOptions"    component={SignupOptionsScreen} />
            <Stack.Screen name="EmailInput"       component={EmailInputScreen} />
            <Stack.Screen name="OTPVerification"  component={OTPVerificationScreen} />
            <Stack.Screen name="Login"            component={LoginScreen} />
            <Stack.Screen name="ProfileDetails"   component={ProfileDetailsScreen} />
            <Stack.Screen name="GenderSelection"  component={GenderSelectionScreen} />
            <Stack.Screen name="PhoneInput" component={EmailInputScreen} />
            <Stack.Screen name="Interests"        component={InterestsSelectionScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}
