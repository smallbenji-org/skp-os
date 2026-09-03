<script setup lang="ts">
import { ref, watch, nextTick, onMounted, onUnmounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { 
  IconHome, 
  IconFolderOpen, 
  IconNotes, 
  IconEdit, 
  IconClock, 
  IconMapPin, 
  IconInbox, 
  IconInfoCircle, 
  IconHelpCircle,
  IconSettings
} from '@tabler/icons-vue'

const isCollapsed = defineModel<boolean>('collapsed', { default: false })

const route = useRoute()

const tabs = [
  { name: 'forside', label: 'Forside', icon: IconHome },
  { name: 'projekter', label: 'Mine Projekter', icon: IconFolderOpen },
  { name: 'skp-projekter', label: 'SKP Projekter', icon: IconNotes },
  { name: 'logbog', label: 'Logbog', icon: IconEdit },
  { name: 'ff', label: 'FF Timer', icon: IconClock },
  { name: 'location', label: 'Tjek Ind', icon: IconMapPin },
  { name: 'meddelelser', label: 'Aktuelle Meddelelser', icon: IconInbox },
  { name: 'info', label: 'Information', icon: IconInfoCircle },
  { name: 'hjaelp', label: 'Hjælp', icon: IconHelpCircle },
]

const activeName = computed(() => (route.name as string) || 'forside')

const sidebarRef = ref<HTMLElement | null>(null)
const tabRefs = new Map<string, HTMLElement>()
const indicatorTop = ref(0)
const hasInitialized = ref(false)

const setTabRef = (name: string, el: any) => {
  if (el) {
    tabRefs.set(name, el as HTMLElement)
  } else {
    tabRefs.delete(name)
  }
}

const updateIndicator = () => {
  const activeEl = tabRefs.get(activeName.value)
  const sidebarEl = sidebarRef.value
  if (activeEl && sidebarEl) {
    const sidebarRect = sidebarEl.getBoundingClientRect()
    const activeRect = activeEl.getBoundingClientRect()
    const borderTop = parseFloat(getComputedStyle(sidebarEl).borderTopWidth) || 0
    indicatorTop.value = activeRect.top - sidebarRect.top - borderTop
  }
}

watch(activeName, () => {
  nextTick(() => {
    updateIndicator()
  })
})

watch(isCollapsed, () => {
  nextTick(() => {
    updateIndicator()
  })
})

let resizeObserver: ResizeObserver | null = null

onMounted(() => {
  updateIndicator()
  window.addEventListener('resize', updateIndicator)

  if (sidebarRef.value && typeof ResizeObserver !== 'undefined') {
    resizeObserver = new ResizeObserver(() => {
      updateIndicator()
    })
    resizeObserver.observe(sidebarRef.value)
  }

  requestAnimationFrame(() => {
    hasInitialized.value = true
  })
})

onUnmounted(() => {
  window.removeEventListener('resize', updateIndicator)
  resizeObserver?.disconnect()
})
</script>

<template>
  <aside 
    ref="sidebarRef" 
    class="sidebar" 
    :class="{ collapsed: isCollapsed }" 
    aria-label="Sidebar"
  >
    <div class="sidebar-header">
      <h1 class="sidebar-title">SKP OS</h1>
    </div>

    <div 
      class="active-indicator" 
      :class="{ animated: hasInitialized }"
      :style="{ 
        transform: `translateY(${indicatorTop}px)`
      }"
    />

    <nav class="tabs-list">
      <RouterLink
        v-for="tab in tabs"
        :key="tab.name"
        :ref="(el) => setTabRef(tab.name, el as any)"
        :to="{ name: tab.name }"
        class="tab-item"
        :class="{ active: activeName === tab.name }"
        :title="isCollapsed ? tab.label : undefined"
        :aria-label="tab.label"
      >
        <component :is="tab.icon" :size="20" :stroke-width="2" class="tab-icon" />
        <span class="tab-label">{{ tab.label }}</span>
      </RouterLink>
    </nav>

    <div class="sidebar-footer">
      <RouterLink
        :ref="(el) => setTabRef('indstillinger', el as any)"
        :to="{ name: 'indstillinger' }"
        class="tab-item"
        :class="{ active: activeName === 'indstillinger' }"
        :title="isCollapsed ? 'Indstillinger' : undefined"
        aria-label="Indstillinger"
      >
        <IconSettings :size="20" :stroke-width="2" class="tab-icon" />
        <span class="tab-label">Indstillinger</span>
      </RouterLink>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  width: 250px;
  height: 100vh;
  border: 2px solid transparent;

  background: 
    linear-gradient(white 0% 100%) padding-box,
    linear-gradient(to right, #fff 75%, #e2e2e2 100%) border-box;

  border-top-right-radius: 40px;
  border-bottom-right-radius: 40px;
  box-shadow: 2px 0 16px rgba(0, 0, 0, 0.12);
  z-index: 100;
  box-sizing: border-box;
  padding: 24px 14px;
  display: flex;
  flex-direction: column;
  transition: width 0.35s cubic-bezier(0.4, 0, 0.2, 1), padding 0.35s cubic-bezier(0.4, 0, 0.2, 1);
  overflow-x: hidden;
}

.sidebar.collapsed {
  width: 72px;
  padding: 24px 10px;
}

.sidebar-header {
  display: flex;
  align-items: center;
  margin-bottom: 18px;
  min-height: 36px;
  padding: 0 8px;
}

.sidebar.collapsed .sidebar-header {
  justify-content: center;
  padding: 0;
}

.sidebar-title {
  font-size: 1.25rem;
  font-weight: 800;
  color: #1a1a1a;
  letter-spacing: -0.5px;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  max-width: 160px;
  opacity: 1;
  transition: max-width 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.2s ease;
}

.sidebar.collapsed .sidebar-title {
  max-width: 0;
  opacity: 0;
}

.active-indicator {
  position: absolute;
  top: 0;
  left: 14px;
  right: 14px;
  height: 44px;
  background: #016BFF;
  border-radius: 14px;
  pointer-events: none;
  z-index: 1;
}

.active-indicator.animated {
  transition: 
    transform 0.35s cubic-bezier(0.34, 1.35, 0.64, 1),
    left 0.35s cubic-bezier(0.4, 0, 0.2, 1),
    right 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.sidebar.collapsed .active-indicator {
  left: 10px;
  right: 10px;
}

.tabs-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}

.tab-item {
  position: relative;
  z-index: 2;
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  height: 44px;
  padding: 0 14px;
  border: none;
  background-color: transparent;
  color: #4b5563;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  outline: none;
  text-align: left;
  font-family: inherit;
  user-select: none;
  text-decoration: none;
  transition: 
    color 0.25s ease, 
    transform 0.15s ease, 
    background-color 0.2s ease,
    padding 0.35s cubic-bezier(0.4, 0, 0.2, 1),
    gap 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.sidebar.collapsed .tab-item {
  padding: 0 16px;
  gap: 0;
  justify-content: center;
}

.tab-item:hover:not(.active) {
  background-color: rgba(0, 0, 0, 0.04);
  color: #111827;
}

.tab-item:active {
  transform: scale(0.97);
}

.tab-item.active {
  color: #ffffff;
}

.tab-icon {
  flex-shrink: 0;
  transition: transform 0.3s cubic-bezier(0.34, 1.5, 0.64, 1);
}

.tab-item.active .tab-icon {
  transform: scale(1.12);
}

.tab-label {
  white-space: nowrap;
  overflow: hidden;
  max-width: 160px;
  opacity: 1;
  transition: max-width 0.35s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.2s ease;
}

.sidebar.collapsed .tab-label {
  max-width: 0;
  opacity: 0;
}

.sidebar-footer {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  width: 100%;
}
</style>
