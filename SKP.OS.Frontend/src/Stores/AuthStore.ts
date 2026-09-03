import AuthService from "@/Services/AuthService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { LoginDto, MeDto, RegisterDto, RolesDto, UserDto } from "@/types";

export const useAuthStore = defineStore("auth", () => {
  const authService = new AuthService();

  const Me = ref<MeDto | null>(null);
  const Roles = ref<string[]>([]);
  const User = ref<UserDto | null>(null);

  const ME = computed(() => Me.value);
  const ROLES = computed(() => Roles.value);
  const USER = computed(() => User.value);
  const IS_AUTHENTICATED = computed(() => Me.value !== null);

  async function LOGIN(data: LoginDto) {
    const user = await authService.login(data);
    if (user) {
      User.value = user;
      await GET_ME();
    }
    return user;
  }

  async function REGISTER(data: RegisterDto) {
    const user = await authService.register(data);
    if (user) {
      User.value = user;
    }
    return user;
  }

  async function LOGOUT() {
    const success = await authService.logout();
    if (success) {
      Me.value = null;
      User.value = null;
      Roles.value = [];
    }
    return success;
  }

  async function GET_ME() {
    const data = await authService.getMe();
    Me.value = data;
    if (data) {
      Roles.value = data.roles;
    }
    return data;
  }

  async function GET_ROLES() {
    const data: RolesDto | null = await authService.getRoles();
    if (data) {
      Roles.value = data.roles;
    }
    return data;
  }

  function HAS_ROLE(role: string) {
    return Roles.value.includes(role);
  }

  function HAS_ANY_ROLE(roles: string[]) {
    return roles.some((role) => HAS_ROLE(role));
  }

  function HAS_ALL_ROLES(roles: string[]) {
    return roles.every((role) => HAS_ROLE(role));
  }

  function CLEAR() {
    Me.value = null;
    User.value = null;
    Roles.value = [];
  }

  return {
    Me, Roles, User,
    ME, ROLES, USER, IS_AUTHENTICATED,
    LOGIN, REGISTER, LOGOUT, GET_ME, GET_ROLES,
    HAS_ROLE, HAS_ANY_ROLE, HAS_ALL_ROLES, CLEAR
  }
});
