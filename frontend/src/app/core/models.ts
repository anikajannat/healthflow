export type Role = 'Patient' | 'Doctor' | 'Admin' | 'SuperAdmin';

export interface User {
  id: string;
  name: string;
  email: string;
  role: Role;
  status: string;
  needPasswordChange?: boolean;
  imageUrl?: string;
  patientProfile?: any;
  doctorProfile?: any;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface Doctor {
  id: string;
  userId?: string;
  name: string;
  imageUrl?: string;
  specialization: string;
  qualifications?: string;
  experienceYears: number;
  consultationFee: number;
  bio?: string;
  averageRating: number;
  reviewCount: number;
}

export interface Schedule {
  id: string;
  date: string;
  startTime: string;
  endTime: string;
  totalSlots: number;
  bookedSlots: number[];
  unavailableSlots?: number[];
  availableSlots: number;
  doctor: Doctor;
}

export interface BookingResponse {
  message: string;
  id: string;
  serialNumber: number;
  appointmentStart: string;
  payment: {
    amount: number;
    provider: string;
    transactionId: string;
    status: string;
  };
}
