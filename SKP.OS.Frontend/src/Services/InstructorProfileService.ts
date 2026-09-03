import type { AxiosResponse } from "axios";
import type { CreateInstructorProfileDto, InstructorProfileDto } from "@/types";
import { api } from "./api";

export default class InstructorProfileService {
  public async getInstructorProfiles(): Promise<InstructorProfileDto[]> {
    try {
      const response: AxiosResponse<InstructorProfileDto[]> = await api({
        url: "/api/instructorprofile",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getInstructorProfile(id: number): Promise<InstructorProfileDto | null> {
    try {
      const response: AxiosResponse<InstructorProfileDto> = await api({
        url: `/api/instructorprofile/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async getMyInstructorProfile(): Promise<InstructorProfileDto | null> {
    try {
      const response: AxiosResponse<InstructorProfileDto> = await api({
        url: "/api/instructorprofile/me",
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createInstructorProfile(data: CreateInstructorProfileDto): Promise<InstructorProfileDto | null> {
    try {
      const response: AxiosResponse<InstructorProfileDto> = await api({
        url: "/api/instructorprofile",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteInstructorProfile(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/instructorprofile/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }

  public async addStudent(id: number, studentId: number): Promise<InstructorProfileDto | null> {
    try {
      const response: AxiosResponse<InstructorProfileDto> = await api({
        url: `/api/instructorprofile/${id}/students/${studentId}`,
        method: "POST"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async removeStudent(id: number, studentId: number): Promise<InstructorProfileDto | null> {
    try {
      const response: AxiosResponse<InstructorProfileDto> = await api({
        url: `/api/instructorprofile/${id}/students/${studentId}`,
        method: "DELETE"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }
}
