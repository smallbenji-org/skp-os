<script setup lang="ts">
import Topbar from './components/Topbar.vue'
import Sidebar from './components/Sidebar.vue'
</script>

<template>
  <main class="main-page">
    <Sidebar />
    <Topbar />
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
