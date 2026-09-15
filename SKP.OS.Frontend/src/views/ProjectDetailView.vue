<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  IconArrowLeft,
  IconExternalLink,
  IconCheck,
  IconAlertTriangle,
  IconUsers,
  IconFolderOpen,
  IconX,
  IconMessage,
  IconEdit,
  IconLink,
  IconNotes,
} from "@tabler/icons-vue";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useAuthStore } from "@/Stores/AuthStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import ProjectStageBadge from "@/components/ProjectStageBadge.vue";
import { STAGE_ORDER } from "@/utils/project-stage";
import type { ProjectStage, UpdateProjectDto } from "@/types";

const route = useRoute();
const router = useRouter();
const projectStore = useProjectStore();
const authStore = useAuthStore();
const studentProfileStore = useStudentProfileStore();

const projectId = computed(() => Number(route.params.id));

const isLoading = ref(true);
const notFound = ref(false);
const isEditing = ref(false);
const isSaving = ref(false);
const isSubmitting = ref(false);
const saveFeedback = ref<"idle" | "success" | "error">("idle");
const submitFeedback = ref<"idle" | "success" | "error">("idle");
const stageBusy = ref(false);
const stageError = ref("");
const feedbackDraft = ref("");
const feedbackSaveFeedback = ref<"idle" | "saving" | "success" | "error">("idle");

const isStudent = computed(() => authStore.HAS_ROLE("Student"));
const isInstructor = computed(() => authStore.HAS_ROLE("Instructor"));

const project = computed(() => projectStore.SELECTED_PROJECT);

const form = reactive<UpdateProjectDto>({
  title: "",
  shortDescription: "",
  evaluation: "",
  conclusion: "",
  perspektivering: "",
  gitRepoUrl: "",
  isCustomProject: false,
  projectTemplateId: null,
});

function isMyProject(): boolean {
  const profile = studentProfileStore.MY_STUDENT_PROFILE;
  return (
    profile != null &&
    project.value?.students.some((s) => s.id === profile.id) === true
  );
}

const canEdit = computed(() => {
  if (!project.value) return false;
  if (isStudent.value) return isMyProject();
  return isInstructor.value;
});

const canSubmit = computed(
  () =>
    project.value?.stage === "Approved" && isStudent.value && isMyProject(),
);

const locked = computed(() => {
  const stage = project.value?.stage;
  return stage === "Submitted" || stage === "Evaluated";
});

const canGiveFeedback = computed(
  () =>
    isInstructor.value &&
    (project.value?.stage === "Submitted" || project.value?.stage === "Evaluated"),
);

function applyProject() {
  const p = project.value;
  if (!p) return;
  form.title = p.title;
  form.shortDescription = p.shortDescription;
  form.evaluation = p.evaluation;
  form.conclusion = p.conclusion;
  form.perspektivering = p.perspektivering;
  form.gitRepoUrl = p.gitRepoUrl;
  form.isCustomProject = p.isCustomProject;
  form.projectTemplateId = p.projectTemplateId;
  feedbackDraft.value = p.feedback ?? "";
}

async function load() {
  await projectStore.GET_PROJECT(projectId.value);
  if (projectStore.SELECTED_PROJECT == null) {
    notFound.value = true;
  } else {
    applyProject();
  }
}

watch(
  () => route.params.id,
  async (newId) => {
    if (newId) {
      await load();
    }
  }
);

function startEdit() {
  applyProject();
  isEditing.value = true;
}

function cancelEdit() {
  applyProject();
  isEditing.value = false;
}

async function saveProject() {
  if (!project.value || isSaving.value) return;
  isSaving.value = true;
  saveFeedback.value = "idle";
  const updated = await projectStore.UPDATE_PROJECT(project.value.id, { ...form });
  isSaving.value = false;
  saveFeedback.value = updated ? "success" : "error";
  if (updated) {
    applyProject();
    isEditing.value = false;
  }
  window.setTimeout(() => {
    saveFeedback.value = "idle";
  }, 3500);
}

async function submitProject() {
  if (!project.value || isSubmitting.value) return;
  isSubmitting.value = true;
  submitFeedback.value = "idle";
  const updated = await projectStore.SUBMIT_PROJECT(project.value.id, { ...form });
  isSubmitting.value = false;
  submitFeedback.value = updated ? "success" : "error";
  if (updated) {
    applyProject();
    isEditing.value = false;
  }
  window.setTimeout(() => {
    submitFeedback.value = "idle";
  }, 4000);
}

async function saveInstructorFeedback() {
  if (!project.value || feedbackSaveFeedback.value === "saving") return;
  feedbackSaveFeedback.value = "saving";
  const updated = await projectStore.UPDATE_PROJECT_FEEDBACK(
    project.value.id,
    feedbackDraft.value.trim() === "" ? null : feedbackDraft.value,
  );
  feedbackSaveFeedback.value = updated ? "success" : "error";
  if (updated) applyProject();
  window.setTimeout(() => {
    feedbackSaveFeedback.value = "idle";
  }, 3000);
}

function previousStage(stage: ProjectStage): ProjectStage | null {
  const idx = STAGE_ORDER.indexOf(stage);
  return idx > 0 ? STAGE_ORDER[idx - 1] : null;
}

async function changeStage(stage: ProjectStage) {
  if (!project.value || stageBusy.value) return;
  stageBusy.value = true;
  stageError.value = "";
  const updated = await projectStore.UPDATE_PROJECT_STAGE(project.value.id, stage);
  stageBusy.value = false;
  if (updated) {
    applyProject();
  } else {
    stageError.value = "Kunne ikke ændre stadie.";
  }
  window.setTimeout(() => {
    stageError.value = "";
  }, 3000);
}

function isValidUrl(url?: string | null): boolean {
  if (!url) return false;
  return /^https?:\/\//i.test(url.trim());
}

function openRepo() {
  const url = project.value?.gitRepoUrl?.trim() ?? "";
  if (!isValidUrl(url)) return;
  window.open(url, "_blank", "noopener,noreferrer");
}

function studentNames(): string {
  if (!project.value) return "";
  const names = project.value.students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(" · ") : "Ingen elever tilknyttet";
}

function formatStudentType(type?: string | null): string {
  if (!type) return "";
  const map: Record<string, string> = {
    DatateknikerProg: "Programmering",
    DatateknikerInfra: "Infrastruktur",
    ItSupporter: "IT-Supporter",
  };
  return map[type] ?? type;
}

function goBack() {
  if (isInstructor.value) {
    router.push({ name: "underviser" });
  } else {
    router.push({ name: "projekter" });
  }
}

onMounted(async () => {
  try {
    await studentProfileStore.GET_MY_STUDENT_PROFILE();
    await load();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <nav class="top-nav" aria-label="Brødkrummer">
      <button class="back-link-btn" type="button" @click="goBack">
        <IconArrowLeft :size="16" :stroke-width="2.2" />
        <span>{{ isInstructor ? "Tilbage til underviser" : "Mine Projekter" }}</span>
      </button>
    </nav>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="notFound" class="surface not-found-card">
      <h2 class="empty-title">Projektet blev ikke fundet</h2>
      <p class="empty-text">Det er måske blevet slettet eller findes ikke.</p>
      <button class="primary-btn" type="button" @click="goBack">
        Gå tilbage
      </button>
    </div>

    <template v-else-if="project">
      <!-- Status & Notice Banners -->
      <div v-if="locked" class="surface notice-banner">
        <IconCheck :size="16" :stroke-width="2.5" class="notice-icon" />
        <span>Projektet er afleveret og låst. Det kan ikke redigeres længere.</span>
      </div>

      <div
        v-if="isStudent && !canSubmit && project.stage === 'Created'"
        class="surface notice-banner info"
      >
        <IconAlertTriangle :size="16" :stroke-width="2" class="notice-icon" />
        <span>Projektet skal godkendes af en underviser, før det kan afleveres.</span>
      </div>

      <div
        v-if="project.feedback"
        class="surface feedback-banner"
      >
        <div class="feedback-banner-header">
          <IconMessage :size="16" :stroke-width="2.2" />
          <span>Feedback fra underviser</span>
        </div>
        <p class="feedback-banner-text">{{ project.feedback }}</p>
      </div>

      <div v-if="saveFeedback === 'success'" class="feedback-toast success">
        <IconCheck :size="15" :stroke-width="2.5" />
        <span>Ændringer er gemt</span>
      </div>
      <div v-else-if="saveFeedback === 'error'" class="feedback-toast error">
        <IconAlertTriangle :size="15" :stroke-width="2.2" />
        <span>Kunne ikke gemme ændringer</span>
      </div>

      <div v-if="submitFeedback === 'success'" class="feedback-toast success">
        <IconCheck :size="15" :stroke-width="2.5" />
        <span>Projektet er afleveret til bedømmelse</span>
      </div>
      <div v-else-if="submitFeedback === 'error'" class="feedback-toast error">
        <IconAlertTriangle :size="15" :stroke-width="2.2" />
        <span>Projektet kunne ikke afleveres</span>
      </div>

      <!-- READ-ONLY PROJECT OVERVIEW (DEFAULT) -->
      <section v-if="!isEditing" class="project-view-layout">
        <!-- Project Hero Card -->
        <div class="surface project-hero-card">
          <div class="hero-top-row">
            <div class="hero-badges">
              <ProjectStageBadge :stage="project.stage" size="md" />
              <span
                v-if="project.projectTemplate || project.isCustomProject"
                class="category-tag"
              >
                {{
                  project.isCustomProject
                    ? "Eget projekt"
                    : project.projectTemplate?.title
                }}
              </span>
              <span
                v-if="project.projectTemplate?.studentType"
                class="type-tag"
              >
                {{ formatStudentType(project.projectTemplate.studentType) }}
              </span>
              <span
                v-if="project.projectTemplate?.haul"
                class="haul-tag"
              >
                {{ project.projectTemplate.haul }}
              </span>
            </div>

            <div class="hero-actions">
              <button
                v-if="canEdit && !locked"
                class="outline-btn"
                type="button"
                @click="startEdit"
              >
                <IconEdit :size="15" :stroke-width="2" />
                <span>Rediger projekt</span>
              </button>

              <button
                v-if="isValidUrl(project.gitRepoUrl)"
                class="outline-btn"
                type="button"
                @click="openRepo"
              >
                <IconExternalLink :size="15" :stroke-width="2" />
                <span>Åbn Git repo</span>
              </button>

              <button
                v-if="canSubmit"
                class="submit-action-btn"
                type="button"
                :disabled="isSubmitting"
                @click="submitProject"
              >
                <IconFolderOpen :size="15" :stroke-width="2.2" />
                <span>Aflever projekt</span>
              </button>
            </div>
          </div>

          <h1 class="project-hero-title">{{ project.title }}</h1>

          <div class="project-hero-meta">
            <div class="hero-meta-item">
              <IconUsers :size="16" :stroke-width="2" class="meta-icon" />
              <span class="hero-meta-text">{{ studentNames() }}</span>
            </div>
          </div>
        </div>

        <!-- Overview Two-Column Grid -->
        <div class="overview-grid">
          <!-- Left Column: Description -->
          <div class="surface content-card">
            <div class="section-label">Projektbeskrivelse</div>
            <p class="description-body">
              {{ project.shortDescription || "Der er ikke angivet nogen kort beskrivelse for dette projekt endnu." }}
            </p>
          </div>

          <!-- Right Column: Technical & Project Metadata -->
          <div class="surface content-card meta-card">
            <div class="section-label">Projektoplysninger</div>
            <dl class="info-list">
              <div class="info-row">
                <dt class="info-key">Status</dt>
                <dd class="info-val">
                  <ProjectStageBadge :stage="project.stage" size="sm" />
                </dd>
              </div>

              <div class="info-row">
                <dt class="info-key">Deltagere</dt>
                <dd class="info-val">{{ studentNames() }}</dd>
              </div>

              <div v-if="project.projectTemplate || project.isCustomProject" class="info-row">
                <dt class="info-key">Skabelon</dt>
                <dd class="info-val">
                  {{ project.isCustomProject ? "Eget projekt" : project.projectTemplate?.title }}
                </dd>
              </div>

              <div v-if="project.projectTemplate?.studentType" class="info-row">
                <dt class="info-key">Fagretning</dt>
                <dd class="info-val">
                  {{ formatStudentType(project.projectTemplate.studentType) }}
                </dd>
              </div>

              <div class="info-row">
                <dt class="info-key">Git repository</dt>
                <dd class="info-val">
                  <a
                    v-if="isValidUrl(project.gitRepoUrl)"
                    :href="project.gitRepoUrl"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="repo-link-inline"
                  >
                    <IconLink :size="13" :stroke-width="2.2" />
                    <span>Gå til repository</span>
                  </a>
                  <span v-else class="text-muted">Ikke angivet</span>
                </dd>
              </div>
            </dl>
          </div>
        </div>

        <!-- Reflection / Evaluation Section -->
        <div class="surface reflection-card">
          <div class="section-header-row">
            <div>
              <h2 class="section-heading">Projektrefleksion</h2>
              <p class="section-sub">
                Evaluering, konklusion og perspektivering udfyldt af eleven.
              </p>
            </div>
            <button
              v-if="canEdit && !locked"
              class="edit-sub-btn"
              type="button"
              @click="startEdit"
            >
              <IconEdit :size="14" :stroke-width="2" />
              <span>Rediger refleksion</span>
            </button>
          </div>

          <div class="reflection-grid">
            <div class="reflection-block">
              <div class="reflection-header">
                <IconNotes :size="15" :stroke-width="2" class="reflection-icon" />
                <h3 class="reflection-title">Evaluering</h3>
              </div>
              <p
                class="reflection-text"
                :class="{ 'text-placeholder': !project.evaluation }"
              >
                {{ project.evaluation || "Ingen evaluering skrevet endnu." }}
              </p>
            </div>

            <div class="reflection-block">
              <div class="reflection-header">
                <IconNotes :size="15" :stroke-width="2" class="reflection-icon" />
                <h3 class="reflection-title">Konklusion</h3>
              </div>
              <p
                class="reflection-text"
                :class="{ 'text-placeholder': !project.conclusion }"
              >
                {{ project.conclusion || "Ingen konklusion skrevet endnu." }}
              </p>
            </div>

            <div class="reflection-block">
              <div class="reflection-header">
                <IconNotes :size="15" :stroke-width="2" class="reflection-icon" />
                <h3 class="reflection-title">Perspektivering</h3>
              </div>
              <p
                class="reflection-text"
                :class="{ 'text-placeholder': !project.perspektivering }"
              >
                {{ project.perspektivering || "Ingen perspektivering skrevet endnu." }}
              </p>
            </div>
          </div>
        </div>

        <!-- Instructor Controls Section (for Instructors) -->
        <div v-if="isInstructor" class="surface instructor-card">
          <div class="instructor-card-top">
            <div>
              <h2 class="section-heading">Underviserstyring</h2>
              <p class="section-sub">
                Godkend, evaluér eller giv skriftlig feedback til eleverne.
              </p>
            </div>
            <ProjectStageBadge :stage="project.stage" size="md" />
          </div>

          <div class="instructor-actions-row">
            <div class="stage-btns">
              <button
                v-if="project.stage === 'Created'"
                class="stage-action-btn approve"
                type="button"
                :disabled="stageBusy"
                @click="changeStage('Approved')"
              >
                <IconCheck :size="14" :stroke-width="2.5" />
                <span>Godkend projekt</span>
              </button>

              <button
                v-if="project.stage === 'Submitted'"
                class="stage-action-btn evaluate"
                type="button"
                :disabled="stageBusy"
                @click="changeStage('Evaluated')"
              >
                <IconCheck :size="14" :stroke-width="2.5" />
                <span>Marker som evalueret</span>
              </button>

              <button
                v-if="project.stage !== 'Created'"
                class="stage-action-btn back"
                type="button"
                :disabled="stageBusy"
                @click="changeStage(previousStage(project.stage)!)"
              >
                <IconX :size="14" :stroke-width="2" />
                <span>Fortryd stadie</span>
              </button>
            </div>

            <span v-if="stageError" class="stage-error">
              <IconAlertTriangle :size="14" :stroke-width="2" />
              {{ stageError }}
            </span>
          </div>

          <div v-if="canGiveFeedback" class="feedback-field-wrap">
            <label class="form-label" for="instructor-feedback-input">
              Feedback til eleverne
            </label>
            <textarea
              id="instructor-feedback-input"
              v-model="feedbackDraft"
              class="form-input"
              rows="4"
              placeholder="Skriv din bedømmelse eller kommentarer her..."
            />
            <div class="feedback-footer">
              <button
                class="primary-btn"
                type="button"
                :disabled="feedbackSaveFeedback === 'saving'"
                @click="saveInstructorFeedback"
              >
                <IconCheck :size="15" :stroke-width="2.5" />
                <span>{{ feedbackSaveFeedback === "saving" ? "Gemmer…" : "Gem feedback" }}</span>
              </button>

              <span
                v-if="feedbackSaveFeedback === 'success'"
                class="save-status-msg success"
              >
                <IconCheck :size="13" :stroke-width="2.5" />
                Feedback gemt
              </span>
              <span
                v-else-if="feedbackSaveFeedback === 'error'"
                class="save-status-msg error"
              >
                <IconAlertTriangle :size="13" :stroke-width="2" />
                Kunne ikke gemme feedback
              </span>
            </div>
          </div>
        </div>
      </section>

      <!-- EDIT MODE FORM -->
      <section v-else class="edit-layout">
        <div class="surface edit-card">
          <div class="edit-header">
            <div>
              <h2 class="edit-title">Rediger projekt</h2>
              <p class="edit-sub">
                Opdater oplysninger og refleksionsafsnit for "{{ project.title }}".
              </p>
            </div>
          </div>

          <form class="edit-form" @submit.prevent="saveProject">
            <!-- Section 1: Project Information -->
            <div class="form-section">
              <h3 class="form-section-title">Projektoplysninger</h3>

              <div class="form-group">
                <label class="form-label" for="edit-title">Projekttitel</label>
                <input
                  id="edit-title"
                  v-model="form.title"
                  class="form-input"
                  type="text"
                  required
                  :disabled="isSaving"
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="edit-desc">Kort beskrivelse</label>
                <textarea
                  id="edit-desc"
                  v-model="form.shortDescription"
                  class="form-input"
                  rows="3"
                  placeholder="Hvad handler projektet overordnet om?"
                  :disabled="isSaving"
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="edit-repo">Git repository URL</label>
                <input
                  id="edit-repo"
                  v-model="form.gitRepoUrl"
                  class="form-input"
                  type="url"
                  placeholder="https://github.com/..."
                  :disabled="isSaving"
                />
                <span class="field-hint">Valgfrit link til projektets kode eller repository.</span>
              </div>
            </div>

            <div class="form-divider" />

            <!-- Section 2: Reflection & Evaluation -->
            <div class="form-section">
              <h3 class="form-section-title">Projektrefleksion</h3>

              <div class="form-group">
                <label class="form-label" for="edit-eval">Evaluering</label>
                <textarea
                  id="edit-eval"
                  v-model="form.evaluation"
                  class="form-input"
                  rows="5"
                  placeholder="Beskriv forløbet, udfordringer og hvad du lærte..."
                  :disabled="isSaving"
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="edit-conclusion">Konklusion</label>
                <textarea
                  id="edit-conclusion"
                  v-model="form.conclusion"
                  class="form-input"
                  rows="5"
                  placeholder="Hvad var resultatet af projektet?"
                  :disabled="isSaving"
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="edit-persp">Perspektivering</label>
                <textarea
                  id="edit-persp"
                  v-model="form.perspektivering"
                  class="form-input"
                  rows="5"
                  placeholder="Hvordan kan projektet videreudvikles eller bruges fremover?"
                  :disabled="isSaving"
                />
              </div>
            </div>

            <!-- Form Actions -->
            <div class="form-actions-bar">
              <div class="actions-left">
                <button
                  class="secondary-btn"
                  type="button"
                  :disabled="isSaving"
                  @click="cancelEdit"
                >
                  Annuller
                </button>
                <button
                  class="primary-btn"
                  type="submit"
                  :disabled="isSaving"
                >
                  <IconCheck :size="15" :stroke-width="2.5" />
                  <span>{{ isSaving ? "Gemmer…" : "Gem ændringer" }}</span>
                </button>
              </div>

              <button
                v-if="canSubmit"
                class="submit-action-btn"
                type="button"
                :disabled="isSaving || isSubmitting"
                @click="submitProject"
              >
                <IconFolderOpen :size="15" :stroke-width="2.2" />
                <span>Aflever projekt</span>
              </button>
            </div>
          </form>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.top-nav {
  display: flex;
  align-items: center;
}

.back-link-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: none;
  border: none;
  padding: 4px 0;
  color: #64748b;
  font-size: 13.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: color 0.15s ease;
}

.back-link-btn:hover {
  color: #016bff;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 22px;
}

.skeleton-surface {
  height: 480px;
  background: linear-gradient(90deg, #e5eaed 25%, #eff2f4 50%, #e5eaed 75%);
  background-size: 400% 100%;
  animation: shimmer 1.4s ease infinite;
  border-radius: 12px;
}

@keyframes shimmer {
  0% {
    background-position: 100% 0;
  }
  100% {
    background-position: -100% 0;
  }
}

/* Notice & Feedback Banners */
.notice-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  background-color: #f8fafc;
  border-color: #e2e8f0;
  color: #475569;
  font-size: 13.5px;
  font-weight: 500;
  padding: 14px 18px;
}

.notice-banner.info {
  background-color: #eff6ff;
  border-color: #bfdbfe;
  color: #1e40af;
}

.notice-icon {
  flex-shrink: 0;
}

.feedback-banner {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background-color: #eff6ff;
  border-color: #bfdbfe;
  padding: 16px 20px;
}

.feedback-banner-header {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  font-size: 12px;
  font-weight: 700;
  color: #1d4ed8;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.feedback-banner-text {
  margin: 0;
  font-size: 14px;
  line-height: 1.6;
  color: #1e3a8a;
  white-space: pre-wrap;
}

.feedback-toast {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  align-self: flex-start;
}

.feedback-toast.success {
  background-color: #ecfdf5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.feedback-toast.error {
  background-color: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

/* Hero Section */
.project-view-layout {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.project-hero-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 24px;
}

.hero-top-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.hero-badges {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.category-tag,
.type-tag,
.haul-tag {
  font-size: 11.5px;
  font-weight: 500;
  padding: 2px 8px;
  border-radius: 6px;
  white-space: nowrap;
}

.category-tag {
  color: #475569;
  background-color: #f1f5f9;
  border: 1px solid #e2e8f0;
}

.type-tag {
  color: #0369a1;
  background-color: #f0f9ff;
  border: 1px solid #bae6fd;
}

.haul-tag {
  color: #4338ca;
  background-color: #eef2ff;
  border: 1px solid #c7d2fe;
}

.hero-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.outline-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 13px;
  background-color: #ffffff;
  color: #334155;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: all 0.15s ease;
}

.outline-btn:hover {
  background-color: #f8fafc;
  color: #0f172a;
  border-color: #94a3b8;
}

.submit-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 14px;
  background-color: #059669;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.submit-action-btn:hover:not(:disabled) {
  background-color: #047857;
}

.submit-action-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.project-hero-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.4px;
  line-height: 1.3;
}

.project-hero-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.hero-meta-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13.5px;
  color: #64748b;
}

.meta-icon {
  color: #94a3b8;
}

/* Two-column overview */
.overview-grid {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 18px;
}

@media (max-width: 860px) {
  .overview-grid {
    grid-template-columns: 1fr;
  }
}

.content-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-label {
  font-size: 11.5px;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.description-body {
  margin: 0;
  font-size: 14.5px;
  line-height: 1.65;
  color: #334155;
  white-space: pre-wrap;
}

.info-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 0;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  font-size: 13px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f1f5f9;
}

.info-row:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.info-key {
  color: #64748b;
  font-weight: 500;
}

.info-val {
  margin: 0;
  color: #111827;
  font-weight: 600;
  text-align: right;
}

.repo-link-inline {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: #016bff;
  text-decoration: none;
  font-weight: 600;
}

.repo-link-inline:hover {
  text-decoration: underline;
}

.text-muted {
  color: #94a3b8;
  font-weight: 400;
}

/* Reflection Section */
.reflection-card {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.section-header-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.section-heading {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.2px;
}

.section-sub {
  margin: 2px 0 0;
  font-size: 13px;
  color: #64748b;
}

.edit-sub-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  background: none;
  border: 1px solid #e2e8f0;
  border-radius: 7px;
  padding: 4px 10px;
  color: #475569;
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s ease;
}

.edit-sub-btn:hover {
  background-color: #f8fafc;
  color: #016bff;
  border-color: #cbd5e1;
}

.reflection-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

@media (max-width: 900px) {
  .reflection-grid {
    grid-template-columns: 1fr;
  }
}

.reflection-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background-color: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
}

.reflection-header {
  display: flex;
  align-items: center;
  gap: 6px;
}

.reflection-icon {
  color: #64748b;
}

.reflection-title {
  margin: 0;
  font-size: 13.5px;
  font-weight: 700;
  color: #1e293b;
}

.reflection-text {
  margin: 0;
  font-size: 13.5px;
  line-height: 1.6;
  color: #334155;
  white-space: pre-wrap;
}

.text-placeholder {
  color: #94a3b8;
  font-style: italic;
}

/* Instructor Card */
.instructor-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.instructor-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.instructor-actions-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.stage-btns {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.stage-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 13px;
  border-radius: 8px;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: all 0.15s ease;
}

.stage-action-btn.approve {
  background-color: #eff6ff;
  color: #1d4ed8;
  border: 1px solid #bfdbfe;
}

.stage-action-btn.approve:hover:not(:disabled) {
  background-color: #dbeafe;
}

.stage-action-btn.evaluate {
  background-color: #ecfdf5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.stage-action-btn.evaluate:hover:not(:disabled) {
  background-color: #d1fae5;
}

.stage-action-btn.back {
  background-color: #ffffff;
  color: #475569;
  border: 1px solid #cbd5e1;
}

.stage-action-btn.back:hover:not(:disabled) {
  background-color: #f1f5f9;
}

.stage-action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.stage-error {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  color: #dc2626;
  font-weight: 600;
}

.feedback-field-wrap {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 10px;
  border-top: 1px solid #f1f5f9;
}

.feedback-footer {
  display: flex;
  align-items: center;
  gap: 12px;
}

.save-status-msg {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12.5px;
  font-weight: 600;
}

.save-status-msg.success {
  color: #059669;
}

.save-status-msg.error {
  color: #dc2626;
}

/* Edit Mode Layout */
.edit-layout {
  display: flex;
  flex-direction: column;
}

.edit-card {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.edit-header {
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 14px;
}

.edit-title {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: #111827;
}

.edit-sub {
  margin: 4px 0 0;
  font-size: 13.5px;
  color: #64748b;
}

.edit-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-section-title {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: #334155;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 13px;
  font-weight: 600;
  color: #334155;
}

.form-input {
  min-height: 38px;
  background-color: #ffffff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 8px 12px;
  font-family: inherit;
  font-size: 13.5px;
  color: #111827;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.form-input:focus {
  border-color: #016bff;
  box-shadow: 0 0 0 3px rgba(1, 107, 255, 0.15);
}

textarea.form-input {
  resize: vertical;
  line-height: 1.55;
}

.field-hint {
  font-size: 12px;
  color: #64748b;
}

.form-divider {
  height: 1px;
  background-color: #e2e8f0;
}

.form-actions-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  padding-top: 8px;
  border-top: 1px solid #f1f5f9;
}

.actions-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.primary-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 38px;
  padding: 0 16px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.15s ease;
}

.primary-btn:hover:not(:disabled) {
  background-color: #005ae0;
}

.primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.secondary-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: 38px;
  padding: 0 16px;
  background-color: #f1f5f9;
  color: #334155;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.15s ease;
}

.secondary-btn:hover:not(:disabled) {
  background-color: #e2e8f0;
}

.not-found-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 10px;
  padding: 48px 24px;
}
</style>