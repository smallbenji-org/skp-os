<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { IconExternalLink, IconPlus, IconCheck, IconAlertTriangle } from "@tabler/icons-vue";
import { useProjectTemplateStore } from "@/Stores/ProjectTemplateStore";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useAuthStore } from "@/Stores/AuthStore";
import type { ProjectTemplateDto } from "@/types";

const projectTemplateStore = useProjectTemplateStore();
const projectStore = useProjectStore();
const authStore = useAuthStore();

const isLoading = ref(true);
const creatingId = ref<number | null>(null);
const feedback = ref<"idle" | "success" | "error">("idle");
const feedbackTemplate = ref<string>("");

const isStudent = computed(() => authStore.HAS_ROLE("Student"));

const templates = computed(() =>
  [...projectTemplateStore.PROJECT_TEMPLATES].sort((a, b) =>
    a.haul.localeCompare(b.haul, "da") || a.title.localeCompare(b.title, "da"),
  ),
);

function templateTypeLabel(template: ProjectTemplateDto): string {
  const labels: Record<string, string> = {
    Hf1: "HF1",
    Hf2: "HF2",
    Hf3: "HF3",
  };
  return labels[template.haul] ?? template.haul;
}

function openRepo(url: string) {
  const target = url.trim();
  if (!/^https?:\/\//i.test(target)) return;
  window.open(target, "_blank", "noopener,noreferrer");
}

async function createFromTemplate(template: ProjectTemplateDto) {
  if (creatingId.value != null) return;
  creatingId.value = template.id;
  feedback.value = "idle";
  feedbackTemplate.value = "";
  const created = await projectStore.CREATE_PROJECT_FROM_TEMPLATE(template.id);
  creatingId.value = null;
  if (created) {
    feedback.value = "success";
  } else {
    feedback.value = "error";
    feedbackTemplate.value = template.title;
  }
  window.setTimeout(() => {
    feedback.value = "idle";
  }, 4000);
}

onMounted(async () => {
  try {
    await projectTemplateStore.GET_PROJECT_TEMPLATES();
    await projectStore.GET_PROJECTS();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">SKP Projekter</h1>
      <p class="page-sub">
        Vælg en skabelon og opret dit eget projekt ud fra den
      </p>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="templates.length === 0" class="surface empty">
      <h2 class="empty-title">Ingen skabeloner endnu</h2>
      <p class="empty-text">
        Der er ikke oprettet nogen SKP-skabeloner endnu. Spørg din underviser
        hvis du mener der mangler noget.
      </p>
    </div>

    <div v-else class="template-grid">
      <article
        v-for="template in templates"
        :key="template.id"
        class="surface template-card"
      >
        <div class="template-heading">
          <span class="haul-badge">{{ templateTypeLabel(template) }}</span>
          <h2 class="template-title">{{ template.title }}</h2>
        </div>
        <p class="template-desc">{{ template.shortDescription || "Ingen beskrivelse." }}</p>
        <div class="template-meta">
          <span class="meta-type">{{ template.studentType }}</span>
          <button
            v-if="template.gitRepoUrl && /^https?:/i.test(template.gitRepoUrl.trim())"
            type="button"
            class="repo-link"
            :title="template.gitRepoUrl"
            @click="openRepo(template.gitRepoUrl)"
          >
            <IconExternalLink :size="15" :stroke-width="2" />
            Repo
          </button>
        </div>
        <button
          v-if="isStudent"
          class="create-btn"
          type="button"
          :disabled="creatingId != null"
          @click="createFromTemplate(template)"
        >
          <IconPlus v-if="creatingId !== template.id" :size="15" :stroke-width="2.5" />
          <span v-else class="spinner" />
          {{ creatingId === template.id ? "Opretter…" : "Opret projekt" }}
        </button>
      </article>
    </div>

    <span
      v-if="feedback === 'success'"
      class="save-feedback success"
    >
      <IconCheck :size="13" :stroke-width="2.5" />
      Projektet er oprettet og ligger nu under "Mine Projekter".
    </span>
    <span
      v-else-if="feedback === 'error'"
      class="save-feedback error"
    >
      <IconAlertTriangle :size="13" :stroke-width="2" />
      Projektet kunne ikke oprettes. Prøv igen.
    </span>
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
  font-size: 14px;
  color: #6b7280;
  margin: 0;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
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

.template-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 14px;
}

.template-card {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.template-heading {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  flex-wrap: wrap;
}

.haul-badge {
  font-size: 11px;
  font-weight: 700;
  color: #2563eb;
  background: #eef2ff;
  border-radius: 999px;
  padding: 2px 10px;
  white-space: nowrap;
}

.template-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.template-desc {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
  color: #4b5563;
}

.template-meta {
  margin-top: auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  border-top: 1px solid #f3f4f6;
  padding-top: 12px;
}

.meta-type {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #6b7280;
  min-width: 0;
}

.repo-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  flex-shrink: 0;
  font-size: 13px;
  font-weight: 600;
  color: #2563eb;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px 6px;
  border-radius: 6px;
}

.repo-link:hover {
  background: #eef2ff;
}

.create-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  height: 38px;
  padding: 0 16px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 10px;
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

.spinner {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.save-feedback {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  font-weight: 600;
}

.save-feedback.success {
  color: #059669;
}

.save-feedback.error {
  color: #dc2626;
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