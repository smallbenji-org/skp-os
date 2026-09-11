<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useAnnouncementStore } from "@/Stores/AnnouncementStore";

const announcementStore = useAnnouncementStore();

const isLoading = ref(true);

const activeAnnouncements = computed(() =>
  [...announcementStore.ACTIVE_ANNOUNCEMENTS].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  ),
);

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString("da-DK", { day: "2-digit", month: "long", year: "numeric" });
}

onMounted(async () => {
  try {
    await announcementStore.GET_ANNOUNCEMENTS();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Aktuelle Meddelelser</h1>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="activeAnnouncements.length === 0" class="surface empty">
      <h2 class="empty-title">Ingen meddelelser lige nu</h2>
      <p class="empty-text">Der er ikke nogen aktive meddelelser. Kig forbi igen senere.</p>
    </div>

    <div v-else class="surface">
      <ul class="announcement-list">
        <li v-for="announcement in activeAnnouncements" :key="announcement.id" class="announcement-item">
          <span class="announcement-date">{{ formatDate(announcement.date) }}</span>
          <h2 class="announcement-title">{{ announcement.title }}</h2>
          <p class="announcement-msg">{{ announcement.message }}</p>
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

.announcement-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0;
}

.announcement-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 20px 0;
  border-bottom: 1px solid #e5e7eb;
}

.announcement-item:first-child {
  padding-top: 0;
}

.announcement-item:last-child {
  padding-bottom: 0;
  border-bottom: none;
}

.announcement-date {
  font-size: 12px;
  font-weight: 600;
  color: #2563eb;
}

.announcement-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.announcement-msg {
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