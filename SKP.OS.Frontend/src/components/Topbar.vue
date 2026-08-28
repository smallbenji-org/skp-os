<script setup lang="ts">
import { IconChevronRight } from '@tabler/icons-vue'

const props = withDefaults(defineProps<{
  isSidebarCollapsed?: boolean
}>(), {
  isSidebarCollapsed: false
})

const emit = defineEmits<{
  (e: 'toggle-sidebar'): void
}>()
</script>

<template>
  <header class="topbar">
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
  </header>
</template>

<style scoped>
.topbar {
  display: flex;
  align-items: center;
  width: 100%;
  height: 50px;
  padding: 0 12px;
  background-color: #fff;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border-bottom: 2px solid #e2e2e2;
  box-sizing: border-box;
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
</style>

