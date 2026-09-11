<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { IconPin } from "@tabler/icons-vue";
import { useInfoEntryStore } from "@/Stores/InfoEntryStore";

const infoEntryStore = useInfoEntryStore();

const isLoading = ref(true);

const infoEntries = computed(() =>
  [...infoEntryStore.INFO_ENTRIES].sort((a, b) => {
    if (a.isPinned !== b.isPinned) return a.isPinned ? -1 : 1;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  }),
);

function formatDateTime(dateStr?: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString("da-DK", { day: "2-digit", month: "short", year: "numeric" });
}

onMounted(async () => {
  try {
    await infoEntryStore.GET_INFO_ENTRIES();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Information</h1>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="infoEntries.length === 0" class="surface empty">
      <h2 class="empty-title">Ingen opslag endnu</h2>
      <p class="empty-text">Der er ikke lagt nogen information op lige nu.</p>
    </div>

    <div v-else class="surface">
      <ul class="info-list">
        <li
          v-for="entry in infoEntries"
          :key="entry.id"
          class="info-item"
          :class="{ pinned: entry.isPinned }"
        >
          <div class="info-heading">
            <span v-if="entry.isPinned" class="pinned-badge">
              <IconPin :size="14" :stroke-width="2.5" />
              Fastgjort
            </span>
            <span class="info-date">{{ formatDateTime(entry.createdAt) }}</span>
          </div>
          <h2 class="info-title">{{ entry.title }}</h2>
          <p class="info-content">{{ entry.content }}</p>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.5px;
  margin: 0;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
}

.info-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 16px 0;
  border-bottom: 1px solid #f3f4f6;
}

.info-item:first-child {
  padding-top: 0;
}

.info-item:last-child {
  padding-bottom: 0;
  border-bottom: none;
}

.info-heading {
  display: flex;
  align-items: center;
  gap: 10px;
}

.pinned-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 600;
  color: #b45309;
  background: #fffbeb;
  border: 1px solid #fde68a;
  padding: 2px 8px;
  border-radius: 999px;
}

.info-date {
  font-size: 12px;
  font-weight: 600;
  color: #6b7280;
}

.info-title {
  margin: 4px 0 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.info-content {
  margin: 0;
  font-size: 14px;
  line-height: 1.55;
  color: #4b5563;
  white-space: pre-wrap;
}

.empty {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.empty-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.empty-text {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
}
</style>