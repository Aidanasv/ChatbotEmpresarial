export interface LoginAuth {
  email: string;
  password: string;
}

export interface RegisterAuth {
  userName: string;
  email: string;
  password: string;
}

export interface ForgotPasswordPayload {
  email: string;
}

export interface ResetPasswordPayload {
  email: string;
  token: string;
  newPassword: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

export interface UserAuthResponse {
  token: string;
  userId?: number;
  email?: string;
  chatbotId?: number;
}

export interface ApiMessageResponse {
  message: string;
}

export interface AuthState {
  userId: string | null;
  email: string | null;
  role: string | null;
  companyId: string | null;
  chatbotId: string | null;
}