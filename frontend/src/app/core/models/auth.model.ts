export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullname: string;
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  fullname: string;
}

export interface AppUser {
  id?: string;
  username: string;
  email: string;
  fullname: string;
  avatarUrl?: string;
  createdAt?: Date;
}

export interface User {
  username: string;
  email: string;
}