<script setup lang="ts">
import { ref } from 'vue'
import Topbar from './components/Topbar.vue'
import Sidebar from './components/Sidebar.vue'

const isSidebarCollapsed = ref(false)

const toggleSidebar = () => {
  isSidebarCollapsed.value = !isSidebarCollapsed.value
}
</script>

<template>
  <main class="main-page">
    <Sidebar v-model:collapsed="isSidebarCollapsed" />
    <Topbar
      :is-sidebar-collapsed="isSidebarCollapsed"
      @toggle-sidebar="toggleSidebar"
    />
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
</style>
