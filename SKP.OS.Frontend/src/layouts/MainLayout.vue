<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Topbar from '@/components/Topbar.vue'
import Sidebar from '@/components/Sidebar.vue'
import { useAuthStore } from '@/Stores/AuthStore'

const router = useRouter()
const authStore = useAuthStore()

const isSidebarCollapsed = ref(false)

const toggleSidebar = () => {
  isSidebarCollapsed.value = !isSidebarCollapsed.value
}

const handleLogout = async () => {
  await authStore.LOGOUT()
  router.push({ name: 'login' })
}
</script>

<template>
  <main class="main-page">
    <Sidebar v-model:collapsed="isSidebarCollapsed" />
    <div class="app-body">
      <Topbar
        :is-sidebar-collapsed="isSidebarCollapsed"
        :user-name="authStore.ME?.name || authStore.ME?.email || 'Bruger'"
        @toggle-sidebar="toggleSidebar"
        @logout="handleLogout"
      />
      <div class="content-area" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
        <Transition name="fade" mode="out-in">
          <RouterView :key="$route.name" />
        </Transition>
      </div>
    </div>
  </main>
</template>

<style scoped>
.main-page {
  position: relative;
  width: 100vw;
  height: 100vh;
  background-color: #E1E6EA;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.app-body {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: 100%;
  overflow: hidden;
}

.content-area {
  flex: 1;
  margin-left: 250px;
  padding: 24px 32px;
  overflow-y: auto;
  transition: margin-left 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.content-area.sidebar-collapsed {
  margin-left: 72px;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(6px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
