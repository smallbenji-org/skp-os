import type { AxiosResponse } from "axios";
import type { LoginDto, MeDto, RegisterDto, RolesDto, UserDto } from "@/types";
import { api } from "./api";

export default class AuthService {
  public async register(data: RegisterDto): Promise<UserDto | null> {
    try {
      const response: AxiosResponse<UserDto> = await api({
        url: "/api/auth/register",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async login(data: LoginDto): Promise<UserDto | null> {
    try {
      const response: AxiosResponse<UserDto> = await api({
        url: "/api/auth/login",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async logout(): Promise<boolean> {
    try {
      await api({
        url: "/api/auth/logout",
        method: "POST"
      });
      return true;
    } catch {
      return false;
    }
  }

  public async getMe(): Promise<MeDto | null> {
    try {
      const response: AxiosResponse<MeDto> = await api({
        url: "/api/auth/me",
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async getRoles(): Promise<RolesDto | null> {
    try {
      const response: AxiosResponse<RolesDto> = await api({
        url: "/api/auth/roles",
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }
}
