import type { AxiosResponse } from "axios";
import type { CreateProjectTemplateDto, ProjectTemplateDto, UpdateProjectTemplateDto } from "@/types";
import { api } from "./api";

export default class ProjectTemplateService {
  public async getProjectTemplates(): Promise<ProjectTemplateDto[]> {
    try {
      const response: AxiosResponse<ProjectTemplateDto[]> = await api({
        url: "/api/projecttemplate",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getProjectTemplate(id: number): Promise<ProjectTemplateDto | null> {
    try {
      const response: AxiosResponse<ProjectTemplateDto> = await api({
        url: `/api/projecttemplate/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createProjectTemplate(data: CreateProjectTemplateDto): Promise<ProjectTemplateDto | null> {
    try {
      const response: AxiosResponse<ProjectTemplateDto> = await api({
        url: "/api/projecttemplate",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateProjectTemplate(id: number, data: UpdateProjectTemplateDto): Promise<ProjectTemplateDto | null> {
    try {
      const response: AxiosResponse<ProjectTemplateDto> = await api({
        url: `/api/projecttemplate/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteProjectTemplate(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/projecttemplate/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
