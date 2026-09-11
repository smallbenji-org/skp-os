<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { IconExternalLink, IconUsers } from "@tabler/icons-vue";
import { useProjectStore } from "@/Stores/ProjectStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useAuthStore } from "@/Stores/AuthStore";

const projectStore = useProjectStore();
const studentProfileStore = useStudentProfileStore();
const authStore = useAuthStore();

const isLoading = ref(true);

const isStudent = computed(() => authStore.HAS_ROLE("Student"));
const myProfile = computed(() => studentProfileStore.MY_STUDENT_PROFILE);

const projects = computed(() => {
  const all = projectStore.PROJECTS;
  if (!isStudent.value || !myProfile.value) return all;
  return all.filter((p) => p.students.some((s) => s.id === myProfile.value?.id));
});

function studentNames(students: { user: { name: string } | null }[]): string {
  const names = students.map((s) => s.user?.name ?? "Ukendt");
  return names.length > 0 ? names.join(", ") : "Ingen elever tilknyttet";
}

function openRepo(url: string) {
  const target = url.trim();
  if (!/^https?:\/\//i.test(target)) return;
  window.open(target, "_blank", "noopener,noreferrer");
}

onMounted(async () => {
  try {
    if (isStudent.value) {
      await studentProfileStore.GET_MY_STUDENT_PROFILE();
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
      <h1 class="page-title">SKP Projekter</h1>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <div v-else-if="projects.length === 0" class="surface empty">
      <h2 class="empty-title">Ingen projekter endnu</h2>
      <p class="empty-text">
        Du er ikke tilknyttet nogen projekter endnu. Spørg din underviser hvis du mener der mangler noget.
      </p>
    </div>

    <div v-else class="project-grid">
      <article v-for="project in projects" :key="project.id" class="surface project-card">
        <div class="project-heading">
          <h2 class="project-title">{{ project.title }}</h2>
          <span v-if="project.isCustomProject" class="custom-badge">Eget projekt</span>
        </div>
        <p class="project-desc">{{ project.shortDescription }}</p>
        <div class="project-meta">
          <span class="meta-students">
            <IconUsers :size="16" :stroke-width="2" />
            {{ studentNames(project.students) }}
          </span>
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

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
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

.custom-badge {
  font-size: 12px;
  font-weight: 600;
  color: #2563eb;
  background: #eef2ff;
  border-radius: 999px;
  padding: 2px 10px;
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
</style>