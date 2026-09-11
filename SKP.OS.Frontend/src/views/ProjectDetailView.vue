<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
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

const projectId = Number(route.params.id);

const isLoading = ref(true);
const notFound = ref(false);
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

const showStudentForm = computed(
  () => isStudent.value && isMyProject(),
);

const showReview = computed(
  () => isInstructor.value || (isStudent.value && !isMyProject()),
);

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
  await projectStore.GET_PROJECT(projectId);
  if (projectStore.SELECTED_PROJECT == null) {
    notFound.value = true;
  } else {
    applyProject();
  }
}

async function saveProject() {
  if (!project.value || isSaving.value) return;
  isSaving.value = true;
  saveFeedback.value = "idle";
  const updated = await projectStore.UPDATE_PROJECT(project.value.id, { ...form });
  isSaving.value = false;
  saveFeedback.value = updated ? "success" : "error";
  if (updated) applyProject();
  window.setTimeout(() => {
    saveFeedback.value = "idle";
  }, 3000);
}

async function submitProject() {
  if (!project.value || isSubmitting.value) return;
  isSubmitting.value = true;
  submitFeedback.value = "idle";
  const updated = await projectStore.SUBMIT_PROJECT(project.value.id, { ...form });
  isSubmitting.value = false;
  submitFeedback.value = updated ? "success" : "error";
  if (updated) applyProject();
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

function openRepo() {
  const url = project.value?.gitRepoUrl?.trim() ?? "";
  if (!/^https?:\/\//i.test(url)) return;
  window.open(url, "_blank", "noopener,noreferrer");
}

function studentNames(): string {
  if (!project.value) return "";
  const names = project.value.students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(", ") : "Ingen elever tilknyttet";
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
    if (isStudent.value) {
      await studentProfileStore.GET_MY_STUDENT_PROFILE();
    }
    await load();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <button class="back-btn" type="button" @click="goBack">
      <IconArrowLeft :size="16" :stroke-width="2" />
      {{ isInstructor ? "Tilbage til underviser" : "Tilbage til projekter" }}
    </button>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else-if="notFound">
      <div class="surface empty">
        <h2 class="empty-title">Projektet blev ikke fundet</h2>
        <p class="empty-text">Det er måske blevet slettet.</p>
      </div>
    </template>

    <template v-else-if="project">
      <div class="surface details-card">
        <div class="detail-header">
          <div class="detail-heading">
            <div class="detail-badges">
              <ProjectStageBadge :stage="project.stage" />
              <span
                v-if="project.projectTemplate || project.isCustomProject"
                class="template-badge"
              >
                {{ project.isCustomProject ? "Eget projekt" : project.projectTemplate?.title }}
              </span>
            </div>
            <h1 class="detail-title">{{ project.title }}</h1>
            <div class="detail-meta">
              <span class="meta-item">
                <IconUsers :size="15" :stroke-width="2" />
                {{ studentNames() }}
              </span>
            </div>
          </div>

          <button
            v-if="project.gitRepoUrl && /^https?:/i.test(project.gitRepoUrl.trim())"
            class="repo-btn"
            type="button"
            @click="openRepo"
          >
            <IconExternalLink :size="16" :stroke-width="2" />
            Åbn Git repository
          </button>
        </div>
      </div>

      <div v-if="locked" class="surface notice">
        <IconCheck :size="16" :stroke-width="2.5" />
        <span>
          Projektet er afleveret og låst. Det kan ikke redigeres længere.
        </span>
      </div>

      <div v-if="showStudentForm && project.feedback" class="surface feedback-banner">
        <div class="feedback-banner-head">
          <IconMessage :size="16" :stroke-width="2.5" />
          Feedback fra underviser
        </div>
        <p class="feedback-banner-text">{{ project.feedback }}</p>
      </div>

      <div v-if="showReview" class="surface review-card">
          <div class="section-heading">Gennemgang af projekt</div>

          <div class="review-row">
            <div class="review-label">Kort beskrivelse</div>
            <p class="review-text">{{ project.shortDescription || "Ingen beskrivelse." }}</p>
          </div>
          <div class="review-row">
            <div class="review-label">Evaluering</div>
            <p class="review-text">{{ project.evaluation || "Ikke udfyldt." }}</p>
          </div>
          <div class="review-row">
            <div class="review-label">Konklusion</div>
            <p class="review-text">{{ project.conclusion || "Ikke udfyldt." }}</p>
          </div>
          <div class="review-row">
            <div class="review-label">Perspektivering</div>
            <p class="review-text">{{ project.perspektivering || "Ikke udfyldt." }}</p>
          </div>
        </div>

        <div v-if="canGiveFeedback" class="surface feedback-card">
          <div class="section-heading">Feedback til elever</div>
          <textarea
            v-model="feedbackDraft"
            class="form-input"
            rows="5"
            placeholder="Skriv feedback til eleverne her..."
            :disabled="isSaving"
          />
          <div class="feedback-actions">
            <button
              class="create-btn"
              type="button"
              :disabled="feedbackSaveFeedback === 'saving'"
              @click="saveInstructorFeedback"
            >
              <IconCheck :size="15" :stroke-width="2.5" />
              Gem feedback
            </button>
            <span v-if="feedbackSaveFeedback === 'success'" class="save-feedback success">
              <IconCheck :size="13" :stroke-width="2.5" />
              Feedback gemt
            </span>
            <span v-else-if="feedbackSaveFeedback === 'error'" class="save-feedback error">
              <IconAlertTriangle :size="13" :stroke-width="2" />
              Kunne ikke gemme feedback
            </span>
          </div>
        </div>

        <div v-if="isInstructor" class="surface stage-card">
          <div class="stage-card-header">
            <div class="section-heading">Stadie</div>
            <ProjectStageBadge :stage="project.stage" />
          </div>
          <div class="stage-actions">
            <button
              v-if="project.stage === 'Created'"
              class="stage-btn approve"
              type="button"
              :disabled="stageBusy"
              @click="changeStage('Approved')"
            >
              <IconCheck :size="14" :stroke-width="2.5" />
              Godkend
            </button>
            <button
              v-if="project.stage === 'Submitted'"
              class="stage-btn evaluate"
              type="button"
              :disabled="stageBusy"
              @click="changeStage('Evaluated')"
            >
              <IconCheck :size="14" :stroke-width="2.5" />
              Evaluér
            </button>
            <button
              v-if="project.stage !== 'Created'"
              class="stage-btn back"
              type="button"
              :disabled="stageBusy"
              @click="changeStage(previousStage(project.stage)!)"
            >
              <IconX :size="14" :stroke-width="2" />
              Fortryd
            </button>
            <span v-if="stageError" class="stage-error">
              <IconAlertTriangle :size="13" :stroke-width="2" />
              {{ stageError }}
            </span>
          </div>
        </div>

        <div v-if="showStudentForm" class="surface form-card">
          <div class="section-heading">Projektoplysninger</div>

          <div class="form-field">
            <label class="form-label" for="detail-title">Titel</label>
            <input
              id="detail-title"
              v-model="form.title"
              class="form-input"
              type="text"
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="form-field">
            <label class="form-label" for="detail-desc">Kort beskrivelse</label>
            <input
              id="detail-desc"
              v-model="form.shortDescription"
              class="form-input"
              type="text"
              placeholder="Hvad handler projektet om?"
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="form-field">
            <label class="form-label" for="detail-repo">Git repository URL</label>
            <input
              id="detail-repo"
              v-model="form.gitRepoUrl"
              class="form-input"
              type="text"
              placeholder="https://github.com/..."
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="divider" />

          <div class="form-field">
            <label class="form-label" for="detail-eval">Evaluering</label>
            <textarea
              id="detail-eval"
              v-model="form.evaluation"
              class="form-input"
              rows="5"
              placeholder="Skriv din evaluering her..."
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="form-field">
            <label class="form-label" for="detail-conclusion">Konklusion</label>
            <textarea
              id="detail-conclusion"
              v-model="form.conclusion"
              class="form-input"
              rows="5"
              placeholder="Skriv din konklusion her..."
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="form-field">
            <label class="form-label" for="detail-persp">Perspektivering</label>
            <textarea
              id="detail-persp"
              v-model="form.perspektivering"
              class="form-input"
              rows="5"
              placeholder="Skriv din perspektivering her..."
              :disabled="locked || !canEdit"
            />
          </div>

          <div class="form-actions">
            <button
              class="create-btn"
              type="button"
              :disabled="isSaving || locked || !canEdit"
              @click="saveProject"
            >
              <IconCheck :size="15" :stroke-width="2.5" />
              Gem ændringer
            </button>

            <button
              v-if="canSubmit"
              class="submit-btn"
              type="button"
              :disabled="isSubmitting"
              @click="submitProject"
            >
              <IconFolderOpen :size="15" :stroke-width="2.5" />
              Aflever projekt
            </button>

            <span v-if="saveFeedback === 'success'" class="save-feedback success">
              <IconCheck :size="13" :stroke-width="2.5" />
              Gemt
            </span>
            <span v-else-if="saveFeedback === 'error'" class="save-feedback error">
              <IconAlertTriangle :size="13" :stroke-width="2" />
              Kunne ikke gemme
            </span>

            <span v-if="submitFeedback === 'success'" class="save-feedback success">
              <IconCheck :size="13" :stroke-width="2.5" />
              Projektet er afleveret
            </span>
            <span v-else-if="submitFeedback === 'error'" class="save-feedback error">
              <IconAlertTriangle :size="13" :stroke-width="2" />
              Projektet kunne ikke afleveres
            </span>

            <span v-if="canEdit && !canSubmit && project.stage === 'Created'" class="hint">
              Projektet skal godkendes af en underviser, før det kan afleveres.
            </span>
          </div>
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

.back-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  align-self: flex-start;
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

.back-btn:hover {
  background: #f8fafc;
  border-color: #016bff;
  color: #016bff;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
}

.skeleton-surface {
  height: 360px;
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

.details-card {
  padding: 22px;
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  flex-wrap: wrap;
}

.detail-heading {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.detail-badges {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.template-badge {
  font-size: 11.5px;
  font-weight: 600;
  color: #2563eb;
  background: #eef2ff;
  border-radius: 999px;
  padding: 2px 10px;
  white-space: nowrap;
}

.detail-title {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.5px;
}

.detail-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.meta-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #6b7280;
}

.repo-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  height: 40px;
  padding: 0 16px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 13.5px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  flex-shrink: 0;
  transition: background-color 0.2s ease;
}

.repo-btn:hover {
  background-color: #005ae0;
}

.notice {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fffbeb;
  border-color: #fcd34d;
  color: #92400e;
  font-size: 13.5px;
  font-weight: 600;
}

.review-card,
.form-card,
.stage-card,
.feedback-card {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding: 22px;
}

.feedback-banner {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: #eff6ff;
  border-color: #93c5fd;
}

.feedback-banner-head {
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
  font-size: 13.5px;
  line-height: 1.55;
  color: #1e3a8a;
  white-space: pre-wrap;
}

.feedback-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.section-heading {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.review-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.review-label {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
}

.review-text {
  margin: 0;
  font-size: 14px;
  line-height: 1.6;
  color: #111827;
  white-space: pre-wrap;
}

.stage-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.stage-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.stage-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 34px;
  padding: 0 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 9px;
  background: #ffffff;
  color: #475569;
  font-size: 12.5px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  white-space: nowrap;
  transition: background-color 0.2s ease, color 0.2s ease, border-color 0.2s ease;
}

.stage-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.stage-btn.approve {
  border-color: #016bff;
  color: #016bff;
}

.stage-btn.approve:hover:not(:disabled) {
  background: #eff6ff;
}

.stage-btn.evaluate {
  border-color: #059669;
  color: #059669;
}

.stage-btn.evaluate:hover:not(:disabled) {
  background: #ecfdf5;
}

.stage-btn.back:hover:not(:disabled) {
  background: #f8fafc;
  color: #374151;
}

.stage-error {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  font-weight: 600;
  color: #dc2626;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #374151;
}

.form-input {
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

textarea.form-input {
  height: auto;
  padding-top: 10px;
  padding-bottom: 10px;
  resize: vertical;
}

.form-input::placeholder {
  color: #9ca3af;
}

.form-input:focus {
  border-color: #016bff;
  background-color: #ffffff;
}

.form-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.divider {
  height: 1px;
  background: #f3f4f6;
}

.form-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  margin-top: 6px;
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

.create-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.submit-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  height: 40px;
  padding: 0 18px;
  background-color: #059669;
  color: #ffffff;
  border: none;
  border-radius: 11px;
  font-size: 13.5px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.2s ease, transform 0.15s ease;
}

.submit-btn:hover:not(:disabled) {
  background-color: #047857;
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.save-feedback {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  font-weight: 600;
}

.save-feedback.success {
  color: #059669;
}

.save-feedback.error {
  color: #dc2626;
}

.hint {
  font-size: 12.5px;
  color: #6b7280;
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