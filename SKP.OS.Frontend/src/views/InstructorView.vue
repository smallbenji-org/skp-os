<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  IconUsers,
  IconBuilding,
  IconFolderPlus,
  IconNotes,
  IconClock,
  IconPlus,
  IconTrash,
  IconAlertTriangle,
  IconRefresh,
  IconCheck,
  IconUserCheck,
  IconUsersGroup,
  IconLock,
  IconLockOpen,
  IconX,
} from "@tabler/icons-vue";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useCheckInStore } from "@/Stores/CheckInStore";
import { useFFEntryStore } from "@/Stores/FFEntryStore";
import { useRoomStore } from "@/Stores/RoomStore";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useProjectTemplateStore } from "@/Stores/ProjectTemplateStore";
import { useInstructorProfileStore } from "@/Stores/InstructorProfileStore";
import type { CheckInDto, FFEntryDto, ProjectHaul, StudentProfileDto, StudentType } from "@/types";

const studentProfileStore = useStudentProfileStore();
const checkInStore = useCheckInStore();
const ffEntryStore = useFFEntryStore();
const roomStore = useRoomStore();
const projectStore = useProjectStore();
const projectTemplateStore = useProjectTemplateStore();
const instructorProfileStore = useInstructorProfileStore();

const SCHOOL_START_MINUTES = 8 * 60;
const LATE_GRACE_MINUTES = 15;
const LATE_THRESHOLD_MINUTES = SCHOOL_START_MINUTES + LATE_GRACE_MINUTES;

const HAULS: ProjectHaul[] = ["Hf1", "Hf2", "Hf3"];
const STUDENT_TYPES: StudentType[] = [
  "Programmer",
  "Infrastructure",
  "ItSupporter",
  "Cybersecurity",
];

const today = new Date();
const todayISO = today.toISOString().split("T")[0];

const isLoading = ref(true);
const activeTab = ref<"students" | "rooms" | "projects" | "templates">("students");

const summary = computed(() => {
  const studentsList = students.value;
  const present = studentsList.filter((s) => statusFor(s).key === "present").length;
  const late = studentsList.filter((s) => statusFor(s).late).length;
  const checkedOut = studentsList.filter((s) => statusFor(s).key === "checked-out").length;
  const absent = studentsList.filter((s) => statusFor(s).key === "absent").length;
  return { present, late, checkedOut, absent };
});

const students = computed(() =>
  [...studentProfileStore.STUDENT_PROFILES].sort((a, b) =>
    (a.user?.name ?? "").localeCompare(b.user?.name ?? "", "da"),
  ),
);

function studentName(student: StudentProfileDto): string {
  return student.user?.name ?? `Elev #${student.id}`;
}

function isTodayAt(timestamp: string): boolean {
  return timestamp.slice(0, 10) === todayISO;
}

function timeOfDay(timestamp: string): string {
  return new Date(timestamp).toLocaleTimeString("da-DK", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function latestOpenCheckIn(studentId: number): CheckInDto | undefined {
  return checkInStore.CHECK_INS.filter(
    (c) => c.studentProfileId === studentId && c.checkOutTime === null,
  ).sort(
    (a, b) => new Date(b.checkInTime).getTime() - new Date(a.checkInTime).getTime(),
  )[0];
}

function isLate(checkIn: CheckInDto): boolean {
  const d = new Date(checkIn.checkInTime);
  return d.getHours() * 60 + d.getMinutes() > LATE_THRESHOLD_MINUTES;
}

function statusFor(student: StudentProfileDto) {
  const open = latestOpenCheckIn(student.id);
  if (open) {
    return {
      key: "present" as const,
      late: isTodayAt(open.checkInTime) && isLate(open),
      label: `${open.room?.name ?? "Ukendt lokation"}${open.seat ? ` · Plads ${open.seat}` : ""}`,
      since: open.checkInTime,
    };
  }
  const checkedOutToday = checkInStore.CHECK_INS.filter(
    (c) =>
      c.studentProfileId === student.id &&
      c.checkOutTime !== null &&
      isTodayAt(c.checkOutTime!),
  ).sort(
    (a, b) =>
      new Date(b.checkOutTime!).getTime() - new Date(a.checkOutTime!).getTime(),
  );
  if (checkedOutToday.length > 0) {
    return {
      key: "checked-out" as const,
      late: false,
      label: `Checket ud kl. ${timeOfDay(checkedOutToday[0].checkOutTime!)}`,
      since: null,
    };
  }
  return { key: "absent" as const, late: false, label: "Ikke mødt i dag", since: null };
}

function parseDurationToSeconds(duration: string): number {
  const parts = duration.split(":").map((n) => parseInt(n, 10) || 0);
  return parts[0] * 3600 + (parts[1] ?? 0) * 60 + (parts[2] ?? 0);
}

function hoursToDuration(hours: number): string {
  const totalSeconds = Math.round(hours * 3600);
  const sign = totalSeconds < 0 ? "-" : "";
  const s = Math.abs(totalSeconds);
  const hh = Math.floor(s / 3600);
  const mm = Math.floor((s % 3600) / 60);
  const ss = s % 60;
  return `${sign}${[hh, mm, ss].map((n) => String(n).padStart(2, "0")).join(":")}`;
}

function formatClock(total: number): string {
  const negative = total < 0;
  const s = Math.abs(Math.floor(total));
  const hh = Math.floor(s / 3600);
  const mm = Math.floor((s % 3600) / 60);
  const ss = s % 60;
  const base =
    s >= 3600
      ? `${hh}t ${String(mm).padStart(2, "0")}m ${String(ss).padStart(2, "0")}s`
      : `${mm}m ${String(ss).padStart(2, "0")}s`;
  return negative ? `-${base}` : base;
}

function ffEntriesFor(studentId: number): FFEntryDto[] {
  return ffEntryStore.FF_ENTRIES.filter((e) => e.studentProfileId === studentId);
}

function totalSecondsFor(studentId: number): number {
  return ffEntriesFor(studentId).reduce(
    (acc, e) => acc + parseDurationToSeconds(e.duration),
    0,
  );
}

const selectedStudentId = ref<number | null>(null);
const selectedStudent = computed(
  () => students.value.find((s) => s.id === selectedStudentId.value) ?? null,
);

function selectStudent(studentId: number) {
  selectedStudentId.value =
    selectedStudentId.value === studentId ? null : studentId;
  grantFeedback.value = "idle";
  grantNote.value = "";
}

const grantMode = ref<"add" | "deduct">("add");
const grantHours = ref(1);
const grantNote = ref("");
const isGranting = ref(false);
const grantFeedback = ref<"idle" | "success" | "error">("idle");
const grantError = ref("");

async function grantFFHours() {
  const studentId = selectedStudentId.value;
  if (studentId == null || isGranting.value) return;
  const input = Number(grantHours.value);
  if (!Number.isFinite(input) || input <= 0) {
    grantFeedback.value = "error";
    grantError.value = "Angiv et antal timer større end 0.";
    return;
  }
  const hours = grantMode.value === "deduct" ? -input : input;
  isGranting.value = true;
  grantFeedback.value = "idle";
  grantError.value = "";
  const result = await ffEntryStore.CREATE_FF_ENTRY({
    date: todayISO,
    duration: hoursToDuration(hours),
    note: grantNote.value.trim(),
    studentProfileId: studentId,
  });
  isGranting.value = false;
  grantFeedback.value = result ? "success" : "error";
  if (result) {
    grantHours.value = 1;
  } else {
    grantError.value = grantMode.value === "deduct"
      ? "Kunne ikke fratrække FF timer."
      : "Kunne ikke tildele FF timer.";
  }
  window.setTimeout(() => {
    grantFeedback.value = "idle";
  }, 3000);
}

const isTogglingBlock = ref<number | null>(null);

async function toggleCheckInBlock(student: StudentProfileDto) {
  if (isTogglingBlock.value != null) return;
  isTogglingBlock.value = student.id;
  await studentProfileStore.UPDATE_STUDENT_PROFILE(student.id, {
    studentType: student.studentType,
    contractType: student.contractType,
    isEuxStudent: student.isEuxStudent,
    isCheckInBlocked: !student.isCheckInBlocked,
    completedHauls: student.completedHauls,
  });
  isTogglingBlock.value = null;
}

const selectedEntries = computed(() => {
  if (selectedStudentId.value == null) return [] as FFEntryDto[];
  return [...ffEntriesFor(selectedStudentId.value)].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  );
});

const selectedFFTotal = computed(() =>
  selectedStudentId.value != null ? totalSecondsFor(selectedStudentId.value) : 0,
);

const selectedProjects = computed(() => {
  if (selectedStudentId.value == null) return [];
  return projects.value.filter((p) =>
    p.students.some((s) => s.id === selectedStudentId.value),
  );
});

async function deleteFFEntry(id: number) {
  await ffEntryStore.DELETE_FF_ENTRY(id);
}

const newRoomName = ref("");
const newRoomLocation = ref("");
const isSavingRoom = ref(false);
const roomFeedback = ref<"idle" | "success" | "error">("idle");
const roomError = ref("");

async function createRoom() {
  if (!newRoomName.value.trim() || isSavingRoom.value) return;
  isSavingRoom.value = true;
  roomFeedback.value = "idle";
  roomError.value = "";
  const result = await roomStore.CREATE_ROOM({
    name: newRoomName.value.trim(),
    location: newRoomLocation.value.trim(),
  });
  isSavingRoom.value = false;
  roomFeedback.value = result ? "success" : "error";
  if (result) {
    newRoomName.value = "";
    newRoomLocation.value = "";
  } else {
    roomError.value = "Kunne ikke oprette lokationen.";
  }
  window.setTimeout(() => {
    roomFeedback.value = "idle";
  }, 3000);
}

async function deleteRoom(id: number) {
  await roomStore.DELETE_ROOM(id);
}

function presentInRoom(roomId: number): number {
  return checkInStore.CHECK_INS.filter(
    (c) => c.roomId === roomId && c.checkOutTime === null,
  ).length;
}

const newProjectTitle = ref("");
const newProjectDescription = ref("");
const newProjectRepo = ref("");
const isCustomProject = ref(false);
const newProjectTemplateId = ref<number | null>(null);
const isSavingProject = ref(false);
const projectFeedback = ref<"idle" | "success" | "error">("idle");
const projectError = ref("");

const projects = computed(() =>
  [...projectStore.PROJECTS].sort((a, b) => a.title.localeCompare(b.title, "da")),
);

async function createProject() {
  if (!newProjectTitle.value.trim() || isSavingProject.value) return;
  if (!isCustomProject.value && newProjectTemplateId.value == null) {
    projectFeedback.value = "error";
    projectError.value = "Vælg en skabelon til projektet.";
    return;
  }
  isSavingProject.value = true;
  projectFeedback.value = "idle";
  projectError.value = "";
  const result = await projectStore.CREATE_PROJECT({
    title: newProjectTitle.value.trim(),
    shortDescription: newProjectDescription.value.trim(),
    gitRepoUrl: newProjectRepo.value.trim() || "",
    isCustomProject: isCustomProject.value,
    projectTemplateId: isCustomProject.value ? null : newProjectTemplateId.value,
  });
  isSavingProject.value = false;
  projectFeedback.value = result ? "success" : "error";
  if (result) {
    newProjectTitle.value = "";
    newProjectDescription.value = "";
    newProjectRepo.value = "";
    isCustomProject.value = false;
    newProjectTemplateId.value = null;
  } else {
    projectError.value = "Kunne ikke oprette projektet.";
  }
  window.setTimeout(() => {
    projectFeedback.value = "idle";
  }, 3000);
}

const assignSelect = ref<Record<number, number | null>>({});

function availableStudents(projectId: number): StudentProfileDto[] {
  const project = projects.value.find((p) => p.id === projectId);
  const assigned = new Set(project?.students.map((s) => s.id) ?? []);
  return students.value.filter((s) => !assigned.has(s.id));
}

async function assignStudent(projectId: number) {
  const studentId = assignSelect.value[projectId];
  if (studentId == null) return;
  const updated = await projectStore.ADD_STUDENT(projectId, studentId);
  if (updated) {
    assignSelect.value = { ...assignSelect.value, [projectId]: null };
    await refreshProjects();
  }
}

async function unassignStudent(projectId: number, studentId: number) {
  const updated = await projectStore.REMOVE_STUDENT(projectId, studentId);
  if (updated) {
    await refreshProjects();
  }
}

async function refreshProjects() {
  await projectStore.GET_PROJECTS();
}

const templates = computed(() =>
  [...projectTemplateStore.PROJECT_TEMPLATES].sort((a, b) =>
    a.title.localeCompare(b.title, "da"),
  ),
);

const templateInstructorProfile = computed(
  () => instructorProfileStore.MY_INSTRUCTOR_PROFILE,
);

const newTemplateTitle = ref("");
const newTemplateDescription = ref("");
const newTemplateRepo = ref("");
const newTemplateHaul = ref<ProjectHaul>("Hf1");
const newTemplateStudentType = ref<StudentType>("Programmer");
const isSavingTemplate = ref(false);
const templateFeedback = ref<"idle" | "success" | "error">("idle");
const templateError = ref("");

async function createTemplate() {
  if (!newTemplateTitle.value.trim() || isSavingTemplate.value) return;
  const profile = templateInstructorProfile.value;
  if (!profile) {
    templateFeedback.value = "error";
    templateError.value = "Der er ikke fundet en underviserprofil for din bruger.";
    return;
  }
  isSavingTemplate.value = true;
  templateFeedback.value = "idle";
  templateError.value = "";
  const result = await projectTemplateStore.CREATE_PROJECT_TEMPLATE({
    title: newTemplateTitle.value.trim(),
    shortDescription: newTemplateDescription.value.trim(),
    gitRepoUrl: newTemplateRepo.value.trim() || "",
    haul: newTemplateHaul.value,
    studentType: newTemplateStudentType.value,
    instructorProfileId: profile.id,
  });
  isSavingTemplate.value = false;
  templateFeedback.value = result ? "success" : "error";
  if (result) {
    newTemplateTitle.value = "";
    newTemplateDescription.value = "";
    newTemplateRepo.value = "";
    newTemplateHaul.value = "Hf1";
    newTemplateStudentType.value = "Programmer";
  } else {
    templateError.value = "Kunne ikke oprette skabelonen.";
  }
  window.setTimeout(() => {
    templateFeedback.value = "idle";
  }, 3000);
}

async function deleteTemplate(id: number) {
  await projectTemplateStore.DELETE_PROJECT_TEMPLATE(id);
}

async function reloadAll() {
  await Promise.all([
    studentProfileStore.GET_STUDENT_PROFILES(),
    checkInStore.GET_CHECK_INS(),
    ffEntryStore.GET_FF_ENTRIES(),
    roomStore.GET_ROOMS(),
    refreshProjects(),
    projectTemplateStore.GET_PROJECT_TEMPLATES(),
    instructorProfileStore.GET_MY_INSTRUCTOR_PROFILE(),
  ]);
}

onMounted(async () => {
  await reloadAll();
  isLoading.value = false;
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Underviser</h1>
      <div class="page-header-row">
        <p class="page-subtitle">Overblik over elever, lokationer og projekter</p>
        <button class="refresh-btn" @click="reloadAll">
          <IconRefresh :size="15" :stroke-width="2" />
          Opdater
        </button>
      </div>
    </div>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else>
      <div class="surface">
        <nav class="tab-bar" aria-label="Underviser sektioner">
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'students' }"
            @click="activeTab = 'students'"
          >
            <IconUsers :size="16" :stroke-width="2" />
            Elevoversigt
          </button>
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'rooms' }"
            @click="activeTab = 'rooms'"
          >
            <IconBuilding :size="16" :stroke-width="2" />
            Lokationer
          </button>
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'projects' }"
            @click="activeTab = 'projects'"
          >
            <IconFolderPlus :size="16" :stroke-width="2" />
            SKP Projekter
          </button>
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'templates' }"
            @click="activeTab = 'templates'"
          >
            <IconNotes :size="16" :stroke-width="2" />
            Skabeloner
          </button>
        </nav>

        <section v-if="activeTab === 'students'" class="tab-panel">
          <div class="summary-row">
            <div class="summary-chip present">
              <IconUserCheck :size="15" :stroke-width="2" />
              <span class="summary-num">{{ summary.present }}</span>
              <span>til stede</span>
            </div>
            <div class="summary-chip late">
              <IconAlertTriangle :size="15" :stroke-width="2" />
              <span class="summary-num">{{ summary.late }}</span>
              <span>for sent</span>
            </div>
            <div class="summary-chip checked-out">
              <IconClock :size="15" :stroke-width="2" />
              <span class="summary-num">{{ summary.checkedOut }}</span>
              <span>checket ud</span>
            </div>
            <div class="summary-chip absent">
              <IconUsersGroup :size="15" :stroke-width="2" />
              <span class="summary-num">{{ summary.absent }}</span>
              <span>ikke mødt</span>
            </div>
          </div>

          <div class="section-heading">Elever</div>
          <table class="data-table" aria-label="Elevoversigt">
            <thead>
              <tr>
                <th>Elev</th>
                <th>Status</th>
                <th>FF i alt</th>
                <th>Tjek ind</th>
                <th class="col-actions">Tildel FF</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="students.length === 0">
                <td colspan="5" class="empty-row">Ingen elever fundet.</td>
              </tr>
              <tr
                v-for="student in students"
                :key="student.id"
                class="table-row"
                :class="{ 'row-selected': selectedStudentId === student.id }"
                @click="selectStudent(student.id)"
              >
                <td class="cell-name">
                  {{ studentName(student) }}
                </td>
                <td class="cell-status">
                  <span
                    class="status-pill"
                    :class="{
                      present: statusFor(student).key === 'present',
                      'checked-out': statusFor(student).key === 'checked-out',
                      absent: statusFor(student).key === 'absent',
                    }"
                  >
                    <span class="dot" />
                    {{ statusFor(student).label }}
                  </span>
                  <span v-if="statusFor(student).late" class="late-badge">
                    For sent
                  </span>
                </td>
                <td class="cell-ff" :class="{ neg: totalSecondsFor(student.id) < 0 }">
                  {{ formatClock(totalSecondsFor(student.id)) }}
                </td>
                <td class="cell-actions" @click.stop>
                  <button
                    class="block-btn"
                    :class="{ blocked: student.isCheckInBlocked }"
                    :disabled="isTogglingBlock === student.id"
                    @click="toggleCheckInBlock(student)"
                    :title="student.isCheckInBlocked ? 'Fjern blokering af tjek ind' : 'Bloker tjek ind'"
                  >
                    <IconLock
                      v-if="student.isCheckInBlocked"
                      :size="13"
                      :stroke-width="2.2"
                    />
                    <IconLockOpen v-else :size="13" :stroke-width="2.2" />
                    {{ student.isCheckInBlocked ? "Blokeret" : "Aktiv" }}
                  </button>
                </td>
                <td class="cell-actions" @click.stop>
                  <button class="grant-btn" @click="selectStudent(student.id)">
                    <IconClock :size="14" :stroke-width="2.2" />
                    Tildel FF
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <Transition name="details-fade">
            <div v-if="selectedStudent" class="details-panel">
              <div class="details-header">
                <div class="details-title">
                  {{ studentName(selectedStudent) }}
                  <span class="details-meta">
                    <IconClock :size="14" :stroke-width="2" />
                    FF i alt: {{ formatClock(selectedFFTotal) }}
                  </span>
                  <span class="details-meta">
                    <IconFolderPlus :size="14" :stroke-width="2" />
                    Projekter: {{ selectedProjects.map((p) => p.title).join(", ") || "Ingen" }}
                  </span>
                </div>
                <button
                  class="icon-btn"
                  @click="selectedStudentId = null"
                  aria-label="Luk detaljer"
                >
                  <IconX :size="16" :stroke-width="2.2" />
                </button>
              </div>

              <div class="grant-form">
                <div class="grant-mode" role="group" aria-label="FF type">
                  <button
                    class="mode-btn"
                    :class="{ active: grantMode === 'add' }"
                    type="button"
                    @click="grantMode = 'add'"
                  >
                    Tildel
                  </button>
                  <button
                    class="mode-btn deduct"
                    :class="{ active: grantMode === 'deduct' }"
                    type="button"
                    @click="grantMode = 'deduct'"
                  >
                    Fratræk
                  </button>
                </div>
                <div class="form-field">
                  <label class="form-label" for="grant-hours">Timer</label>
                  <input
                    id="grant-hours"
                    v-model="grantHours"
                    class="form-input grant-input"
                    type="number"
                    min="0.25"
                    step="0.25"
                  />
                </div>
                <div class="form-field">
                  <label class="form-label" for="grant-note">Note</label>
                  <input
                    id="grant-note"
                    v-model="grantNote"
                    class="form-input"
                    type="text"
                    placeholder="fx frihed for godt arbejde"
                  />
                </div>
                <button
                  class="create-btn"
                  :class="{ deduct: grantMode === 'deduct' }"
                  type="button"
                  :disabled="isGranting"
                  @click="grantFFHours"
                >
                  <IconPlus :size="15" :stroke-width="2.5" />
                  {{ grantMode === "deduct" ? "Fratræk FF timer" : "Tildel FF timer" }}
                </button>
                <span
                  v-if="grantFeedback === 'success'"
                  class="save-feedback success"
                >
                  <IconCheck :size="13" :stroke-width="2.5" />
                  {{ grantMode === "deduct" ? "Fratrukket" : "Tildelt" }}
                </span>
                <span
                  v-else-if="grantFeedback === 'error'"
                  class="save-feedback error"
                >
                  <IconAlertTriangle :size="13" :stroke-width="2" />
                  {{ grantError }}
                </span>
              </div>

              <table class="data-table inner" aria-label="FF registreringer">
                <thead>
                  <tr>
                    <th>Dato</th>
                    <th>Underviser</th>
                    <th>Varighed</th>
                    <th>Note</th>
                    <th class="col-actions"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="selectedEntries.length === 0">
                    <td colspan="5" class="empty-row">Ingen FF registreringer.</td>
                  </tr>
                  <tr v-for="entry in selectedEntries" :key="entry.id">
                    <td class="cell-date">
                      {{ new Date(entry.date).toLocaleDateString("da-DK") }}
                    </td>
                    <td class="cell-note">{{ entry.instructorName || "—" }}</td>
                    <td class="cell-ff" :class="{ neg: parseDurationToSeconds(entry.duration) < 0 }">{{ formatClock(parseDurationToSeconds(entry.duration)) }}</td>
                    <td class="cell-note">{{ entry.note || "—" }}</td>
                    <td class="cell-actions">
                      <button
                        class="icon-btn danger"
                        @click="deleteFFEntry(entry.id)"
                        aria-label="Slet FF registrering"
                      >
                        <IconTrash :size="15" :stroke-width="2" />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </Transition>
        </section>

        <section v-else-if="activeTab === 'rooms'" class="tab-panel">
          <div class="section-heading">Opret lokation</div>
          <form class="create-form" @submit.prevent="createRoom">
            <div class="form-field">
              <label class="form-label" for="room-name">Navn</label>
              <input
                id="room-name"
                v-model="newRoomName"
                class="form-input"
                type="text"
                placeholder="fx Lokale 12"
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="room-location">Placering</label>
              <input
                id="room-location"
                v-model="newRoomLocation"
                class="form-input"
                type="text"
                placeholder="fx 1. sal, vest"
              />
            </div>
            <button
              class="create-btn"
              type="submit"
              :disabled="!newRoomName.trim() || isSavingRoom"
            >
              <IconPlus :size="16" :stroke-width="2.5" />
              Opret lokation
            </button>
            <span
              v-if="roomFeedback === 'success'"
              class="save-feedback success"
            >
              <IconCheck :size="13" :stroke-width="2.5" /> Oprettet
            </span>
            <span
              v-else-if="roomFeedback === 'error'"
              class="save-feedback error"
            >
              <IconAlertTriangle :size="13" :stroke-width="2" />
              {{ roomError }}
            </span>
          </form>

          <div class="divider" />

          <div class="section-heading">Eksisterende lokationer</div>
          <table class="data-table" aria-label="Lokationer">
            <thead>
              <tr>
                <th>Navn</th>
                <th>Placering</th>
                <th>Til stede</th>
                <th class="col-actions"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="roomStore.ROOMS.length === 0">
                <td colspan="4" class="empty-row">Ingen lokationer endnu.</td>
              </tr>
              <tr v-for="room in roomStore.ROOMS" :key="room.id">
                <td class="cell-name">{{ room.name }}</td>
                <td>{{ room.location || "—" }}</td>
                <td>
                  <span class="ff-total">{{ presentInRoom(room.id) }}</span>
                </td>
                <td class="cell-actions">
                  <button
                    class="icon-btn danger"
                    @click="deleteRoom(room.id)"
                    aria-label="Slet lokation"
                  >
                    <IconTrash :size="15" :stroke-width="2" />
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <section v-else-if="activeTab === 'projects'" class="tab-panel">
          <div class="section-heading">Opret SKP projekt</div>
          <form class="create-form" @submit.prevent="createProject">
            <div class="form-field">
              <label class="form-label" for="project-title">Titel</label>
              <input
                id="project-title"
                v-model="newProjectTitle"
                class="form-input"
                type="text"
                placeholder="fx Byg en salesanalyse"
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="project-description">Kort beskrivelse</label>
              <input
                id="project-description"
                v-model="newProjectDescription"
                class="form-input"
                type="text"
                placeholder="Hvad handler projektet om?"
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="project-repo">Git repository URL</label>
              <input
                id="project-repo"
                v-model="newProjectRepo"
                class="form-input"
                type="text"
                placeholder="https://github.com/..."
              />
            </div>
            <div class="form-field form-check">
              <label class="checkbox-label" for="project-custom">
                <span
                  class="custom-checkbox"
                  :class="{ checked: isCustomProject }"
                  role="checkbox"
                  :aria-checked="isCustomProject"
                  @click="isCustomProject = !isCustomProject"
                  @keydown.space.prevent="isCustomProject = !isCustomProject"
                  @keydown.enter.prevent="isCustomProject = !isCustomProject"
                >
                  <IconCheck v-if="isCustomProject" :size="11" :stroke-width="3" />
                </span>
                <input
                  id="project-custom"
                  v-model="isCustomProject"
                  type="checkbox"
                  class="sr-only"
                />
                <span class="checkbox-text">Eget projekt uden skabelon</span>
              </label>
            </div>
            <div v-if="!isCustomProject" class="form-field">
              <label class="form-label" for="project-template">Skabelon</label>
              <select
                id="project-template"
                v-model="newProjectTemplateId"
                class="form-select"
              >
                <option :value="null" disabled>Vælg skabelon…</option>
                <option
                  v-for="template in projectTemplateStore.PROJECT_TEMPLATES"
                  :key="template.id"
                  :value="template.id"
                >
                  {{ template.title }}
                </option>
              </select>
            </div>
            <button
              class="create-btn"
              type="submit"
              :disabled="!newProjectTitle.trim() || isSavingProject"
            >
              <IconPlus :size="16" :stroke-width="2.5" />
              Opret projekt
            </button>
            <span
              v-if="projectFeedback === 'success'"
              class="save-feedback success"
            >
              <IconCheck :size="13" :stroke-width="2.5" /> Oprettet
            </span>
            <span
              v-else-if="projectFeedback === 'error'"
              class="save-feedback error"
            >
              <IconAlertTriangle :size="13" :stroke-width="2" />
              {{ projectError }}
            </span>
          </form>

          <div class="divider" />

          <div class="section-heading">Eksisterende projekter</div>
          <div v-if="projects.length === 0" class="empty-card">Ingen projekter endnu.</div>
          <article
            v-for="project in projects"
            :key="project.id"
            class="project-card"
          >
            <div class="project-head">
              <div class="project-title">{{ project.title }}</div>
              <span class="project-tag" :class="{ custom: project.isCustomProject }">
                {{ project.isCustomProject ? "Eget" : project.projectTemplate?.title ?? "Skabelon" }}
              </span>
            </div>
            <p class="project-desc">
              {{ project.shortDescription || "Ingen beskrivelse." }}
            </p>

            <div class="assign-area">
              <div class="member-list">
                <span v-if="project.students.length === 0" class="muted">
                  Ingen elever tilknyttet
                </span>
                <span
                  v-for="member in project.students"
                  :key="member.id"
                  class="member-chip"
                >
                  {{ studentName(member) }}
                  <button
                    class="chip-remove"
                    @click="unassignStudent(project.id, member.id)"
                    aria-label="Fjern elev fra projekt"
                  >
                    <IconX :size="12" :stroke-width="2.5" />
                  </button>
                </span>
              </div>

              <div class="assign-form">
                <select
                  v-model="assignSelect[project.id]"
                  class="form-select small"
                  aria-label="Vælg elev"
                >
                  <option :value="null" disabled>Tilføj elev…</option>
                  <option
                    v-for="student in availableStudents(project.id)"
                    :key="student.id"
                    :value="student.id"
                  >
                    {{ studentName(student) }}
                  </option>
                </select>
                <button
                  class="assign-btn"
                  :disabled="assignSelect[project.id] == null"
                  @click="assignStudent(project.id)"
                >
                  <IconPlus :size="14" :stroke-width="2.5" />
                  Tilføj
                </button>
              </div>
            </div>
          </article>
        </section>

        <section v-else class="tab-panel">
          <div class="section-heading">Opret skabelon</div>
          <form class="create-form" @submit.prevent="createTemplate">
            <div class="form-field">
              <label class="form-label" for="template-title">Titel</label>
              <input
                id="template-title"
                v-model="newTemplateTitle"
                class="form-input"
                type="text"
                placeholder="fx Byg en webshop"
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="template-description">Kort beskrivelse</label>
              <input
                id="template-description"
                v-model="newTemplateDescription"
                class="form-input"
                type="text"
                placeholder="Hvad handler skabelonen om?"
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="template-repo">Git repository URL</label>
              <input
                id="template-repo"
                v-model="newTemplateRepo"
                class="form-input"
                type="text"
                placeholder="https://github.com/..."
              />
            </div>
            <div class="form-field">
              <label class="form-label" for="template-haul">Forløb</label>
              <select
                id="template-haul"
                v-model="newTemplateHaul"
                class="form-select"
              >
                <option v-for="haul in HAULS" :key="haul" :value="haul">
                  {{ haul }}
                </option>
              </select>
            </div>
            <div class="form-field">
              <label class="form-label" for="template-type">Elevtype</label>
              <select
                id="template-type"
                v-model="newTemplateStudentType"
                class="form-select"
              >
                <option v-for="type in STUDENT_TYPES" :key="type" :value="type">
                  {{ type }}
                </option>
              </select>
            </div>
            <button
              class="create-btn"
              type="submit"
              :disabled="!newTemplateTitle.trim() || isSavingTemplate"
            >
              <IconPlus :size="16" :stroke-width="2.5" />
              Opret skabelon
            </button>
            <span
              v-if="templateFeedback === 'success'"
              class="save-feedback success"
            >
              <IconCheck :size="13" :stroke-width="2.5" /> Oprettet
            </span>
            <span
              v-else-if="templateFeedback === 'error'"
              class="save-feedback error"
            >
              <IconAlertTriangle :size="13" :stroke-width="2" />
              {{ templateError }}
            </span>
          </form>

          <div class="divider" />

          <div class="section-heading">Eksisterende skabeloner</div>
          <div v-if="templates.length === 0" class="empty-card">Ingen skabeloner endnu.</div>
          <table class="data-table" aria-label="Skabeloner">
            <thead>
              <tr>
                <th>Titel</th>
                <th>Kort beskrivelse</th>
                <th>Forløb</th>
                <th>Elevtype</th>
                <th>Git repository</th>
                <th class="col-actions"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="template in templates" :key="template.id">
                <td class="cell-name">{{ template.title }}</td>
                <td class="cell-note">{{ template.shortDescription || "—" }}</td>
                <td>{{ template.haul }}</td>
                <td>{{ template.studentType }}</td>
                <td>
                  <span v-if="template.gitRepoUrl" class="repo-link">{{ template.gitRepoUrl }}</span>
                  <span v-else class="muted">—</span>
                </td>
                <td class="cell-actions">
                  <button
                    class="icon-btn danger"
                    @click="deleteTemplate(template.id)"
                    aria-label="Slet skabelon"
                  >
                    <IconTrash :size="15" :stroke-width="2" />
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </section>
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
}

.page-header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.page-subtitle {
  font-size: 13px;
  color: #6b7280;
}

.refresh-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 12px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  background: #ffffff;
  color: #374151;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease, border-color 0.2s ease;
}

.refresh-btn:hover {
  background: #f8fafc;
  border-color: #016bff;
  color: #016bff;
}

.surface {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.skeleton-surface {
  height: 640px;
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

.tab-bar {
  display: flex;
  gap: 6px;
  padding: 14px 18px 0;
  border-bottom: 1px solid #dde1e5;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 9px 14px;
  border: none;
  background: transparent;
  color: #6b7280;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
  transition: color 0.2s ease, border-color 0.2s ease;
}

.tab-btn:hover {
  color: #111827;
}

.tab-btn.active {
  color: #016bff;
  border-bottom-color: #016bff;
}

.tab-panel {
  padding: 22px 22px 26px;
}

.section-heading {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
  margin-bottom: 14px;
}

.divider {
  height: 1px;
  background: #dde1e5;
  margin: 24px 0;
}

.summary-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 22px;
}

.summary-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 12.5px;
  font-weight: 600;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  color: #374151;
}

.summary-num {
  font-weight: 800;
}

.summary-chip.present svg {
  color: #059669;
}

.summary-chip.late svg {
  color: #d97706;
}

.summary-chip.checked-out svg {
  color: #6b7280;
}

.summary-chip.absent svg {
  color: #dc2626;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.data-table th {
  text-align: left;
  font-size: 11.5px;
  font-weight: 700;
  color: #6b7280;
  padding: 8px 10px;
  border-bottom: 1px solid #e2e8f0;
}

.data-table td {
  padding: 10px;
  border-bottom: 1px solid #f1f5f9;
  color: #111827;
  vertical-align: middle;
}

.data-table.inner td {
  border-bottom-color: #f8fafc;
}

.table-row {
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.table-row:hover {
  background: #f8fafc;
}

.table-row.row-selected {
  background: #eff6ff;
}

.empty-row {
  text-align: center;
  color: #9ca3af;
  padding: 22px 10px !important;
}

.cell-name {
  font-weight: 600;
}

.cell-status {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 9px;
  border-radius: 999px;
  font-size: 11.5px;
  font-weight: 600;
  background: #f1f5f9;
  color: #475569;
  white-space: nowrap;
}

.status-pill .dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #94a3b8;
}

.status-pill.present {
  background: #ecfdf5;
  color: #065f46;
}

.status-pill.present .dot {
  background: #059669;
}

.status-pill.checked-out {
  background: #f8fafc;
  color: #475569;
}

.status-pill.checked-out .dot {
  background: #94a3b8;
}

.status-pill.absent {
  background: #fef2f2;
  color: #991b1b;
}

.status-pill.absent .dot {
  background: #dc2626;
}

.late-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 8px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  background: #fffbeb;
  color: #b45309;
  border: 1px solid #fcd34d;
  white-space: nowrap;
}

.cell-ff {
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.ff-total {
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.grant-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 32px;
  padding: 0 12px;
  border: 1.5px solid #016bff;
  border-radius: 9px;
  background: #ffffff;
  color: #016bff;
  font-size: 12px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  white-space: nowrap;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.grant-btn:hover {
  background: #eff6ff;
}

.block-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 32px;
  padding: 0 12px;
  border: 1.5px solid #059669;
  border-radius: 9px;
  background: #ffffff;
  color: #059669;
  font-size: 12px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  white-space: nowrap;
  transition: background-color 0.2s ease, color 0.2s ease, border-color 0.2s ease;
}

.block-btn:hover:not(:disabled) {
  background: #ecfdf5;
}

.block-btn.blocked {
  border-color: #dc2626;
  color: #dc2626;
}

.block-btn.blocked:hover:not(:disabled) {
  background: #fef2f2;
}

.block-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.col-actions {
  width: 64px;
  text-align: right;
}

.cell-actions {
  text-align: right;
  white-space: nowrap;
}

.cell-date {
  white-space: nowrap;
}

.cell-note {
  color: #4b5563;
}

.icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1.5px solid #e2e8f0;
  border-radius: 9px;
  background: #ffffff;
  color: #6b7280;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease, border-color 0.2s ease;
}

.icon-btn:hover {
  background: #f8fafc;
  color: #111827;
}

.icon-btn.primary {
  color: #ffffff;
  background: #016bff;
  border-color: #016bff;
}

.icon-btn.primary:hover {
  background: #005ae0;
}

.icon-btn.danger {
  color: #dc2626;
  border-color: #fecaca;
}

.icon-btn.danger:hover {
  background: #fef2f2;
}

.details-panel {
  margin-top: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #f8fafc;
  padding: 16px 18px;
  overflow-x: auto;
}

.details-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.details-title {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 14px;
  font-weight: 700;
  color: #111827;
}

.details-meta {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
}

.create-form {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 14px;
  align-items: end;
}

.grant-form {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 12px;
  padding: 14px;
  margin-bottom: 16px;
  border: 1px dashed #cbd5e1;
  border-radius: 12px;
  background: #ffffff;
}

.grant-form .form-field {
  flex: 1 1 160px;
}

.grant-input {
  max-width: 110px;
}

.grant-mode {
  display: inline-flex;
  align-items: center;
  height: 40px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
  background: #f8fafc;
}

.mode-btn {
  height: 100%;
  padding: 0 14px;
  border: none;
  background: transparent;
  color: #6b7280;
  font-size: 12.5px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.mode-btn.active {
  background: #016bff;
  color: #ffffff;
}

.mode-btn.deduct.active {
  background: #dc2626;
}

.create-btn.deduct {
  background-color: #dc2626;
}

.create-btn.deduct:hover:not(:disabled) {
  background-color: #b91c1c;
}

.neg {
  color: #dc2626;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.form-check {
  justify-content: flex-end;
}

.form-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #374151;
}

.form-input,
.form-select {
  height: 40px;
  background-color: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 11px;
  padding: 0 12px;
  font-family: inherit;
  font-size: 13px;
  color: #111827;
  outline: none;
  transition: border-color 0.2s ease, background-color 0.2s ease;
}

.form-select {
  cursor: pointer;
}

.form-select.small {
  height: 34px;
  max-width: 220px;
}

.form-input::placeholder {
  color: #9ca3af;
}

.form-input:focus,
.form-select:focus {
  border-color: #016bff;
  background-color: #ffffff;
}

.checkbox-label {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.custom-checkbox {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  border: 1.5px solid #cbd5e1;
  border-radius: 5px;
  color: #ffffff;
  background: #ffffff;
  transition: background-color 0.15s ease, border-color 0.15s ease;
}

.custom-checkbox.checked {
  background: #016bff;
  border-color: #016bff;
}

.checkbox-text {
  font-size: 13px;
  font-weight: 500;
  color: #374151;
}

.create-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  height: 40px;
  padding: 0 18px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 11px;
  font-size: 13.5px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.2s ease, transform 0.15s ease;
}

.create-btn:hover:not(:disabled) {
  background-color: #005ae0;
}

.create-btn:active:not(:disabled) {
  transform: scale(0.98);
}

.create-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.save-feedback {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  font-weight: 600;
  grid-column: 1 / -1;
}

.save-feedback.success {
  color: #059669;
}

.save-feedback.error {
  color: #dc2626;
}

.project-card {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 12px;
  background: #fdfdfd;
}

.project-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.project-title {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
}

.project-tag {
  padding: 3px 9px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  background: #eff6ff;
  color: #1d4ed8;
}

.project-tag.custom {
  background: #f5f3ff;
  color: #6d28d9;
}

.project-desc {
  font-size: 12.5px;
  color: #6b7280;
  margin: 8px 0 12px;
}

.muted {
  color: #9ca3af;
  font-size: 12.5px;
}

.repo-link {
  font-size: 12px;
  color: #016bff;
  word-break: break-all;
}

.assign-area {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.member-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.member-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 6px 4px 10px;
  border-radius: 999px;
  background: #f1f5f9;
  color: #334155;
  font-size: 12px;
  font-weight: 600;
}

.chip-remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  border: none;
  border-radius: 50%;
  background: transparent;
  color: #64748b;
  cursor: pointer;
  padding: 0;
  transition: background-color 0.15s ease, color 0.15s ease;
}

.chip-remove:hover {
  background: #e2e8f0;
  color: #dc2626;
}

.assign-form {
  display: flex;
  align-items: center;
  gap: 8px;
}

.assign-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  height: 34px;
  padding: 0 12px;
  border: 1.5px solid #016bff;
  border-radius: 9px;
  background: #ffffff;
  color: #016bff;
  font-size: 12.5px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.assign-btn:hover:not(:disabled) {
  background: #eff6ff;
}

.assign-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.empty-card {
  padding: 24px;
  text-align: center;
  color: #9ca3af;
  font-size: 13px;
  border: 1px dashed #dde1e5;
  border-radius: 12px;
}

.details-fade-enter-active,
.details-fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.details-fade-enter-from {
  opacity: 0;
  transform: translateY(-6px);
}

.details-fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>