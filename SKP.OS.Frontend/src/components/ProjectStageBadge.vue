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
    {{ label }}
  </span>
</template>

<style scoped>
.stage-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border-radius: 999px;
  font-weight: 600;
  white-space: nowrap;
  letter-spacing: 0.1px;
}

.stage-badge .dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  flex-shrink: 0;
}

.stage-badge.md {
  padding: 3px 9px;
  font-size: 11.5px;
}

.stage-badge.sm {
  padding: 2px 7px;
  font-size: 10.5px;
}

/* Oprettet: neutral blue/gray */
.stage-badge.stage-created {
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #e2e8f0;
}

.stage-badge.stage-created .dot {
  background: #64748b;
}

/* I gang: blue accent */
.stage-badge.stage-approved {
  background: #eff6ff;
  color: #1d4ed8;
  border: 1px solid #dbeafe;
}

.stage-badge.stage-approved .dot {
  background: #2563eb;
}

/* Afventer evaluering: warm amber */
.stage-badge.stage-submitted {
  background: #fffbeb;
  color: #b45309;
  border: 1px solid #fef3c7;
}

.stage-badge.stage-submitted .dot {
  background: #f59e0b;
}

/* Afsluttet: muted emerald/green */
.stage-badge.stage-evaluated {
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #dcfce7;
}

.stage-badge.stage-evaluated .dot {
  background: #16a34a;
}
</style>