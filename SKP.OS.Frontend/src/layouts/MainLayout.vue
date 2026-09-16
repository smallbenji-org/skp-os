<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Topbar from '@/components/Topbar.vue'
import Sidebar from '@/components/Sidebar.vue'
import { useAuthStore } from '@/Stores/AuthStore'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const isSidebarCollapsed = ref(false)
const isMobileOpen = ref(false)
const isMobile = ref(false)

const checkMobile = () => {
  isMobile.value = window.innerWidth <= 900
}

onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

watch(() => route.path, () => {
  isMobileOpen.value = false
})

const toggleSidebar = () => {
  if (isMobile.value) {
    isMobileOpen.value = !isMobileOpen.value
  } else {
    isSidebarCollapsed.value = !isSidebarCollapsed.value
  }
}

const handleProfileClick = () => {
  if (authStore.HAS_ROLE('Student')) {
    router.push({ name: 'profil' })
  } else {
    router.push({ name: 'indstillinger' })
  }
}

const handleLogout = async () => {
  await authStore.LOGOUT()
  router.push({ name: 'login' })
}
</script>

<template>
  <main class="main-page" role="main">
    <div
      v-if="isMobileOpen"
      class="sidebar-backdrop"
      @click="isMobileOpen = false"
    />
    <Sidebar
      v-model:collapsed="isSidebarCollapsed"
      :is-mobile-open="isMobileOpen"
      @close-mobile="isMobileOpen = false"
    />
    <div class="app-body">
      <Topbar
        :is-sidebar-collapsed="isSidebarCollapsed"
        :user-name="authStore.ME?.name || authStore.ME?.email || 'Bruger'"
        @toggle-sidebar="toggleSidebar"
        @profile-click="handleProfileClick"
        @logout="handleLogout"
      />
      <div class="content-area" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" :key="$route.name" />
          </transition>
        </router-view>
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

.sidebar-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  backdrop-filter: blur(2px);
  z-index: 99;
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

@media (max-width: 900px) {
  .content-area {
    margin-left: 0 !important;
    padding: 16px 12px;
    width: 100%;
    box-sizing: border-box;
  }
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
