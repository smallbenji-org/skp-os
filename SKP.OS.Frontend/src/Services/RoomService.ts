import type { AxiosResponse } from "axios";
import type { CreateRoomDto, RoomDto, UpdateRoomDto } from "@/types";
import { api } from "./api";

export default class RoomService {
  public async getRooms(): Promise<RoomDto[]> {
    try {
      const response: AxiosResponse<RoomDto[]> = await api({
        url: "/api/room",
        method: "GET"
      });
      return response.data ? response.data : [];
    } catch {
      return [];
    }
  }

  public async getRoom(id: number): Promise<RoomDto | null> {
    try {
      const response: AxiosResponse<RoomDto> = await api({
        url: `/api/room/${id}`,
        method: "GET"
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async createRoom(data: CreateRoomDto): Promise<RoomDto | null> {
    try {
      const response: AxiosResponse<RoomDto> = await api({
        url: "/api/room",
        method: "POST",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async updateRoom(id: number, data: UpdateRoomDto): Promise<RoomDto | null> {
    try {
      const response: AxiosResponse<RoomDto> = await api({
        url: `/api/room/${id}`,
        method: "PUT",
        data
      });
      return response.data ? response.data : null;
    } catch {
      return null;
    }
  }

  public async deleteRoom(id: number): Promise<boolean> {
    try {
      await api({
        url: `/api/room/${id}`,
        method: "DELETE"
      });
      return true;
    } catch {
      return false;
    }
  }
}
