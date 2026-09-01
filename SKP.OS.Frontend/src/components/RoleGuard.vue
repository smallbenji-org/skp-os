<script setup lang="ts">
import { computed } from 'vue'
import { useAuth } from '../composables/useAuth'

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

const { user, hasAnyRole, hasAllRoles } = useAuth()

const allowed = computed(() => {
  if (props.deny.length > 0 && hasAnyRole(props.deny)) {
    return false
  }
  if (props.allow.length > 0 && !hasAllRoles(props.allow)) {
    return false
  }
  if (props.any.length > 0 && !hasAnyRole(props.any)) {
    return false
  }
  return !!user.value
})
</script>

<template>
  <slot v-if="allowed" />
  <slot v-else name="fallback" />
</template>
