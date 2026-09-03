<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
</script>

<template>
  <div v-if="loading" class="app-loader">
    <div class="loader-spinner"></div>
  </div>
  <RouterView v-else />
</template>

<style scoped>
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
