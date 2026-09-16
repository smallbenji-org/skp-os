<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import {
  IconPlus,
  IconFolderOpen,
  IconFolder,
  IconSearch,
  IconBrandGithub,
  IconExternalLink,
  IconChevronRight,
  IconX,
  IconLoader2,
  IconAlertCircle,
  IconUser,
} from "@tabler/icons-vue";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useProjectTemplateStore } from "@/Stores/ProjectTemplateStore";
import type { ProjectStage } from "@/types";

const router = useRouter();
const projectStore = useProjectStore();
const studentProfileStore = useStudentProfileStore();
const projectTemplateStore = useProjectTemplateStore();

const isLoading = ref(true);

const showCreateModal = ref(false);
const isSubmitting = ref(false);
const formError = ref("");
const newProjectForm = ref({
  title: "",
  shortDescription: "",
  gitRepoUrl: "",
});

type SourceFilterKey = "all" | "personal" | "template";
const activeSourceFilter = ref<SourceFilterKey>("all");

type FilterKey = "all" | ProjectStage;
const activeStageFilter = ref<FilterKey>("all");

const searchQuery = ref("");

const myProfile = computed(() => studentProfileStore.MY_STUDENT_PROFILE);

const myProjects = computed(() => {
  const profileId = myProfile.value?.id;
  const list = projectStore.PROJECTS.filter((p) =>
    p.students?.some((s) => String(s.id) === String(profileId)),
  );

  const existingIds = new Set(list.map((p) => String(p.id)));
  for (const pp of projectStore.PERSONAL_PROJECTS) {
    if (!existingIds.has(String(pp.id))) {
      list.push(pp);
    }
  }

  return list.sort((a, b) => a.title.localeCompare(b.title, "da"));
});

const personalProjects = computed(() =>
  myProjects.value.filter((p) => p.isCustomProject),
);

const templateProjects = computed(() =>
  myProjects.value.filter((p) => !p.isCustomProject),
);

const sourceTabs = computed(() => [
  {
    id: "all" as SourceFilterKey,
    label: "Alle projekter",
    count: myProjects.value.length,
  },
  {
    id: "personal" as SourceFilterKey,
    label: "Personlige projekter",
    count: personalProjects.value.length,
  },
  {
    id: "template" as SourceFilterKey,
    label: "Skabelonprojekter",
    count: templateProjects.value.length,
  },
]);

const projectsBySource = computed(() => {
  if (activeSourceFilter.value === "personal") return personalProjects.value;
  if (activeSourceFilter.value === "template") return templateProjects.value;
  return myProjects.value;
});

const filteredProjects = computed(() => {
  let list = projectsBySource.value;

  if (activeStageFilter.value !== "all") {
    list = list.filter((p) => p.stage === activeStageFilter.value);
  }

  const query = searchQuery.value.trim().toLowerCase();
  if (query) {
    list = list.filter(
      (p) =>
        p.title.toLowerCase().includes(query) ||
        (p.shortDescription && p.shortDescription.toLowerCase().includes(query)),
    );
  }

  return list;
});

const stageOptions: { id: FilterKey; label: string }[] = [
  { id: "all", label: "Alle statusser" },
  { id: "Approved", label: "I gang" },
  { id: "Created", label: "Oprettet" },
  { id: "Submitted", label: "Afventer evaluering" },
  { id: "Evaluated", label: "Afsluttet" },
];

function getStageCount(key: FilterKey): number {
  if (key === "all") return projectsBySource.value.length;
  return projectsBySource.value.filter((p) => p.stage === key).length;
}

function getStageLabel(stage?: ProjectStage): string {
  switch (stage) {
    case "Created":
      return "Oprettet";
    case "Approved":
      return "I gang";
    case "Submitted":
      return "Afventer evaluering";
    case "Evaluated":
      return "Afsluttet";
    default:
      return stage || "Ukendt";
  }
}

function getStageDotClass(stage?: ProjectStage): string {
  switch (stage) {
    case "Approved":
      return "dot-approved";
    case "Created":
      return "dot-created";
    case "Submitted":
      return "dot-submitted";
    case "Evaluated":
      return "dot-evaluated";
    default:
      return "dot-default";
  }
}

const hasTemplates = computed(
  () => projectTemplateStore.PROJECT_TEMPLATES.length > 0,
);

function studentNames(students?: { user: { name: string } | null }[] | null): string {
  if (!students || !Array.isArray(students)) return "Ingen tilknyttet";
  const names = students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(", ") : "Ingen tilknyttet";
}

function isValidUrl(url?: string | null): boolean {
  if (!url) return false;
  return /^https?:\/\//i.test(url.trim());
}

function goToTemplates() {
  router.push({ name: "skp-projekter" });
}

function openProject(projectId: number | string) {
  router.push({ name: "projekt-detalje", params: { id: projectId } });
}

function openCreateModal() {
  formError.value = "";
  newProjectForm.value = {
    title: "",
    shortDescription: "",
    gitRepoUrl: "",
  };
  showCreateModal.value = true;
}

function closeCreateModal() {
  if (isSubmitting.value) return;
  showCreateModal.value = false;
  formError.value = "";
}

async function handleCreatePersonalProject() {
  formError.value = "";
  const title = newProjectForm.value.title.trim();
  const shortDescription = newProjectForm.value.shortDescription.trim();
  const gitRepoUrl = newProjectForm.value.gitRepoUrl.trim();

  if (!title) {
    formError.value = "Venligst indtast en titel på projektet.";
    return;
  }

  if (gitRepoUrl && !isValidUrl(gitRepoUrl)) {
    formError.value = "GitHub repository URL skal starte med http:// eller https://";
    return;
  }

  isSubmitting.value = true;
  try {
    const created = await projectStore.CREATE_PERSONAL_PROJECT({
      title,
      shortDescription,
      gitRepoUrl,
    });

    if (created) {
      closeCreateModal();
      openProject(created.id);
    } else {
      formError.value = "Der opstod en fejl under oprettelsen af projektet. Prøv igen.";
    }
  } catch (err: any) {
    formError.value = err?.message || "Der opstod en uventet fejl.";
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  try {
    await studentProfileStore.GET_MY_STUDENT_PROFILE();
    await Promise.all([
      projectStore.GET_PROJECTS(),
      projectStore.GET_PERSONAL_PROJECTS(),
      projectTemplateStore.GET_PROJECT_TEMPLATES(),
    ]);
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <header class="page-header">
      <div class="header-main">
        <h1 class="page-title">Mine Projekter</h1>
      </div>
      <div class="header-actions">
        <button
          class="btn btn-primary"
          type="button"
          @click="openCreateModal"
        >
          <IconPlus :size="16" :stroke-width="2.2" />
          <span>Nyt personligt projekt</span>
        </button>
        <button
          v-if="hasTemplates"
          class="btn btn-secondary"
          type="button"
          @click="goToTemplates"
        >
          <IconFolderOpen :size="16" :stroke-width="1.8" />
          <span>Vælg fra skabelon</span>
        </button>
      </div>
    </header>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else class="content-wrapper">
      <div class="filter-toolbar">
        <div class="segmented-control" role="tablist" aria-label="Projekttyper">
          <button
            v-for="st in sourceTabs"
            :key="st.id"
            type="button"
            role="tab"
            class="segment-btn"
            :class="{ active: activeSourceFilter === st.id }"
            :aria-selected="activeSourceFilter === st.id"
            @click="activeSourceFilter = st.id"
          >
            <span>{{ st.label }}</span>
            <span class="segment-count">{{ st.count }}</span>
          </button>
        </div>

        <div class="toolbar-controls">
          <div class="status-filter-wrap">
            <label for="status-select" class="status-label">Status:</label>
            <select
              id="status-select"
              v-model="activeStageFilter"
              class="status-select"
            >
              <option
                v-for="opt in stageOptions"
                :key="opt.id"
                :value="opt.id"
              >
                {{ opt.label }} ({{ getStageCount(opt.id) }})
              </option>
            </select>
          </div>

          <div class="search-wrap">
            <IconSearch :size="15" :stroke-width="1.8" class="search-icon" />
            <input
              v-model="searchQuery"
              type="search"
              class="search-input"
              placeholder="Søg i projekter..."
              aria-label="Søg i projekter"
            />
          </div>
        </div>
      </div>

      <div class="projects-table-container">
        <div class="table-header">
          <div class="th col-project">Projekt</div>
          <div class="th col-type">Type</div>
          <div class="th col-status">Status</div>
          <div class="th col-students">Tilknyttet</div>
          <div class="th col-actions">Handling</div>
        </div>

        <Transition name="tab-fade" mode="out-in">
          <div :key="activeSourceFilter" class="table-content-area">
            <div
              v-if="filteredProjects.length === 0"
              class="empty-state"
            >
              <div class="empty-title">Ingen projekter fundet</div>
              <p class="empty-text">
                {{
                  searchQuery.trim()
                    ? "Der er ingen projekter, der matcher din søgning."
                    : activeStageFilter !== "all"
                    ? "Der er ingen projekter med den valgte status i denne kategori."
                    : activeSourceFilter === "personal"
                    ? "Du har endnu ikke oprettet nogen personlige projekter."
                    : activeSourceFilter === "template"
                    ? "Du er ikke tilknyttet nogen skabelonprojekter endnu."
                    : "Du har endnu ingen projekter tilknyttet din profil."
                }}
              </p>
              <div class="empty-actions">
                <button
                  v-if="searchQuery.trim() || activeStageFilter !== 'all'"
                  type="button"
                  class="btn btn-secondary"
                  @click="activeStageFilter = 'all'; searchQuery = ''"
                >
                  Nulstil filtre
                </button>
                <button
                  v-else-if="activeSourceFilter === 'personal' || activeSourceFilter === 'all'"
                  type="button"
                  class="btn btn-primary"
                  @click="openCreateModal"
                >
                  <IconPlus :size="15" :stroke-width="2" />
                  <span>Opret personligt projekt</span>
                </button>
                <button
                  v-else-if="hasTemplates"
                  type="button"
                  class="btn btn-secondary"
                  @click="goToTemplates"
                >
                  <IconFolderOpen :size="15" :stroke-width="1.8" />
                  <span>Vis SKP-skabeloner</span>
                </button>
              </div>
            </div>

            <div v-else class="table-body">
              <div
                v-for="project in filteredProjects"
                :key="project.id"
                class="table-row"
                tabindex="0"
                role="button"
                :aria-label="`Åbn projekt: ${project.title}`"
                @click="openProject(project.id)"
                @keydown.enter="openProject(project.id)"
                @keydown.space.prevent="openProject(project.id)"
              >
                <div class="td col-project">
                  <div class="project-icon-box">
                    <IconFolder :size="18" :stroke-width="1.8" />
                  </div>
                  <div class="project-info">
                    <span class="project-title">{{ project.title }}</span>
                    <span
                      v-if="project.shortDescription"
                      class="project-desc"
                      :title="project.shortDescription"
                    >
                      {{ project.shortDescription }}
                    </span>
                  </div>
                </div>

                <div class="td col-type">
                  <span class="type-cell-label">Type:</span>
                  <span
                    class="type-text"
                    :class="{ 'is-personal': project.isCustomProject }"
                  >
                    {{
                      project.isCustomProject
                        ? "Personligt projekt"
                        : project.projectTemplate?.title || "Skabelonprojekt"
                    }}
                  </span>
                </div>

                <div class="td col-status">
                  <span class="type-cell-label">Status:</span>
                  <div class="status-indicator">
                    <span
                      class="status-dot"
                      :class="getStageDotClass(project.stage)"
                    />
                    <span class="status-text">{{ getStageLabel(project.stage) }}</span>
                  </div>
                </div>

                <div class="td col-students">
                  <span class="type-cell-label">Tilknyttet:</span>
                  <div class="student-meta" :title="studentNames(project.students)">
                    <IconUser :size="14" :stroke-width="1.8" class="student-icon" />
                    <span class="student-text">{{ studentNames(project.students) }}</span>
                  </div>
                </div>

                <div class="td col-actions" @click.stop>
                  <a
                    v-if="isValidUrl(project.gitRepoUrl)"
                    :href="project.gitRepoUrl"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="repo-link"
                    title="Åbn GitHub repository"
                  >
                    <IconBrandGithub :size="14" :stroke-width="1.8" />
                    <span>GitHub</span>
                    <IconExternalLink :size="12" :stroke-width="2" class="ext-icon" />
                  </a>
                  <button
                    type="button"
                    class="open-btn"
                    title="Åbn projekt"
                    @click="openProject(project.id)"
                  >
                    <span>Åbn</span>
                    <IconChevronRight :size="14" :stroke-width="2" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showCreateModal"
          class="modal-backdrop"
          @click.self="closeCreateModal"
          @keydown.esc="closeCreateModal"
        >
          <div
            class="modal-dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="personal-modal-title"
          >
            <div class="modal-header">
              <div>
                <h2 id="personal-modal-title" class="modal-title">
                  Nyt personligt projekt
                </h2>
                <p class="modal-subtitle">
                  Opret dit eget projekt. Du tilknyttes automatisk som elev.
                </p>
              </div>
              <button
                type="button"
                class="modal-close-btn"
                aria-label="Luk dialog"
                :disabled="isSubmitting"
                @click="closeCreateModal"
              >
                <IconX :size="18" :stroke-width="1.8" />
              </button>
            </div>

            <form class="modal-form" @submit.prevent="handleCreatePersonalProject">
              <div v-if="formError" class="modal-error-alert" role="alert">
                <IconAlertCircle :size="16" :stroke-width="2" class="error-alert-icon" />
                <span>{{ formError }}</span>
              </div>

              <div class="form-group">
                <label for="personal-title" class="form-label">
                  Projekttitel <span class="required">*</span>
                </label>
                <input
                  id="personal-title"
                  v-model="newProjectForm.title"
                  type="text"
                  class="form-input"
                  placeholder="fx Portfolio website, Rust CLI værktøj..."
                  required
                  :disabled="isSubmitting"
                  autofocus
                />
              </div>

              <div class="form-group">
                <label for="personal-desc" class="form-label">
                  Kort beskrivelse
                </label>
                <textarea
                  id="personal-desc"
                  v-model="newProjectForm.shortDescription"
                  class="form-textarea"
                  rows="3"
                  placeholder="Hvad går dit personlige projekt ud på?"
                  :disabled="isSubmitting"
                ></textarea>
              </div>

              <div class="form-group">
                <label for="personal-git" class="form-label">
                  GitHub repository URL
                </label>
                <div class="input-with-icon">
                  <IconBrandGithub :size="16" :stroke-width="1.8" class="input-icon" />
                  <input
                    id="personal-git"
                    v-model="newProjectForm.gitRepoUrl"
                    type="url"
                    class="form-input icon-padded"
                    placeholder="https://github.com/brugernavn/projekt"
                    :disabled="isSubmitting"
                  />
                </div>
                <span class="field-hint">Valgfrit. Kan også tilføjes eller redigeres senere.</span>
              </div>

              <div class="modal-actions">
                <button
                  type="button"
                  class="btn btn-secondary"
                  :disabled="isSubmitting"
                  @click="closeCreateModal"
                >
                  Annuller
                </button>
                <button
                  type="submit"
                  class="btn btn-primary"
                  :disabled="isSubmitting || !newProjectForm.title.trim()"
                >
                  <IconLoader2 v-if="isSubmitting" :size="15" class="spinning" />
                  <IconPlus v-else :size="15" :stroke-width="2.2" />
                  <span>{{ isSubmitting ? "Opretter..." : "Opret projekt" }}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.header-main {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  line-height: 1.25;
}

.page-subtitle {
  font-size: 13.5px;
  color: #64748b;
  margin: 0;
  line-height: 1.4;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 36px;
  padding: 0 14px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.15s ease, border-color 0.15s ease, color 0.15s ease;
  white-space: nowrap;
  outline: none;
}

.btn-primary {
  background-color: #016bff;
  color: #ffffff;
  border: 1px solid #016bff;
}

.btn-primary:hover:not(:disabled) {
  background-color: #005ae0;
  border-color: #005ae0;
}

.btn-primary:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #ffffff;
  color: #334155;
  border: 1px solid #cbd5e1;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #f8fafc;
  border-color: #94a3b8;
  color: #0f172a;
}

.btn-secondary:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.content-wrapper {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.filter-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.segmented-control {
  display: inline-flex;
  align-items: center;
  background-color: #f1f5f9;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 3px;
  gap: 2px;
}

.segment-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 28px;
  padding: 0 11px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: #64748b;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s ease;
  user-select: none;
}

.segment-btn:hover:not(.active) {
  color: #0f172a;
}

.segment-btn.active {
  background-color: #ffffff;
  color: #0f172a;
  font-weight: 600;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.segment-count {
  font-size: 11.5px;
  color: #94a3b8;
  font-weight: 500;
}

.segment-btn.active .segment-count {
  color: #016bff;
  font-weight: 600;
}

.toolbar-controls {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.status-filter-wrap {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.status-label {
  font-size: 13px;
  color: #64748b;
  font-weight: 500;
}

.status-select {
  height: 32px;
  padding: 0 28px 0 10px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background-color: #ffffff;
  color: #1e293b;
  font-size: 13px;
  font-family: inherit;
  cursor: pointer;
  outline: none;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='%2364748b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 8px center;
  transition: border-color 0.15s ease;
}

.status-select:focus {
  border-color: #016bff;
}

.search-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 9px;
  color: #94a3b8;
  pointer-events: none;
}

.search-input {
  height: 32px;
  padding: 0 10px 0 30px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 13px;
  font-family: inherit;
  color: #0f172a;
  background-color: #ffffff;
  outline: none;
  width: 180px;
  transition: border-color 0.15s ease, width 0.15s ease;
}

.search-input:focus {
  border-color: #016bff;
  width: 220px;
}

.projects-table-container {
  background-color: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow-x: auto;
}

.table-header {
  display: grid;
  grid-template-columns: minmax(240px, 2.2fr) minmax(130px, 1fr) 190px 160px 110px;
  align-items: center;
  padding: 10px 16px;
  background-color: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  min-width: 830px;
}

.th {
  font-size: 11.5px;
  font-weight: 600;
  color: #64748b;
}

.th.col-actions {
  text-align: right;
}

.table-content-area {
  min-width: 830px;
}

.table-body {
  display: flex;
  flex-direction: column;
}

.table-row {
  display: grid;
  grid-template-columns: minmax(240px, 2.2fr) minmax(130px, 1fr) 190px 160px 110px;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f1f5f9;
  cursor: pointer;
  outline: none;
  background-color: #ffffff;
  transition: background-color 0.12s ease;
}

.table-row:last-child {
  border-bottom: none;
}

.table-row:hover {
  background-color: #f8fafc;
}

.table-row:focus-visible {
  background-color: #f8fafc;
  outline: 2px solid #016bff;
  outline-offset: -2px;
}

.col-project {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
  padding-right: 12px;
}

.project-icon-box {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  border-radius: 8px;
  background-color: #f1f5f9;
  color: #64748b;
  flex-shrink: 0;
  transition: color 0.15s ease, background-color 0.15s ease;
}

.table-row:hover .project-icon-box {
  background-color: #eff6ff;
  color: #016bff;
}

.project-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.project-title {
  font-size: 13.5px;
  font-weight: 600;
  color: #0f172a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.project-desc {
  font-size: 12.5px;
  color: #64748b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.35;
}

.col-type {
  min-width: 0;
}

.type-text {
  font-size: 13px;
  color: #475569;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  display: block;
}

.type-text.is-personal {
  color: #0f172a;
  font-weight: 500;
}

.col-status {
  min-width: 0;
}

.status-indicator {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #334155;
  white-space: nowrap;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dot-approved {
  background-color: #016bff;
}

.dot-created {
  background-color: #94a3b8;
}

.dot-submitted {
  background-color: #d97706;
}

.dot-evaluated {
  background-color: #16a34a;
}

.dot-default {
  background-color: #94a3b8;
}

.status-text {
  line-height: 1;
}

.col-students {
  min-width: 0;
  padding-right: 12px;
}

.student-meta {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  color: #64748b;
  max-width: 100%;
}

.student-icon {
  flex-shrink: 0;
  color: #94a3b8;
}

.student-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.col-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}

.repo-link {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  font-size: 12px;
  color: #64748b;
  text-decoration: none;
  padding: 4px 6px;
  border-radius: 6px;
  transition: color 0.15s ease, background-color 0.15s ease;
  white-space: nowrap;
}

.repo-link:hover {
  color: #0f172a;
  background-color: #f1f5f9;
}

.ext-icon {
  opacity: 0.6;
}

.open-btn {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  height: 28px;
  padding: 0 8px;
  border: none;
  background: transparent;
  color: #64748b;
  border-radius: 6px;
  font-size: 12.5px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: color 0.15s ease, background-color 0.15s ease;
  white-space: nowrap;
}

.table-row:hover .open-btn {
  color: #016bff;
  background-color: #eff6ff;
}

.type-cell-label {
  display: none;
  font-size: 11.5px;
  font-weight: 600;
  color: #94a3b8;
  margin-right: 4px;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 48px 20px;
  gap: 8px;
}

.empty-title {
  font-size: 15px;
  font-weight: 600;
  color: #0f172a;
}

.empty-text {
  font-size: 13.5px;
  color: #64748b;
  max-width: 380px;
  margin: 0;
  line-height: 1.45;
}

.empty-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 10px;
  flex-wrap: wrap;
  justify-content: center;
}

.skeleton-surface {
  height: 380px;
  background: linear-gradient(90deg, #f1f5f9 25%, #f8fafc 50%, #f1f5f9 75%);
  background-size: 400% 100%;
  animation: shimmer 1.4s ease infinite;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

@keyframes shimmer {
  0% {
    background-position: 100% 0;
  }
  100% {
    background-position: -100% 0;
  }
}

.tab-fade-enter-active {
  transition: opacity 0.18s ease-out, transform 0.18s ease-out;
}

.tab-fade-leave-active {
  transition: opacity 0.12s ease-in;
}

.tab-fade-enter-from {
  opacity: 0;
  transform: translateY(4px);
}

.tab-fade-leave-to {
  opacity: 0;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  background-color: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(2px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 1000;
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-dialog {
  transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.2s ease;
}

.modal-leave-active .modal-dialog {
  transition: transform 0.15s ease, opacity 0.15s ease;
}

.modal-enter-from .modal-dialog {
  opacity: 0;
  transform: scale(0.96) translateY(8px);
}

.modal-leave-to .modal-dialog {
  opacity: 0;
  transform: scale(0.98) translateY(4px);
}

.modal-dialog {
  background-color: #ffffff;
  border-radius: 10px;
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05);
  width: 100%;
  max-width: 480px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding: 18px 20px;
  border-bottom: 1px solid #f1f5f9;
}

.modal-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #0f172a;
}

.modal-subtitle {
  margin: 3px 0 0 0;
  font-size: 13px;
  color: #64748b;
  line-height: 1.35;
}

.modal-close-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  transition: all 0.15s ease;
  flex-shrink: 0;
}

.modal-close-btn:hover {
  background-color: #f1f5f9;
  color: #0f172a;
}

.modal-form {
  padding: 18px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.modal-error-alert {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  font-size: 12.5px;
}

.error-alert-icon {
  flex-shrink: 0;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.form-label {
  font-size: 13px;
  font-weight: 500;
  color: #334155;
}

.required {
  color: #dc2626;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 13px;
  font-family: inherit;
  color: #0f172a;
  background-color: #ffffff;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease;
}

.form-input:focus,
.form-textarea:focus {
  border-color: #016bff;
}

.form-textarea {
  resize: vertical;
  min-height: 75px;
}

.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 10px;
  color: #94a3b8;
  pointer-events: none;
}

.icon-padded {
  padding-left: 32px;
}

.field-hint {
  font-size: 11.5px;
  color: #94a3b8;
}

.modal-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 6px;
}

.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 860px) {
  .projects-table-container {
    overflow-x: visible;
  }

  .table-header {
    display: none;
  }

  .table-content-area {
    min-width: 0;
  }

  .table-row {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
    padding: 14px 16px;
    min-width: 0;
  }

  .col-project {
    width: 100%;
    padding-right: 0;
  }

  .col-type,
  .col-status,
  .col-students {
    display: flex;
    align-items: center;
    gap: 6px;
    width: 100%;
    padding-right: 0;
  }

  .type-cell-label {
    display: inline;
  }

  .col-actions {
    width: 100%;
    justify-content: flex-start;
    padding-top: 6px;
    border-top: 1px dashed #f1f5f9;
  }
}

@media (max-width: 640px) {
  .header-actions {
    width: 100%;
  }

  .header-actions .btn {
    flex: 1;
  }

  .filter-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .segmented-control {
    width: 100%;
  }

  .segment-btn {
    flex: 1;
    justify-content: center;
  }

  .toolbar-controls {
    flex-direction: column;
    align-items: stretch;
  }

  .status-filter-wrap {
    width: 100%;
  }

  .status-select {
    flex: 1;
  }

  .search-input {
    width: 100%;
  }

  .search-input:focus {
    width: 100%;
  }
}
</style>