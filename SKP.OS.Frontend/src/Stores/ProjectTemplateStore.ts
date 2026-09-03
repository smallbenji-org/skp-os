import ProjectTemplateService from "@/Services/ProjectTemplateService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateProjectTemplateDto, ProjectTemplateDto, UpdateProjectTemplateDto } from "@/types";

export const useProjectTemplateStore = defineStore("projectTemplate", () => {
  const projectTemplateService = new ProjectTemplateService();

  const ProjectTemplates = ref<ProjectTemplateDto[]>([]);
  const SelectedProjectTemplate = ref<ProjectTemplateDto | null>(null);

  const PROJECT_TEMPLATES = computed(() => ProjectTemplates.value);
  const SELECTED_PROJECT_TEMPLATE = computed(() => SelectedProjectTemplate.value);

  async function GET_PROJECT_TEMPLATES() {
    const data = await projectTemplateService.getProjectTemplates();
    ProjectTemplates.value = data;
    return data;
  }

  async function GET_PROJECT_TEMPLATE(id: number) {
    const data = await projectTemplateService.getProjectTemplate(id);
    SelectedProjectTemplate.value = data;
    return data;
  }

  async function CREATE_PROJECT_TEMPLATE(data: CreateProjectTemplateDto) {
    const created = await projectTemplateService.createProjectTemplate(data);
    if (created) {
      await GET_PROJECT_TEMPLATES();
    }
    return created;
  }

  async function UPDATE_PROJECT_TEMPLATE(id: number, data: UpdateProjectTemplateDto) {
    const updated = await projectTemplateService.updateProjectTemplate(id, data);
    if (updated) {
      await GET_PROJECT_TEMPLATES();
      SelectedProjectTemplate.value = updated;
    }
    return updated;
  }

  async function DELETE_PROJECT_TEMPLATE(id: number) {
    const success = await projectTemplateService.deleteProjectTemplate(id);
    if (success) {
      await GET_PROJECT_TEMPLATES();
    }
    return success;
  }

  return {
    ProjectTemplates, SelectedProjectTemplate,
    PROJECT_TEMPLATES, SELECTED_PROJECT_TEMPLATE,
    GET_PROJECT_TEMPLATES, GET_PROJECT_TEMPLATE, CREATE_PROJECT_TEMPLATE, UPDATE_PROJECT_TEMPLATE, DELETE_PROJECT_TEMPLATE
  }
});
