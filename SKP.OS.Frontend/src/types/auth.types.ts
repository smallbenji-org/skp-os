export interface RegisterDto {
    userName: string;
    email: string;
    name: string;
    password: string;
    role: string;
}

export interface LoginDto {
    userName: string;
    password: string;
}

export interface MeDto {
    name: string;
    email: string;
    roles: string[];
}

export interface RolesDto {
    roles: string[];
}
