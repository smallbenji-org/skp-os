import type { AxiosResponse } from "axios";
import type { CreateLogbookEntryDto, LogbookEntryDto, UpdateLogbookEntryDto } from "@/types";
import { api } from "./api";

export default class LogbookEntryService {
  public async getLogbookEntries(studentProfileId?: number): Promise<LogbookEntryDto[]> {
    try {
      const response: AxiosResponse<LogbookEntryDto[]> = await api({
        url: "/api/logbookentry",
        method: "GET",
        params: { studentProfileId }
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getLogbookEntry(id: number): Promise<LogbookEntryDto | null> {
    try {
      const response: AxiosResponse<LogbookEntryDto> = await api({
        url: `/api/logbookentry/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createLogbookEntry(data: CreateLogbookEntryDto): Promise<LogbookEntryDto | null> {
    try {
      const response: AxiosResponse<LogbookEntryDto> = await api({
        url: "/api/logbookentry",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateLogbookEntry(id: number, data: UpdateLogbookEntryDto): Promise<LogbookEntryDto | null> {
    try {
      const response: AxiosResponse<LogbookEntryDto> = await api({
        url: `/api/logbookentry/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteLogbookEntry(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/logbookentry/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
