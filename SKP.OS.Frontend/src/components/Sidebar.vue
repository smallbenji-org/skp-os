<script setup lang="ts">
import { ref } from 'vue'
import { IconHome, IconFolderOpen, IconNotes, IconEdit, IconClock, IconMapPin, IconInbox, IconInfoCircle, IconHelpCircle } from '@tabler/icons-vue'

const activeTab = ref('forside')

const tabs = [
  { id: 'forside', name: 'Forside', icon: IconHome },
  { id: 'projekter', name: 'Mine Projekter', icon: IconFolderOpen },
  { id: 'skp-projekter', name: 'SKP Projekter', icon: IconNotes },
  { id: 'logbog', name: 'Logbog', icon: IconEdit },
  { id: 'ff', name: 'FF Timer', icon: IconClock },
  { id: 'location', name: 'Tjek Ind', icon: IconMapPin },
  { id: 'meddelelser', name: 'Aktuelle Meddelelser', icon: IconInbox },
  { id: 'info', name: 'Information', icon: IconInfoCircle },
  { id: 'hjaelp', name: 'Hjælp', icon: IconHelpCircle },
]
</script>

<template>
  <aside class="sidebar" aria-label="Sidebar">
    <nav class="tabs-list">
      <h1>SKP OS</h1>

      <button
        v-for="tab in tabs"
        :key="tab.id"
        class="tab-item"
        :class="{ active: activeTab === tab.id }"
        @click="activeTab = tab.id"
      >
        <component :is="tab.icon" :size="22" :stroke-width="2" class="tab-icon" />
        <span class="tab-label">{{ tab.name }}</span>
      </button>
    </nav>
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
    linear-gradient(to right, #fff 75%, #ececec 100%) border-box;

  border-top-right-radius: 40px;
  border-bottom-right-radius: 40px;
  box-shadow: 2px 0 16px rgba(0, 0, 0, 0.15);
  z-index: 100;
  box-sizing: border-box;
  padding: 20px 12px;
  display: flex;
  flex-direction: column;
}

.tabs-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: 100%;
}

h1 {
  right: auto;
  margin-left: 10px;
  margin-bottom: 10px;
}

.tab-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 11px 14px;
  border: none;
  background-color: transparent;
  color: #000000;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  outline: none;
  transition: background-color 0.2s cubic-bezier(.5,0,.5,1), color 0.2s cubic-bezier(.5,0,.5,1);
  text-align: left;
  font-family: inherit;
}

.tab-item:hover:not(.active) {
  background-color: rgba(0, 0, 0, 0.04);
}

.tab-item.active {
  background-color: #0066ff;
  color: #ffffff;
}

.tab-icon {
  flex-shrink: 0;
}

.tab-label {
  white-space: nowrap;
}
</style>
