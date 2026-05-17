import * as yup from 'yup';

export const loginSchema = yup.object().shape({
  email: yup
    .string()
    .email('Please enter a valid email')
    .required('Email is required'),
  password: yup
    .string()
    .min(6, 'Password must be at least 6 characters')
    .required('Password is required'),
});

export const profileSchema = yup.object().shape({
  name: yup.string().required('Name is required'),
  bio: yup.string().max(300, 'Bio must be at most 300 characters'),
  age: yup
    .number()
    .min(18, 'You must be at least 18 years old')
    .max(100, 'Please enter a valid age')
    .required('Age is required'),
});
