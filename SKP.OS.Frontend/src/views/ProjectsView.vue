<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import {
  IconPlus,
  IconExternalLink,
  IconUsers,
  IconArrowRight,
  IconFolderOpen,
} from "@tabler/icons-vue";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useProjectTemplateStore } from "@/Stores/ProjectTemplateStore";
import ProjectStageBadge from "@/components/ProjectStageBadge.vue";
import type { ProjectStage } from "@/types";

const router = useRouter();
const projectStore = useProjectStore();
const studentProfileStore = useStudentProfileStore();
const projectTemplateStore = useProjectTemplateStore();

const isLoading = ref(true);
type FilterKey = "all" | ProjectStage;
const activeFilter = ref<FilterKey>("all");

const myProfile = computed(() => studentProfileStore.MY_STUDENT_PROFILE);

const myProjects = computed(() =>
  projectStore.PROJECTS.filter((p) =>
    p.students.some((s) => s.id === myProfile.value?.id),
  ).sort((a, b) => a.title.localeCompare(b.title, "da")),
);

const filterTabs: { id: FilterKey; label: string }[] = [
  { id: "all", label: "Alle" },
  { id: "Approved", label: "I gang" },
  { id: "Created", label: "Oprettet" },
  { id: "Submitted", label: "Afventer evaluering" },
  { id: "Evaluated", label: "Afsluttet" },
];

function getStageCount(key: FilterKey): number {
  if (key === "all") return myProjects.value.length;
  return myProjects.value.filter((p) => p.stage === key).length;
}

const filteredProjects = computed(() => {
  if (activeFilter.value === "all") return myProjects.value;
  return myProjects.value.filter((p) => p.stage === activeFilter.value);
});

const hasTemplates = computed(
  () => projectTemplateStore.PROJECT_TEMPLATES.length > 0,
);

function studentNames(students: { user: { name: string } | null }[]): string {
  const names = students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(" · ") : "Ingen elever tilknyttet";
}

function isValidUrl(url?: string | null): boolean {
  if (!url) return false;
  return /^https?:\/\//i.test(url.trim());
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
    <header class="page-header">
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
    </header>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="myProjects.length === 0" class="surface empty-card">
      <div class="empty-icon-wrap">
        <IconFolderOpen :size="32" :stroke-width="1.8" />
      </div>
      <h2 class="empty-title">Ingen projekter endnu</h2>
      <p class="empty-text">
        Når du opretter eller bliver tilknyttet et projekt, vises det her. Du kan
        komme i gang ved at vælge en SKP-skabelon.
      </p>
      <button class="empty-btn" type="button" @click="goToTemplates">
        <IconPlus :size="15" :stroke-width="2.5" />
        Vis SKP-skabeloner
      </button>
    </div>

    <div v-else class="projects-content">
      <nav class="filter-bar" aria-label="Projektfiltre">
        <button
          v-for="tab in filterTabs"
          :key="tab.id"
          type="button"
          class="filter-pill"
          :class="{ active: activeFilter === tab.id }"
          @click="activeFilter = tab.id"
        >
          <span>{{ tab.label }}</span>
          <span class="filter-count">{{ getStageCount(tab.id) }}</span>
        </button>
      </nav>

      <div v-if="filteredProjects.length === 0" class="surface empty-filter-card">
        <h3 class="empty-title">Ingen projekter matcher filteret</h3>
        <p class="empty-text">
          Der er ingen projekter med denne status lige nu.
        </p>
        <button
          class="reset-filter-btn"
          type="button"
          @click="activeFilter = 'all'"
        >
          Vis alle projekter
        </button>
      </div>

      <div v-else class="project-grid">
        <article
          v-for="project in filteredProjects"
          :key="project.id"
          class="surface project-card"
          tabindex="0"
          role="button"
          :aria-label="`Åbn projekt: ${project.title}`"
          @click="openProject(project.id)"
          @keydown.enter="openProject(project.id)"
          @keydown.space.prevent="openProject(project.id)"
        >
          <div class="card-top">
            <div class="card-badges">
              <ProjectStageBadge :stage="project.stage" size="sm" />
              <span
                v-if="project.projectTemplate || project.isCustomProject"
                class="template-tag"
              >
                {{
                  project.isCustomProject
                    ? "Eget projekt"
                    : project.projectTemplate?.title
                }}
              </span>
            </div>
            <h2 class="card-title">{{ project.title }}</h2>
          </div>

          <p class="card-desc">
            {{ project.shortDescription || "Ingen kort beskrivelse angivet." }}
          </p>

          <div class="card-meta">
            <div class="meta-row">
              <IconUsers :size="15" :stroke-width="2" class="meta-icon" />
              <span class="meta-text" :title="studentNames(project.students)">
                {{ studentNames(project.students) }}
              </span>
            </div>
          </div>

          <div class="card-footer">
            <div class="footer-left">
              <button
                v-if="isValidUrl(project.gitRepoUrl)"
                type="button"
                class="repo-btn"
                title="Åbn Git repository"
                @click.stop="openRepo(project.gitRepoUrl)"
              >
                <IconExternalLink :size="14" :stroke-width="2" />
                Repo
              </button>
            </div>
            <span class="open-action">
              <span>Åbn projekt</span>
              <IconArrowRight :size="14" :stroke-width="2.2" class="arrow-icon" />
            </span>
          </div>
        </article>
      </div>
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

.page-subtitle {
  font-size: 14px;
  color: #6b7280;
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
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.2s ease, transform 0.15s ease;
  white-space: nowrap;
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
  height: 440px;
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

.projects-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.filter-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 32px;
  padding: 0 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background-color: #ffffff;
  color: #4b5563;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s ease;
  user-select: none;
}

.filter-pill:hover:not(.active) {
  background-color: #f8fafc;
  color: #111827;
  border-color: #cbd5e1;
}

.filter-pill.active {
  background-color: #eff6ff;
  border-color: #bfdbfe;
  color: #1d4ed8;
  font-weight: 600;
}

.filter-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 999px;
  background-color: #f1f5f9;
  font-size: 11px;
  font-weight: 600;
  color: #64748b;
}

.filter-pill.active .filter-count {
  background-color: #dbeafe;
  color: #1e40af;
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 16px;
}

.project-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 18px 20px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  outline: none;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.15s ease;
}

.project-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.05);
  transform: translateY(-1px);
}

.project-card:focus-visible {
  border-color: #016bff;
  box-shadow: 0 0 0 3px rgba(1, 107, 255, 0.15);
}

.card-top {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.card-badges {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.template-tag {
  font-size: 11.5px;
  font-weight: 500;
  color: #475569;
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 1.5px 8px;
  white-space: nowrap;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
  line-height: 1.35;
  letter-spacing: -0.2px;
}

.card-desc {
  margin: 0;
  font-size: 13.5px;
  line-height: 1.55;
  color: #4b5563;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-meta {
  margin-top: auto;
  padding-top: 4px;
}

.meta-row {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #6b7280;
}

.meta-icon {
  flex-shrink: 0;
  color: #9ca3af;
}

.meta-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  border-top: 1px solid #f1f5f9;
  padding-top: 12px;
  margin-top: 4px;
}

.footer-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.repo-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12.5px;
  font-weight: 600;
  color: #475569;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  padding: 3px 8px;
  border-radius: 6px;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s ease;
}

.repo-btn:hover {
  background: #eff6ff;
  color: #1d4ed8;
  border-color: #bfdbfe;
}

.open-action {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  font-weight: 600;
  color: #016bff;
  transition: gap 0.15s ease;
}

.project-card:hover .open-action {
  gap: 7px;
}

.arrow-icon {
  transition: transform 0.15s ease;
}

.project-card:hover .arrow-icon {
  transform: translateX(2px);
}

.empty-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 48px 24px;
  gap: 8px;
}

.empty-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: 14px;
  background-color: #f1f5f9;
  color: #64748b;
  margin-bottom: 8px;
}

.empty-title {
  margin: 0;
  font-size: 17px;
  font-weight: 600;
  color: #111827;
}

.empty-text {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
  max-width: 440px;
  line-height: 1.5;
}

.empty-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  height: 38px;
  padding: 0 16px;
  margin-top: 12px;
  background-color: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.2s ease;
}

.empty-btn:hover {
  background-color: #005ae0;
}

.empty-filter-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 36px 20px;
  gap: 6px;
}

.reset-filter-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: 34px;
  padding: 0 14px;
  margin-top: 10px;
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

.reset-filter-btn:hover {
  background-color: #e2e8f0;
}
</style>