import type { AxiosResponse } from "axios";
import type { CreateStudentProfileDto, StudentProfileDto, UpdateStudentProfileDto } from "@/types";
import { api } from "./api";

export default class StudentProfileService {
  public async getStudentProfiles(): Promise<StudentProfileDto[]> {
    try {
      const response: AxiosResponse<StudentProfileDto[]> = await api({
        url: "/api/studentprofile",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getStudentProfile(id: number): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: `/api/studentprofile/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async getMyStudentProfile(): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: "/api/studentprofile/me",
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createStudentProfile(data: CreateStudentProfileDto): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: "/api/studentprofile",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateStudentProfile(id: number, data: UpdateStudentProfileDto): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: `/api/studentprofile/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteStudentProfile(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/studentprofile/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }

  public async addInstructor(id: number, instructorId: number): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: `/api/studentprofile/${id}/instructors/${instructorId}`,
        method: "POST"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async removeInstructor(id: number, instructorId: number): Promise<StudentProfileDto | null> {
    try {
      const response: AxiosResponse<StudentProfileDto> = await api({
        url: `/api/studentprofile/${id}/instructors/${instructorId}`,
        method: "DELETE"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }
}
