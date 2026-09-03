import type { AxiosResponse } from "axios";
import type { CheckInDto, CreateCheckInDto, UpdateCheckInDto } from "@/types";
import { api } from "./api";

export default class CheckInService {
  public async getCheckIns(studentProfileId?: number, roomId?: number): Promise<CheckInDto[]> {
    try {
      const response: AxiosResponse<CheckInDto[]> = await api({
        url: "/api/checkin",
        method: "GET",
        params: { studentProfileId, roomId }
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getCheckIn(id: number): Promise<CheckInDto | null> {
    try {
      const response: AxiosResponse<CheckInDto> = await api({
        url: `/api/checkin/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createCheckIn(data: CreateCheckInDto): Promise<CheckInDto | null> {
    try {
      const response: AxiosResponse<CheckInDto> = await api({
        url: "/api/checkin",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateCheckIn(id: number, data: UpdateCheckInDto): Promise<CheckInDto | null> {
    try {
      const response: AxiosResponse<CheckInDto> = await api({
        url: `/api/checkin/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteCheckIn(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/checkin/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
