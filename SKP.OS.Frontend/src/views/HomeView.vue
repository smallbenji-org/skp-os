<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import {
  IconMapPin,
  IconNotebook,
  IconClockHour4,
  IconBell,
  IconCalendarClock,
} from "@tabler/icons-vue";
import { useAuthStore } from "@/Stores/AuthStore";
import { useAnnouncementStore } from "@/Stores/AnnouncementStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useFFEntryStore } from "@/Stores/FFEntryStore";

const router = useRouter();
const authStore = useAuthStore();
const announcementStore = useAnnouncementStore();
const studentProfileStore = useStudentProfileStore();
const ffEntryStore = useFFEntryStore();

const isLoading = ref(true);

const isInstructor = computed(() => authStore.HAS_ROLE("Instructor"));
const isStudent = computed(() => authStore.HAS_ROLE("Student"));
const displayName = computed(() => authStore.ME?.name?.trim() || "Velkommen");

interface QuickLink {
  name: string;
  route: string;
  description: string;
  icon: typeof IconMapPin;
}

const quickLinks = computed<QuickLink[]>(() => {
  const links: QuickLink[] = [
    { name: "Tjek ind", route: "location", description: "Mød op i lokalet", icon: IconMapPin },
    { name: "Logbog", route: "logbog", description: "Skriv om din dag", icon: IconNotebook },
    { name: "FF-timer", route: "ff", description: "Se dit timeregnskab", icon: IconClockHour4 },
    { name: "Meddelelser", route: "meddelelser", description: "Aktuelle meddelelser", icon: IconBell },
  ];
  if (isInstructor.value) {
    links.push({ name: "Instruktør", route: "underviser", description: "Elever, lokaler og meddelelser", icon: IconCalendarClock });
  }
  return links;
});

function goTo(routeName: string) {
  router.push({ name: routeName });
}

const activeAnnouncements = computed(() =>
  [...announcementStore.ACTIVE_ANNOUNCEMENTS].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  ),
);
const latestAnnouncements = computed(() => activeAnnouncements.value.slice(0, 3));

function parseDurationToMinutes(durationStr?: string | null): number {
  if (!durationStr) return 0;
  const str = durationStr.trim();
  const isNegative = str.startsWith("-");
  const clean = isNegative ? str.substring(1).trim() : str;

  const tMinMatch = clean.match(/^(\d+)t\s*(\d+)?(?:min)?$/i);
  if (tMinMatch) {
    const hours = parseInt(tMinMatch[1], 10) || 0;
    const minutes = parseInt(tMinMatch[2], 10) || 0;
    const total = hours * 60 + minutes;
    return isNegative ? -total : total;
  }

  let days = 0;
  let timePart = clean;
  if (clean.includes(".")) {
    const parts = clean.split(".");
    days = parseInt(parts[0], 10) || 0;
    timePart = parts[1] || "";
  }

  const pieces = timePart.split(":");
  const hours = parseInt(pieces[0], 10) || 0;
  const minutes = parseInt(pieces[1], 10) || 0;
  const total = days * 24 * 60 + hours * 60 + minutes;
  return isNegative ? -total : total;
}

const totalBalanceMinutes = computed(() =>
  ffEntryStore.FF_ENTRIES.reduce((acc, entry) => acc + parseDurationToMinutes(entry.duration), 0),
);

const formattedBalance = computed(() => {
  const abs = Math.abs(totalBalanceMinutes.value);
  const h = Math.floor(abs / 60);
  const m = abs % 60;
  const hLabel = h === 1 ? "time" : "timer";
  const mLabel = m === 1 ? "minut" : "minutter";
  return `${totalBalanceMinutes.value < 0 ? "-" : ""}${h} ${hLabel} og ${m} ${mLabel}`;
});

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString("da-DK", { day: "2-digit", month: "long", year: "numeric" });
}

onMounted(async () => {
  try {
    await announcementStore.GET_ANNOUNCEMENTS();
    if (isStudent.value) {
      await studentProfileStore.GET_MY_STUDENT_PROFILE();
      const profile = studentProfileStore.MY_STUDENT_PROFILE;
      if (profile) {
        await ffEntryStore.GET_FF_ENTRIES(profile.id);
      }
    }
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Forside</h1>
      <p class="page-sub">{{ displayName }}</p>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <template v-else>
      <div class="quick-grid">
        <button
          v-for="link in quickLinks"
          :key="link.route"
          type="button"
          class="quick-card"
          @click="goTo(link.route)"
        >
          <span class="quick-icon">
            <component :is="link.icon" :size="22" :stroke-width="2" />
          </span>
          <span class="quick-body">
            <span class="quick-title">{{ link.name }}</span>
            <span class="quick-desc">{{ link.description }}</span>
          </span>
        </button>
      </div>

      <div class="summary-grid">
        <div v-if="isStudent" class="surface summary">
          <h2 class="surface-title">FF-timer</h2>
          <p class="summary-value">{{ formattedBalance }}</p>
          <p class="summary-note">Nuværende saldo</p>
        </div>
        <div class="surface summary">
          <h2 class="surface-title">Aktive meddelelser</h2>
          <p class="summary-value">{{ activeAnnouncements.length }}</p>
          <p class="summary-note">Læs dem under Meddelelser</p>
        </div>
      </div>

      <div v-if="latestAnnouncements.length > 0" class="surface">
        <h2 class="surface-title">Seneste meddelelser</h2>
        <ul class="announcement-list">
          <li v-for="announcement in latestAnnouncements" :key="announcement.id" class="announcement-item">
            <span class="announcement-date">{{ formatDate(announcement.date) }}</span>
            <span class="announcement-title">{{ announcement.title }}</span>
            <p class="announcement-msg">{{ announcement.message }}</p>
          </li>
        </ul>
      </div>
    </template>
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

.page-sub {
  font-size: 15px;
  color: #6b7280;
  margin: 0;
}

.quick-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 14px;
}

.quick-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.quick-card:hover {
  border-color: #b6c2ff;
  box-shadow: 0 4px 16px rgba(37, 99, 235, 0.08);
}

.quick-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  border-radius: 10px;
  color: #2563eb;
  background: #eef2ff;
}

.quick-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.quick-title {
  font-size: 15px;
  font-weight: 600;
  color: #111827;
}

.quick-desc {
  font-size: 13px;
  color: #6b7280;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 14px;
}

.summary {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.surface-title {
  margin: 0;
  font-size: 13px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #6b7280;
}

.summary-value {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.summary-note {
  margin: 0;
  font-size: 13px;
  color: #9ca3af;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
}

.announcement-list {
  list-style: none;
  margin: 14px 0 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.announcement-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-bottom: 14px;
  border-bottom: 1px solid #f3f4f6;
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
  font-size: 15px;
  font-weight: 600;
  color: #111827;
}

.announcement-msg {
  margin: 0;
  font-size: 14px;
  color: #4b5563;
  white-space: pre-wrap;
}
</style>