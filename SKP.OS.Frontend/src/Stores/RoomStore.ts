import RoomService from "@/Services/RoomService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateRoomDto, RoomDto, UpdateRoomDto } from "@/types";

export const useRoomStore = defineStore("room", () => {
  const roomService = new RoomService();

  const Rooms = ref<RoomDto[]>([]);
  const SelectedRoom = ref<RoomDto | null>(null);

  const ROOMS = computed(() => Rooms.value);
  const SELECTED_ROOM = computed(() => SelectedRoom.value);

  async function GET_ROOMS() {
    const data = await roomService.getRooms();
    Rooms.value = data;
    return data;
  }

  async function GET_ROOM(id: number) {
    const data = await roomService.getRoom(id);
    SelectedRoom.value = data;
    return data;
  }

  async function CREATE_ROOM(data: CreateRoomDto) {
    const created = await roomService.createRoom(data);
    if (created) {
      await GET_ROOMS();
    }
    return created;
  }

  async function UPDATE_ROOM(id: number, data: UpdateRoomDto) {
    const updated = await roomService.updateRoom(id, data);
    if (updated) {
      await GET_ROOMS();
      SelectedRoom.value = updated;
    }
    return updated;
  }

  async function DELETE_ROOM(id: number) {
    const success = await roomService.deleteRoom(id);
    if (success) {
      await GET_ROOMS();
    }
    return success;
  }

  return {
    Rooms, SelectedRoom,
    ROOMS, SELECTED_ROOM,
    GET_ROOMS, GET_ROOM, CREATE_ROOM, UPDATE_ROOM, DELETE_ROOM
  }
});
