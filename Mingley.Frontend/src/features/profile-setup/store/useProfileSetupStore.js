import { create } from 'zustand';

export const useProfileSetupStore = create((set) => ({
  profileDetails: {
    firstName: '',
    lastName: '',
    birthday: null,
    avatar: null,
  },
  gender: '',
  interests: [],

  setProfileDetails: (details) => set((state) => ({
    profileDetails: { ...state.profileDetails, ...details }
  })),

  setGender: (gender) => set({ gender }),

  toggleInterest: (interest) => set((state) => {
    const existing = state.interests.includes(interest);
    if (existing) {
      return { interests: state.interests.filter(i => i !== interest) };
    }
    return { interests: [...state.interests, interest] };
  }),

  clearProfileSetup: () => set({
    profileDetails: {
      firstName: '',
      lastName: '',
      birthday: null,
      avatar: null,
    },
    gender: '',
    interests: [],
  })
}));
