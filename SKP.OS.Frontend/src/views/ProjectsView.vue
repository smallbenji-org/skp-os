<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import {
  IconPlus,
  IconExternalLink,
  IconUsers,
  IconArrowRight,
} from "@tabler/icons-vue";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useProjectTemplateStore } from "@/Stores/ProjectTemplateStore";
import ProjectStageBadge from "@/components/ProjectStageBadge.vue";

const router = useRouter();
const projectStore = useProjectStore();
const studentProfileStore = useStudentProfileStore();
const projectTemplateStore = useProjectTemplateStore();

const isLoading = ref(true);

const myProfile = computed(() => studentProfileStore.MY_STUDENT_PROFILE);

const myProjects = computed(() =>
  projectStore.PROJECTS.filter((p) =>
    p.students.some((s) => s.id === myProfile.value?.id),
  ).sort((a, b) => a.title.localeCompare(b.title, "da")),
);

const hasTemplates = computed(
  () => projectTemplateStore.PROJECT_TEMPLATES.length > 0,
);

function studentNames(students: { user: { name: string } | null }[]): string {
  const names = students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(", ") : "Ingen elever tilknyttet";
}

function openRepo(url: string) {
  const target = url.trim();
  if (!/^https?:\/\//i.test(target)) return;
  window.open(target, "_blank", "noopener,noreferrer");
}

function goToTemplates() {
  router.push({ name: "skp-projekter" });
}

function openProject(projectId: number) {
  router.push({ name: "projekt-detalje", params: { id: projectId } });
}

onMounted(async () => {
  try {
    await studentProfileStore.GET_MY_STUDENT_PROFILE();
    await Promise.all([
      projectStore.GET_PROJECTS(),
      projectTemplateStore.GET_PROJECT_TEMPLATES(),
    ]);
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <div class="page-header-row">
        <div class="page-header-text">
          <h1 class="page-title">Mine Projekter</h1>
        </div>
        <button
          v-if="hasTemplates"
          class="new-project-btn"
          type="button"
          @click="goToTemplates"
        >
          <IconPlus :size="16" :stroke-width="2.5" />
          Nyt projekt fra skabelon
        </button>
      </div>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="myProjects.length === 0" class="surface empty">
      <h2 class="empty-title">Du har ingen projekter endnu</h2>
      <p class="empty-text">
        Vælg en SKP-skabelon for at oprette dit eget projekt, eller spørg din
        underviser hvis du mener der mangler noget.
      </p>
      <button class="empty-btn" type="button" @click="goToTemplates">
        <IconPlus :size="15" :stroke-width="2.5" />
        Vis SKP-skabeloner
      </button>
    </div>

    <div v-else class="project-grid">
      <article v-for="project in myProjects" :key="project.id" class="surface project-card">
        <div class="project-heading">
          <h2 class="project-title">{{ project.title }}</h2>
          <ProjectStageBadge :stage="project.stage" />
          <span
            v-if="project.projectTemplate || project.isCustomProject"
            class="template-badge"
          >
            {{ project.isCustomProject ? "Eget projekt" : project.projectTemplate?.title }}
          </span>
        </div>
        <p class="project-desc">{{ project.shortDescription || "Ingen beskrivelse." }}</p>
        <div class="project-meta">
          <span class="meta-students">
            <IconUsers :size="16" :stroke-width="2" />
            {{ studentNames(project.students) }}
          </span>
          <div class="meta-actions">
            <button
              v-if="project.gitRepoUrl && /^https?:/i.test(project.gitRepoUrl.trim())"
              type="button"
              class="repo-link"
              :title="project.gitRepoUrl"
              @click="openRepo(project.gitRepoUrl)"
            >
              <IconExternalLink :size="16" :stroke-width="2" />
              Repo
            </button>
            <button
              type="button"
              class="open-btn"
              @click="openProject(project.id)"
            >
              Åbn projekt
              <IconArrowRight :size="14" :stroke-width="2" />
            </button>
          </div>
        </div>
      </article>
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

.page-header-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.page-header-text {
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


.new-project-btn {
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

.new-project-btn:hover {
  background-color: #005ae0;
}

.new-project-btn:active {
  transform: scale(0.98);
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

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 14px;
}

.project-card {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.project-heading {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.project-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
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

.project-desc {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
  color: #4b5563;
}

.project-meta {
  margin-top: auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  border-top: 1px solid #f3f4f6;
  padding-top: 12px;
}

.meta-students {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #6b7280;
  min-width: 0;
}

.meta-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.open-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  font-weight: 600;
  color: #ffffff;
  background: #016bff;
  border: none;
  cursor: pointer;
  padding: 5px 10px;
  border-radius: 7px;
  font-family: inherit;
  transition: background-color 0.2s ease;
}

.open-btn:hover {
  background: #005ae0;
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

.empty-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  align-self: flex-start;
  height: 36px;
  padding: 0 14px;
  margin-top: 6px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.2s ease;
}

.empty-btn:hover {
  background-color: #005ae0;
}
</style>