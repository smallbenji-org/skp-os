import LogbookEntryService from "@/Services/LogbookEntryService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateLogbookEntryDto, LogbookEntryDto, UpdateLogbookEntryDto } from "@/types";

export const useLogbookEntryStore = defineStore("logbookEntry", () => {
  const logbookEntryService = new LogbookEntryService();

  const LogbookEntries = ref<LogbookEntryDto[]>([]);
  const SelectedLogbookEntry = ref<LogbookEntryDto | null>(null);

  const LOGBOOK_ENTRIES = computed(() => LogbookEntries.value);
  const SELECTED_LOGBOOK_ENTRY = computed(() => SelectedLogbookEntry.value);

  async function GET_LOGBOOK_ENTRIES(studentProfileId?: number) {
    const data = await logbookEntryService.getLogbookEntries(studentProfileId);
    LogbookEntries.value = data;
    return data;
  }

  async function GET_LOGBOOK_ENTRY(id: number) {
    const data = await logbookEntryService.getLogbookEntry(id);
    SelectedLogbookEntry.value = data;
    return data;
  }

  async function CREATE_LOGBOOK_ENTRY(data: CreateLogbookEntryDto) {
    const created = await logbookEntryService.createLogbookEntry(data);
    if (created) {
      await GET_LOGBOOK_ENTRIES();
    }
    return created;
  }

  async function UPDATE_LOGBOOK_ENTRY(id: number, data: UpdateLogbookEntryDto) {
    const updated = await logbookEntryService.updateLogbookEntry(id, data);
    if (updated) {
      await GET_LOGBOOK_ENTRIES();
      SelectedLogbookEntry.value = updated;
    }
    return updated;
  }

  async function DELETE_LOGBOOK_ENTRY(id: number) {
    const success = await logbookEntryService.deleteLogbookEntry(id);
    if (success) {
      await GET_LOGBOOK_ENTRIES();
    }
    return success;
  }

  return {
    LogbookEntries, SelectedLogbookEntry,
    LOGBOOK_ENTRIES, SELECTED_LOGBOOK_ENTRY,
    GET_LOGBOOK_ENTRIES, GET_LOGBOOK_ENTRY, CREATE_LOGBOOK_ENTRY, UPDATE_LOGBOOK_ENTRY, DELETE_LOGBOOK_ENTRY
  }
});
