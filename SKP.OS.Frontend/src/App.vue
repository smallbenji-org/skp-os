<script setup lang="ts">
import { ref, computed } from 'vue'
import Topbar from './components/Topbar.vue'
import Sidebar from './components/Sidebar.vue'

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
  <main class="main-page">
    <Sidebar
      v-model:collapsed="isSidebarCollapsed"
      v-model:active-tab="activeTab"
    />
    <div class="app-body">
      <Topbar
        :is-sidebar-collapsed="isSidebarCollapsed"
        @toggle-sidebar="toggleSidebar"
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
</style>
