<script setup lang="ts">
import { computed } from "vue";
import type { ProjectStage } from "@/types";
import { stageClass, stageLabel } from "@/utils/project-stage";

const props = defineProps<{
  stage: ProjectStage | null | undefined;
  size?: "sm" | "md";
}>();

const label = computed(() => stageLabel(props.stage));
const cls = computed(() => stageClass(props.stage));
</script>

<template>
  <span class="stage-badge" :class="[cls, size ?? 'md']">
    <span class="dot" />
    {{ label }}
  </span>
</template>

<style scoped>
.stage-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border-radius: 999px;
  font-weight: 700;
  white-space: nowrap;
}

.stage-badge .dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.stage-badge.md {
  padding: 4px 10px;
  font-size: 11.5px;
}

.stage-badge.sm {
  padding: 2px 8px;
  font-size: 10.5px;
}

.stage-badge.stage-created {
  background: #f1f5f9;
  color: #475569;
}

.stage-badge.stage-created .dot {
  background: #94a3b8;
}

.stage-badge.stage-approved {
  background: #eff6ff;
  color: #1d4ed8;
}

.stage-badge.stage-approved .dot {
  background: #3b82f6;
}

.stage-badge.stage-submitted {
  background: #fffbeb;
  color: #b45309;
}

.stage-badge.stage-submitted .dot {
  background: #f59e0b;
}

.stage-badge.stage-evaluated {
  background: #ecfdf5;
  color: #065f46;
}

.stage-badge.stage-evaluated .dot {
  background: #10b981;
}
</style>