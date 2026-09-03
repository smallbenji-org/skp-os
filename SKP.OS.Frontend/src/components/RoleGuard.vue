<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/Stores/AuthStore'

const props = withDefaults(
  defineProps<{
    allow?: string[]
    any?: string[]
    deny?: string[]
  }>(),
  {
    allow: () => [],
    any: () => [],
    deny: () => [],
  },
)

const authStore = useAuthStore()

const allowed = computed(() => {
  if (props.deny.length > 0 && authStore.HAS_ANY_ROLE(props.deny)) {
    return false
  }
  if (props.allow.length > 0 && !authStore.HAS_ALL_ROLES(props.allow)) {
    return false
  }
  if (props.any.length > 0 && !authStore.HAS_ANY_ROLE(props.any)) {
    return false
  }
  return authStore.IS_AUTHENTICATED
})
</script>

<template>
  <slot v-if="allowed" />
  <slot v-else name="fallback" />
</template>
