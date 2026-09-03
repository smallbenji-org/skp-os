import type { AxiosResponse } from "axios";
import type { CreateProjectDto, ProjectDto, UpdateProjectDto } from "@/types";
import { api } from "./api";

export default class ProjectService {
  public async getProjects(): Promise<ProjectDto[]> {
    try {
      const response: AxiosResponse<ProjectDto[]> = await api({
        url: "/api/project",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getProject(id: number): Promise<ProjectDto | null> {
    try {
      const response: AxiosResponse<ProjectDto> = await api({
        url: `/api/project/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createProject(data: CreateProjectDto): Promise<ProjectDto | null> {
    try {
      const response: AxiosResponse<ProjectDto> = await api({
        url: "/api/project",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateProject(id: number, data: UpdateProjectDto): Promise<ProjectDto | null> {
    try {
      const response: AxiosResponse<ProjectDto> = await api({
        url: `/api/project/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteProject(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/project/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }

  public async addStudent(id: number, studentId: number): Promise<ProjectDto | null> {
    try {
      const response: AxiosResponse<ProjectDto> = await api({
        url: `/api/project/${id}/students/${studentId}`,
        method: "POST"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async removeStudent(id: number, studentId: number): Promise<ProjectDto | null> {
    try {
      const response: AxiosResponse<ProjectDto> = await api({
        url: `/api/project/${id}/students/${studentId}`,
        method: "DELETE"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }
}
