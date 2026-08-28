export interface RoomDto {
    id: number;
    name: string;
    location: string;
}

export interface CreateRoomDto {
    name: string;
    location: string;
}

export interface UpdateRoomDto {
    name: string;
    location: string;
}
