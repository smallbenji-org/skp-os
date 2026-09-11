<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  IconUser,
  IconMail,
  IconSchool,
  IconClock,
  IconMapPin,
  IconFolderOpen,
  IconBriefcase,
  IconCheck,
} from "@tabler/icons-vue";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useFFEntryStore } from "@/Stores/FFEntryStore";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useCheckInStore } from "@/Stores/CheckInStore";
import { useAuthStore } from "@/Stores/AuthStore";
import ProjectStageBadge from "@/components/ProjectStageBadge.vue";
import type { CheckInDto, ProjectDto, StudentProfileDto } from "@/types";

const authStore = useAuthStore();
const studentProfileStore = useStudentProfileStore();
const ffEntryStore = useFFEntryStore();
const projectStore = useProjectStore();
const checkInStore = useCheckInStore();

const isLoading = ref(true);

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

function formatBalance(minutes: number): string {
  const isNegative = minutes < 0;
  const abs = Math.abs(minutes);
  const h = Math.floor(abs / 60);
  const m = abs % 60;
  const hLabel = h === 1 ? "time" : "timer";
  const mLabel = m === 1 ? "minut" : "minutter";
  return `${isNegative ? "-" : ""}${h} ${hLabel} og ${m} ${mLabel}`;
}

const today = new Date();
const todayISO = today.toISOString().split("T")[0];

const profile = computed<StudentProfileDto | null>(() =>
  studentProfileStore.MY_STUDENT_PROFILE,
);

const displayName = computed(
  () =>
    profile.value?.user?.name ||
    authStore.ME?.name ||
    authStore.ME?.email ||
    "Ukendt elev",
);

const displayEmail = computed(
  () => profile.value?.user?.email || authStore.ME?.email || "—",
);

const ffBalanceMinutes = computed(() =>
  ffEntryStore.FF_ENTRIES.reduce(
    (acc, entry) => acc + parseDurationToMinutes(entry.duration),
    0,
  ),
);

const ffBalanceLabel = computed(() =>
  formatBalance(ffBalanceMinutes.value),
);

const myProjects = computed<ProjectDto[]>(() =>
  projectStore.PROJECTS.filter((p) =>
    p.students.some((s) => s.id === profile.value?.id),
  ),
);

const latestOpenCheckIn = computed<CheckInDto | undefined>(() => {
  if (profile.value == null) return undefined;
  return [...checkInStore.CHECK_INS]
    .filter(
      (c) =>
        c.studentProfileId === profile.value!.id && c.checkOutTime === null,
    )
    .sort(
      (a, b) =>
        new Date(b.checkInTime).getTime() - new Date(a.checkInTime).getTime(),
    )[0];
});

const checkedOutToday = computed<CheckInDto | undefined>(() => {
  if (profile.value == null) return undefined;
  return [...checkInStore.CHECK_INS]
    .filter(
      (c) =>
        c.studentProfileId === profile.value!.id &&
        c.checkOutTime !== null &&
        c.checkOutTime!.slice(0, 10) === todayISO,
    )
    .sort(
      (a, b) =>
        new Date(b.checkOutTime!).getTime() - new Date(a.checkOutTime!).getTime(),
    )[0];
});

const status = computed(() => {
  const open = latestOpenCheckIn.value;
  if (open) {
    return {
      label: open.room?.name
        ? `Checket ind på ${open.room.name}${open.seat ? ` · Plads ${open.seat}` : ""}`
        : "Checket ind",
      ok: true,
    };
  }
  if (checkedOutToday.value) {
    return {
      label: `Checket ud kl. ${timeOfDay(checkedOutToday.value.checkOutTime!)}`,
      ok: true,
    };
  }
  return { label: "Ikke mødt i dag", ok: false };
});

function timeOfDay(timestamp: string): string {
  return new Date(timestamp).toLocaleTimeString("da-DK", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

onMounted(async () => {
  try {
    const profileData = await studentProfileStore.GET_MY_STUDENT_PROFILE();
    if (profileData) {
      await ffEntryStore.GET_FF_ENTRIES(profileData.id);
      await checkInStore.GET_CHECK_INS(profileData.id);
    }
    await projectStore.GET_PROJECTS();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Min profil</h1>
      <p class="page-subtitle">Dine oplysninger og oversigt</p>
    </div>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else>
      <div class="profile-grid">
        <div class="surface profile-card">
          <div class="profile-head">
            <div class="profile-avatar">
              <IconUser :size="28" :stroke-width="2.2" />
            </div>
            <div class="profile-head-text">
              <span class="profile-name">{{ displayName }}</span>
              <span class="profile-email">
                <IconMail :size="13" :stroke-width="2" />
                {{ displayEmail }}
              </span>
            </div>
          </div>

          <div class="divider" />

          <dl class="info-list">
            <div class="info-row">
              <dt>
                <IconSchool :size="15" :stroke-width="2" />
                Uddannelsesretning
              </dt>
              <dd>{{ profile?.studentType || "—" }}</dd>
            </div>
            <div class="info-row">
              <dt>
                <IconBriefcase :size="15" :stroke-width="2" />
                Kontrakttype
              </dt>
              <dd>{{ profile?.contractType || "—" }}</dd>
            </div>
            <div class="info-row">
              <dt>
                <IconCheck :size="15" :stroke-width="2" />
                EUX-studerende
              </dt>
              <dd>
                <span
                  :class="profile?.isEuxStudent ? 'badge-positive' : 'badge-neutral'"
                >
                  {{ profile?.isEuxStudent ? "Ja" : "Nej" }}
                </span>
              </dd>
            </div>
            <div class="info-row">
              <dt>
                <IconFolderOpen :size="15" :stroke-width="2" />
                Fuldførte hauls
              </dt>
              <dd>
                <template v-if="profile && profile.completedHauls.length > 0">
                  <span
                    v-for="haul in profile.completedHauls"
                    :key="haul"
                    class="haul-chip"
                  >
                    {{ haul }}
                  </span>
                </template>
                <span v-else class="muted">Ingen</span>
              </dd>
            </div>
          </dl>
        </div>

        <div class="stats-column">
          <div class="surface stat-card">
            <div class="stat-label">
              <IconClock :size="15" :stroke-width="2" />
              FF-saldo
            </div>
            <div class="stat-value">{{ ffBalanceLabel }}</div>
          </div>

          <div class="surface stat-card">
            <div class="stat-label">
              <IconFolderOpen :size="15" :stroke-width="2" />
              Projekter
            </div>
            <div class="stat-value">{{ myProjects.length }}</div>
          </div>

          <div class="surface stat-card">
            <div class="stat-label">
              <IconMapPin :size="15" :stroke-width="2" />
              Status i dag
            </div>
            <div class="stat-value" :class="{ 'stat-ok': status.ok }">
              {{ status.label }}
            </div>
          </div>
        </div>
      </div>

      <div class="surface projects-card">
        <div class="section-heading">Mine projekter</div>
        <template v-if="myProjects.length > 0">
          <div v-for="project in myProjects" :key="project.id" class="project-row">
            <div class="project-icon">
              <IconFolderOpen :size="17" :stroke-width="2" />
            </div>
            <div class="project-main">
              <span class="project-title">{{ project.title }}</span>
              <span class="project-desc">
                {{ project.shortDescription || "Ingen beskrivelse" }}
              </span>
            </div>
            <ProjectStageBadge :stage="project.stage" size="sm" />
          </div>
        </template>
        <p v-else class="empty-text">Du er endnu ikke tilknyttet et projekt.</p>
      </div>
    </template>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
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
}

.page-subtitle {
  font-size: 13px;
  color: #6b7280;
  margin: 0;
}

.skeleton-surface {
  height: 480px;
  background: linear-gradient(90deg, #e5eaed 25%, #eff2f4 50%, #e5eaed 75%);
  background-size: 400% 100%;
  animation: shimmer 1.4s ease infinite;
  border-radius: 10px;
}

@keyframes shimmer {
  0% {
    background-position: 100% 0;
  }
  100% {
    background-position: -100% 0;
  }
}

.profile-grid {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 16px;
  align-items: start;
}

.surface {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.profile-card {
  padding: 22px;
}

.profile-head {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;
}

.profile-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: #eff6ff;
  color: #016bff;
  flex-shrink: 0;
}

.profile-head-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.profile-name {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.3px;
}

.profile-email {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  color: #6b7280;
}

.divider {
  height: 1px;
  background: #dde1e5;
}

.info-list {
  display: flex;
  flex-direction: column;
  gap: 0;
  margin: 14px 0 0;
  padding: 0;
}

.info-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 13px 0;
  border-bottom: 1px solid #f3f4f6;
}

.info-row:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.info-row dt {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  color: #6b7280;
}

.info-row dd {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  font-size: 13px;
  font-weight: 600;
  color: #111827;
  text-align: right;
}

.haul-chip {
  display: inline-block;
  font-size: 11.5px;
  font-weight: 600;
  color: #374151;
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  padding: 2px 8px;
}

.badge-positive {
  font-size: 11.5px;
  font-weight: 600;
  color: #065f46;
  background: #ecfdf5;
  border: 1px solid #a7f3d0;
  border-radius: 4px;
  padding: 2px 8px;
}

.badge-neutral {
  font-size: 11.5px;
  font-weight: 600;
  color: #64748b;
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  padding: 2px 8px;
}

.muted {
  color: #9ca3af;
}

.stats-column {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.stat-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 16px 18px;
}

.stat-label {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
}

.stat-value {
  font-size: 15px;
  font-weight: 700;
  color: #111827;
  line-height: 1.4;
}

.stat-value.stat-ok {
  color: #15803d;
}

.projects-card {
  padding: 20px 22px;
}

.section-heading {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin-bottom: 12px;
}

.project-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
  border-bottom: 1px solid #f3f4f6;
}

.project-row:last-child {
  border-bottom: none;
}

.project-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  border-radius: 9px;
  background: #eff6ff;
  color: #016bff;
  flex-shrink: 0;
}

.project-main {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.project-title {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
}

.project-desc {
  font-size: 12.5px;
  color: #6b7280;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-text {
  font-size: 13px;
  color: #9ca3af;
  margin: 4px 0;
}

@media (max-width: 900px) {
  .profile-grid {
    grid-template-columns: 1fr;
  }
}
</style>