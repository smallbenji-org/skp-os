import type { AxiosResponse } from "axios";
import type { AnnouncementDto, CreateAnnouncementDto, UpdateAnnouncementDto } from "@/types";
import { api } from "./api";

export default class AnnouncementService {
  public async getAnnouncements(): Promise<AnnouncementDto[]> {
    try {
      const response: AxiosResponse<AnnouncementDto[]> = await api({
        url: "/api/announcement",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getAnnouncement(id: number): Promise<AnnouncementDto | null> {
    try {
      const response: AxiosResponse<AnnouncementDto> = await api({
        url: `/api/announcement/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createAnnouncement(data: CreateAnnouncementDto): Promise<AnnouncementDto | null> {
    try {
      const response: AxiosResponse<AnnouncementDto> = await api({
        url: "/api/announcement",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateAnnouncement(id: number, data: UpdateAnnouncementDto): Promise<AnnouncementDto | null> {
    try {
      const response: AxiosResponse<AnnouncementDto> = await api({
        url: `/api/announcement/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteAnnouncement(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/announcement/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}