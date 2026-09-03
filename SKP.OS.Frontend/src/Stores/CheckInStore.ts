import CheckInService from "@/Services/CheckInService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CheckInDto, CreateCheckInDto, UpdateCheckInDto } from "@/types";

export const useCheckInStore = defineStore("checkIn", () => {
  const checkInService = new CheckInService();

  const CheckIns = ref<CheckInDto[]>([]);
  const SelectedCheckIn = ref<CheckInDto | null>(null);

  const CHECK_INS = computed(() => CheckIns.value);
  const SELECTED_CHECK_IN = computed(() => SelectedCheckIn.value);

  async function GET_CHECK_INS(studentProfileId?: number, roomId?: number) {
    const data = await checkInService.getCheckIns(studentProfileId, roomId);
    CheckIns.value = data;
    return data;
  }

  async function GET_CHECK_IN(id: number) {
    const data = await checkInService.getCheckIn(id);
    SelectedCheckIn.value = data;
    return data;
  }

  async function CREATE_CHECK_IN(data: CreateCheckInDto) {
    const created = await checkInService.createCheckIn(data);
    if (created) {
      await GET_CHECK_INS();
    }
    return created;
  }

  async function UPDATE_CHECK_IN(id: number, data: UpdateCheckInDto) {
    const updated = await checkInService.updateCheckIn(id, data);
    if (updated) {
      await GET_CHECK_INS();
      SelectedCheckIn.value = updated;
    }
    return updated;
  }

  async function DELETE_CHECK_IN(id: number) {
    const success = await checkInService.deleteCheckIn(id);
    if (success) {
      await GET_CHECK_INS();
    }
    return success;
  }

  return {
    CheckIns, SelectedCheckIn,
    CHECK_INS, SELECTED_CHECK_IN,
    GET_CHECK_INS, GET_CHECK_IN, CREATE_CHECK_IN, UPDATE_CHECK_IN, DELETE_CHECK_IN
  }
});
