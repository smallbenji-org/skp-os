import type { AxiosResponse } from "axios";
import type { CreateFFEntryDto, FFEntryDto, UpdateFFEntryDto } from "@/types";
import { api } from "./api";

export default class FFEntryService {
  public async getFFEntries(studentProfileId?: number): Promise<FFEntryDto[]> {
    try {
      const response: AxiosResponse<FFEntryDto[]> = await api({
        url: "/api/ffentry",
        method: "GET",
        params: { studentProfileId }
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getFFEntry(id: number): Promise<FFEntryDto | null> {
    try {
      const response: AxiosResponse<FFEntryDto> = await api({
        url: `/api/ffentry/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createFFEntry(data: CreateFFEntryDto): Promise<FFEntryDto | null> {
    try {
      const response: AxiosResponse<FFEntryDto> = await api({
        url: "/api/ffentry",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateFFEntry(id: number, data: UpdateFFEntryDto): Promise<FFEntryDto | null> {
    try {
      const response: AxiosResponse<FFEntryDto> = await api({
        url: `/api/ffentry/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteFFEntry(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/ffentry/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
