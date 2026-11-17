export interface AuthSession {
  token: string;
  expiration: string;
  email: string;
  roles: string[];
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  token: string;
  email: string;
  roles: string[];
  expires: string;
}