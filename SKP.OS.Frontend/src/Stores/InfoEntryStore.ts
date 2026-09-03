import InfoEntryService from "@/Services/InfoEntryService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateInfoEntryDto, InfoEntryDto, UpdateInfoEntryDto } from "@/types";

export const useInfoEntryStore = defineStore("infoEntry", () => {
  const infoEntryService = new InfoEntryService();

  const InfoEntries = ref<InfoEntryDto[]>([]);
  const SelectedInfoEntry = ref<InfoEntryDto | null>(null);

  const INFO_ENTRIES = computed(() => InfoEntries.value);
  const SELECTED_INFO_ENTRY = computed(() => SelectedInfoEntry.value);

  async function GET_INFO_ENTRIES(pinned?: boolean) {
    const data = await infoEntryService.getInfoEntries(pinned);
    InfoEntries.value = data;
    return data;
  }

  async function GET_INFO_ENTRY(id: number) {
    const data = await infoEntryService.getInfoEntry(id);
    SelectedInfoEntry.value = data;
    return data;
  }

  async function CREATE_INFO_ENTRY(data: CreateInfoEntryDto) {
    const created = await infoEntryService.createInfoEntry(data);
    if (created) {
      await GET_INFO_ENTRIES();
    }
    return created;
  }

  async function UPDATE_INFO_ENTRY(id: number, data: UpdateInfoEntryDto) {
    const updated = await infoEntryService.updateInfoEntry(id, data);
    if (updated) {
      await GET_INFO_ENTRIES();
      SelectedInfoEntry.value = updated;
    }
    return updated;
  }

  async function DELETE_INFO_ENTRY(id: number) {
    const success = await infoEntryService.deleteInfoEntry(id);
    if (success) {
      await GET_INFO_ENTRIES();
    }
    return success;
  }

  return {
    InfoEntries, SelectedInfoEntry,
    INFO_ENTRIES, SELECTED_INFO_ENTRY,
    GET_INFO_ENTRIES, GET_INFO_ENTRY, CREATE_INFO_ENTRY, UPDATE_INFO_ENTRY, DELETE_INFO_ENTRY
  }
});
