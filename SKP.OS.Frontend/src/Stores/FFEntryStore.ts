import FFEntryService from "@/Services/FFEntryService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateFFEntryDto, FFEntryDto, UpdateFFEntryDto } from "@/types";

export const useFFEntryStore = defineStore("ffEntry", () => {
  const ffEntryService = new FFEntryService();

  const FFEntries = ref<FFEntryDto[]>([]);
  const SelectedFFEntry = ref<FFEntryDto | null>(null);

  const FF_ENTRIES = computed(() => FFEntries.value);
  const SELECTED_FF_ENTRY = computed(() => SelectedFFEntry.value);

  async function GET_FF_ENTRIES(studentProfileId?: number) {
    const data = await ffEntryService.getFFEntries(studentProfileId);
    FFEntries.value = data;
    return data;
  }

  async function GET_FF_ENTRY(id: number) {
    const data = await ffEntryService.getFFEntry(id);
    SelectedFFEntry.value = data;
    return data;
  }

  async function CREATE_FF_ENTRY(data: CreateFFEntryDto) {
    const created = await ffEntryService.createFFEntry(data);
    if (created) {
      await GET_FF_ENTRIES();
    }
    return created;
  }

  async function UPDATE_FF_ENTRY(id: number, data: UpdateFFEntryDto) {
    const updated = await ffEntryService.updateFFEntry(id, data);
    if (updated) {
      await GET_FF_ENTRIES();
      SelectedFFEntry.value = updated;
    }
    return updated;
  }

  async function DELETE_FF_ENTRY(id: number) {
    const success = await ffEntryService.deleteFFEntry(id);
    if (success) {
      await GET_FF_ENTRIES();
    }
    return success;
  }

  return {
    FFEntries, SelectedFFEntry,
    FF_ENTRIES, SELECTED_FF_ENTRY,
    GET_FF_ENTRIES, GET_FF_ENTRY, CREATE_FF_ENTRY, UPDATE_FF_ENTRY, DELETE_FF_ENTRY
  }
});
