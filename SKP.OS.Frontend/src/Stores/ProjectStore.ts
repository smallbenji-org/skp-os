import ProjectService from "@/Services/ProjectService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateProjectDto, ProjectDto, ProjectStage, UpdateProjectDto } from "@/types";

export const useProjectStore = defineStore("project", () => {
  const projectService = new ProjectService();

  const Projects = ref<ProjectDto[]>([]);
  const SelectedProject = ref<ProjectDto | null>(null);

  const PROJECTS = computed(() => Projects.value);
  const SELECTED_PROJECT = computed(() => SelectedProject.value);

  async function GET_PROJECTS() {
    const data = await projectService.getProjects();
    Projects.value = data;
    return data;
  }

  async function GET_PROJECT(id: number) {
    const data = await projectService.getProject(id);
    SelectedProject.value = data;
    return data;
  }

  async function CREATE_PROJECT(data: CreateProjectDto) {
    const created = await projectService.createProject(data);
    if (created) {
      await GET_PROJECTS();
    }
    return created;
  }

  async function CREATE_PROJECT_FROM_TEMPLATE(templateId: number) {
    const created = await projectService.createProjectFromTemplate(templateId);
    if (created) {
      await GET_PROJECTS();
    }
    return created;
  }

  async function UPDATE_PROJECT(id: number, data: UpdateProjectDto) {
    const updated = await projectService.updateProject(id, data);
    if (updated) {
      await GET_PROJECTS();
      SelectedProject.value = updated;
    }
    return updated;
  }

  async function UPDATE_PROJECT_STAGE(id: number, stage: ProjectStage) {
    const updated = await projectService.updateProjectStage(id, stage);
    if (updated) {
      await GET_PROJECTS();
      SelectedProject.value = updated;
    }
    return updated;
  }

  async function SUBMIT_PROJECT(id: number, data: UpdateProjectDto) {
    const updated = await projectService.submitProject(id, data);
    if (updated) {
      await GET_PROJECTS();
      SelectedProject.value = updated;
    }
    return updated;
  }

  async function DELETE_PROJECT(id: number) {
    const success = await projectService.deleteProject(id);
    if (success) {
      await GET_PROJECTS();
    }
    return success;
  }

  async function ADD_STUDENT(id: number, studentId: number) {
    const updated = await projectService.addStudent(id, studentId);
    if (updated) {
      SelectedProject.value = updated;
    }
    return updated;
  }

  async function REMOVE_STUDENT(id: number, studentId: number) {
    const updated = await projectService.removeStudent(id, studentId);
    if (updated) {
      SelectedProject.value = updated;
    }
    return updated;
  }

  return {
    Projects, SelectedProject,
    PROJECTS, SELECTED_PROJECT,
    GET_PROJECTS, GET_PROJECT, CREATE_PROJECT, CREATE_PROJECT_FROM_TEMPLATE, UPDATE_PROJECT, UPDATE_PROJECT_STAGE, SUBMIT_PROJECT, DELETE_PROJECT,
    ADD_STUDENT, REMOVE_STUDENT
  }
});
