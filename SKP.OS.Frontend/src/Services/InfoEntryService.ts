import type { AxiosResponse } from "axios";
import type { CreateInfoEntryDto, InfoEntryDto, UpdateInfoEntryDto } from "@/types";
import { api } from "./api";

export default class InfoEntryService {
  public async getInfoEntries(pinned?: boolean): Promise<InfoEntryDto[]> {
    try {
      const response: AxiosResponse<InfoEntryDto[]> = await api({
        url: "/api/infoentry",
        method: "GET",
        params: { pinned }
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getInfoEntry(id: number): Promise<InfoEntryDto | null> {
    try {
      const response: AxiosResponse<InfoEntryDto> = await api({
        url: `/api/infoentry/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createInfoEntry(data: CreateInfoEntryDto): Promise<InfoEntryDto | null> {
    try {
      const response: AxiosResponse<InfoEntryDto> = await api({
        url: "/api/infoentry",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateInfoEntry(id: number, data: UpdateInfoEntryDto): Promise<InfoEntryDto | null> {
    try {
      const response: AxiosResponse<InfoEntryDto> = await api({
        url: `/api/infoentry/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteInfoEntry(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/infoentry/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
