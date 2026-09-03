<script setup lang="ts">
import { IconChevronRight, IconUser, IconLogout } from '@tabler/icons-vue'

const props = withDefaults(defineProps<{
  isSidebarCollapsed?: boolean
  userName?: string
}>(), {
  isSidebarCollapsed: false,
  userName: 'Mikkel Martin Larsen'
})

const emit = defineEmits<{
  (e: 'toggle-sidebar'): void
  (e: 'profile-click'): void
  (e: 'logout'): void
}>()
</script>

<template>
  <header class="topbar">
    <div class="topbar-left">
      <button
        class="collapse-toggle-btn"
        :class="{ 'sidebar-collapsed': isSidebarCollapsed }"
        type="button"
        :aria-label="isSidebarCollapsed ? 'Udvid sidebar' : 'Skjul sidebar'"
        :title="isSidebarCollapsed ? 'Udvid sidebar' : 'Skjul sidebar'"
        @click="emit('toggle-sidebar')"
      >
        <IconChevronRight
          :size="18"
          :stroke-width="2.5"
          class="collapse-chevron"
          :class="{ flipped: !isSidebarCollapsed }"
        />
      </button>
    </div>

    <div class="topbar-right">
      <button
        class="profile-btn"
        type="button"
        aria-label="Brugerprofil"
        @click="emit('profile-click')"
      >
        <div class="profile-avatar">
          <IconUser :size="16" :stroke-width="2.2" />
        </div>
        <span class="profile-text">
          <span class="profile-name">{{ userName }}</span>
        </span>
      </button>

      <button
        class="logout-btn"
        type="button"
        aria-label="Log af"
        @click="emit('logout')"
      >
        <IconLogout :size="16" :stroke-width="2.2" class="logout-icon" />
        <span>Log af</span>
      </button>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  height: 50px;
  padding: 0 16px 0 12px;
  background-color: #fff;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border-bottom: 2px solid #e2e2e2;
  box-sizing: border-box;
}

.topbar-left {
  display: flex;
  align-items: center;
}

.collapse-toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  margin-left: 240px;
  border-radius: 10px;
  background-color: #f8fafc;
  border: none;
  color: #4b5563;
  cursor: pointer;
  outline: none;
  flex-shrink: 0;
  padding: 0;
  transition: 
    margin-left 0.35s cubic-bezier(0.4, 0, 0.2, 1),
    background-color 0.2s ease, 
    color 0.2s ease, 
    border-color 0.2s ease, 
    transform 0.15s ease;
}

.collapse-toggle-btn.sidebar-collapsed {
  margin-left: 65px;
}

.collapse-toggle-btn:hover {
  color: #111827;
  background-color: #f1f5f9;
}

.collapse-toggle-btn:active {
  transform: scale(0.92);
}

.collapse-chevron {
  transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
  transform: rotate(0deg);
}

.collapse-chevron.flipped {
  transform: rotate(180deg);
}

.topbar-right {
  display: flex;
  align-items: center;
  gap: 10px;
}

.profile-btn {
  display: flex;
  align-items: center;
  height: 34px;
  padding: 0 12px 0 6px;
  background-color: #f8fafc;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  cursor: pointer;
  outline: none;
  font-family: inherit;
  user-select: none;
  transition: 
    background-color 0.2s ease,
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.15s ease;
}

.profile-btn:hover {
  background-color: #f1f5f9;
  border-color: #cbd5e1;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.profile-btn:active {
  transform: scale(0.97);
}

.profile-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 7px;
  background: #eff6ff;
  color: #016BFF;
  flex-shrink: 0;
}

.profile-text {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  white-space: nowrap;
}

.profile-label {
  color: #6b7280;
  font-weight: 500;
}

.profile-name {
  color: #111827;
  font-weight: 600;
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 12px;
  background-color: #ef4444;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  outline: none;
  user-select: none;
  transition: 
    background-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.15s ease;
}

.logout-btn:hover {
  background-color: #dc2626;
  box-shadow: 0 3px 10px rgba(239, 68, 68, 0.3);
}

.logout-btn:active {
  transform: scale(0.97);
}

.logout-icon {
  flex-shrink: 0;
}
</style>

