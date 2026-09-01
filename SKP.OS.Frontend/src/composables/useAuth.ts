import { readonly, ref } from 'vue'
import type { MeDto } from '../types'

const user = ref<MeDto | null>(null)
const loading = ref(false)
const loaded = ref(false)

async function fetchMe(): Promise<MeDto | null> {
  const response = await fetch('/api/auth/me', { credentials: 'include' })
  if (!response.ok) {
    user.value = null
    loaded.value = true
    return null
  }
  const data = (await response.json()) as MeDto
  user.value = data
  loaded.value = true
  return data
}

async function loadUser(): Promise<MeDto | null> {
  if (loaded.value) {
    return user.value
  }
  loading.value = true
  try {
    return await fetchMe()
  } finally {
    loading.value = false
  }
}

async function refreshUser(): Promise<MeDto | null> {
  loaded.value = false
  return loadUser()
}

function hasRole(role: string): boolean {
  return !!user.value?.roles?.includes(role)
}

function hasAnyRole(roles: string[]): boolean {
  return roles.some((role) => hasRole(role))
}

function hasAllRoles(roles: string[]): boolean {
  if (roles.length === 0) {
    return true
  }
  return roles.every((role) => hasRole(role))
}

function clearUser(): void {
  user.value = null
  loaded.value = false
}

export function useAuth() {
  return {
    user: readonly(user),
    loading: readonly(loading),
    loaded: readonly(loaded),
    loadUser,
    refreshUser,
    fetchMe,
    hasRole,
    hasAnyRole,
    hasAllRoles,
    clearUser,
  }
}
