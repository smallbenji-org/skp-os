import InstructorProfileService from "@/Services/InstructorProfileService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateInstructorProfileDto, InstructorProfileDto } from "@/types";

export const useInstructorProfileStore = defineStore("instructorProfile", () => {
  const instructorProfileService = new InstructorProfileService();

  const InstructorProfiles = ref<InstructorProfileDto[]>([]);
  const SelectedInstructorProfile = ref<InstructorProfileDto | null>(null);
  const MyInstructorProfile = ref<InstructorProfileDto | null>(null);

  const INSTRUCTOR_PROFILES = computed(() => InstructorProfiles.value);
  const SELECTED_INSTRUCTOR_PROFILE = computed(() => SelectedInstructorProfile.value);
  const MY_INSTRUCTOR_PROFILE = computed(() => MyInstructorProfile.value);

  async function GET_INSTRUCTOR_PROFILES() {
    const data = await instructorProfileService.getInstructorProfiles();
    InstructorProfiles.value = data;
    return data;
  }

  async function GET_INSTRUCTOR_PROFILE(id: number) {
    const data = await instructorProfileService.getInstructorProfile(id);
    SelectedInstructorProfile.value = data;
    return data;
  }

  async function GET_MY_INSTRUCTOR_PROFILE() {
    const data = await instructorProfileService.getMyInstructorProfile();
    MyInstructorProfile.value = data;
    return data;
  }

  async function CREATE_INSTRUCTOR_PROFILE(data: CreateInstructorProfileDto) {
    const created = await instructorProfileService.createInstructorProfile(data);
    if (created) {
      await GET_INSTRUCTOR_PROFILES();
    }
    return created;
  }

  async function DELETE_INSTRUCTOR_PROFILE(id: number) {
    const success = await instructorProfileService.deleteInstructorProfile(id);
    if (success) {
      await GET_INSTRUCTOR_PROFILES();
    }
    return success;
  }

  async function ADD_STUDENT(id: number, studentId: number) {
    const updated = await instructorProfileService.addStudent(id, studentId);
    if (updated) {
      SelectedInstructorProfile.value = updated;
    }
    return updated;
  }

  async function REMOVE_STUDENT(id: number, studentId: number) {
    const updated = await instructorProfileService.removeStudent(id, studentId);
    if (updated) {
      SelectedInstructorProfile.value = updated;
    }
    return updated;
  }

  return {
    InstructorProfiles, SelectedInstructorProfile, MyInstructorProfile,
    INSTRUCTOR_PROFILES, SELECTED_INSTRUCTOR_PROFILE, MY_INSTRUCTOR_PROFILE,
    GET_INSTRUCTOR_PROFILES, GET_INSTRUCTOR_PROFILE, GET_MY_INSTRUCTOR_PROFILE,
    CREATE_INSTRUCTOR_PROFILE, DELETE_INSTRUCTOR_PROFILE, ADD_STUDENT, REMOVE_STUDENT
  }
});
