export interface LoginAuth {
  email: string;
  password: string;
}

export interface RegisterAuth {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface UserAuthResponse {
  token: string;
  userId?: number;
  email?: string;
}

export interface AuthState {
  userId: string | null;
  email: string | null;
  role: string | null;
  companyId: string | null;
}