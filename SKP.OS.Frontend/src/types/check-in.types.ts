import type { RoomDto } from "./room.types";

export interface CheckInDto {
    id: number;
    checkInTime: string;
    checkOutTime: string | null;
    seat: string;
    studentProfileId: number;
    roomId: number;
    room: RoomDto | null;
}

export interface CreateCheckInDto {
    checkInTime: string;
    checkOutTime: string | null;
    seat: string;
    studentProfileId: number;
    roomId: number;
}

export interface UpdateCheckInDto {
    checkInTime: string;
    checkOutTime: string | null;
    seat: string;
    roomId: number;
}
