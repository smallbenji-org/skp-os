<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Topbar from './components/Topbar.vue'
import Sidebar from './components/Sidebar.vue'
import AuthView from './views/AuthView.vue'
import { useAuthStore } from '@/Stores/AuthStore'

const authStore = useAuthStore()
const loading = ref(true)

onMounted(async () => {
  try {
    await authStore.GET_ME()
  } finally {
    loading.value = false
  }
})

import HomeView from './views/HomeView.vue'
import ProjectsView from './views/ProjectsView.vue'
import SKPProjectsView from './views/SKPProjectsView.vue'
import LogView from './views/LogView.vue'
import FFView from './views/FFView.vue'
import LocationView from './views/LocationView.vue'
import MessagesView from './views/MessagesView.vue'
import InfoView from './views/InfoView.vue'
import HelpView from './views/HelpView.vue'
import SettingsView from './views/SettingsView.vue'

const isSidebarCollapsed = ref(false)
const activeTab = ref('forside')

const toggleSidebar = () => {
  isSidebarCollapsed.value = !isSidebarCollapsed.value
}

const currentViewComponent = computed(() => {
  switch (activeTab.value) {
    case 'forside':
      return HomeView
    case 'projekter':
      return ProjectsView
    case 'skp-projekter':
      return SKPProjectsView
    case 'logbog':
      return LogView
    case 'ff':
      return FFView
    case 'location':
      return LocationView
    case 'meddelelser':
      return MessagesView
    case 'info':
      return InfoView
    case 'hjaelp':
      return HelpView
    case 'indstillinger':
      return SettingsView
    default:
      return HomeView
  }
})
</script>

<template>
  <div v-if="loading" class="app-loader">
    <div class="loader-spinner"></div>
  </div>
  <AuthView v-else-if="!authStore.IS_AUTHENTICATED" />
  <main v-else class="main-page">
    <Sidebar
      v-model:collapsed="isSidebarCollapsed"
      v-model:active-tab="activeTab"
    />
    <div class="app-body">
      <Topbar
        :is-sidebar-collapsed="isSidebarCollapsed"
        :user-name="authStore.ME?.name || authStore.ME?.email || 'Bruger'"
        @toggle-sidebar="toggleSidebar"
        @logout="authStore.LOGOUT"
      />
      <div class="content-area" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
        <Transition name="fade" mode="out-in">
          <component :is="currentViewComponent" :key="activeTab" />
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

.app-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100vw;
  height: 100vh;
  background-color: #E1E6EA;
}

.loader-spinner {
  width: 40px;
  height: 40px;
  border: 3.5px solid rgba(1, 107, 255, 0.2);
  border-top-color: #016BFF;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
