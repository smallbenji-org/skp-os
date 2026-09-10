import AnnouncementService from "@/Services/AnnouncementService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { AnnouncementDto, CreateAnnouncementDto, UpdateAnnouncementDto } from "@/types";

export const useAnnouncementStore = defineStore("announcement", () => {
  const announcementService = new AnnouncementService();

  const Announcements = ref<AnnouncementDto[]>([]);
  const SelectedAnnouncement = ref<AnnouncementDto | null>(null);

  const ANNOUNCEMENTS = computed(() => Announcements.value);
  const ACTIVE_ANNOUNCEMENTS = computed(() =>
    Announcements.value.filter((a) => a.isActive),
  );
  const SELECTED_ANNOUNCEMENT = computed(() => SelectedAnnouncement.value);

  async function GET_ANNOUNCEMENTS() {
    const data = await announcementService.getAnnouncements();
    Announcements.value = data;
    return data;
  }

  async function GET_ANNOUNCEMENT(id: number) {
    const data = await announcementService.getAnnouncement(id);
    SelectedAnnouncement.value = data;
    return data;
  }

  async function CREATE_ANNOUNCEMENT(data: CreateAnnouncementDto) {
    const created = await announcementService.createAnnouncement(data);
    if (created) {
      await GET_ANNOUNCEMENTS();
    }
    return created;
  }

  async function UPDATE_ANNOUNCEMENT(id: number, data: UpdateAnnouncementDto) {
    const updated = await announcementService.updateAnnouncement(id, data);
    if (updated) {
      await GET_ANNOUNCEMENTS();
      SelectedAnnouncement.value = updated;
    }
    return updated;
  }

  async function DELETE_ANNOUNCEMENT(id: number) {
    const success = await announcementService.deleteAnnouncement(id);
    if (success) {
      await GET_ANNOUNCEMENTS();
    }
    return success;
  }

  return {
    Announcements, SelectedAnnouncement,
    ANNOUNCEMENTS, ACTIVE_ANNOUNCEMENTS, SELECTED_ANNOUNCEMENT,
    GET_ANNOUNCEMENTS, GET_ANNOUNCEMENT, CREATE_ANNOUNCEMENT, UPDATE_ANNOUNCEMENT, DELETE_ANNOUNCEMENT
  }
});