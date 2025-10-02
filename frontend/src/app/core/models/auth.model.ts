export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
}

export interface AppUser {
  id?: string;
  username: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
  createdAt?: Date;
}

export interface User {
  username: string;
  email: string;
}