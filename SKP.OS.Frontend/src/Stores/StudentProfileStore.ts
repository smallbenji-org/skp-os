import StudentProfileService from "@/Services/StudentProfileService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import type { CreateStudentProfileDto, StudentProfileDto, UpdateStudentProfileDto } from "@/types";

export const useStudentProfileStore = defineStore("studentProfile", () => {
  const studentProfileService = new StudentProfileService();

  const StudentProfiles = ref<StudentProfileDto[]>([]);
  const SelectedStudentProfile = ref<StudentProfileDto | null>(null);
  const MyStudentProfile = ref<StudentProfileDto | null>(null);

  const STUDENT_PROFILES = computed(() => StudentProfiles.value);
  const SELECTED_STUDENT_PROFILE = computed(() => SelectedStudentProfile.value);
  const MY_STUDENT_PROFILE = computed(() => MyStudentProfile.value);

  async function GET_STUDENT_PROFILES() {
    const data = await studentProfileService.getStudentProfiles();
    StudentProfiles.value = data;
    return data;
  }

  async function GET_STUDENT_PROFILE(id: number) {
    const data = await studentProfileService.getStudentProfile(id);
    SelectedStudentProfile.value = data;
    return data;
  }

  async function GET_MY_STUDENT_PROFILE() {
    const data = await studentProfileService.getMyStudentProfile();
    MyStudentProfile.value = data;
    return data;
  }

  async function CREATE_STUDENT_PROFILE(data: CreateStudentProfileDto) {
    const created = await studentProfileService.createStudentProfile(data);
    if (created) {
      await GET_STUDENT_PROFILES();
    }
    return created;
  }

  async function UPDATE_STUDENT_PROFILE(id: number, data: UpdateStudentProfileDto) {
    const updated = await studentProfileService.updateStudentProfile(id, data);
    if (updated) {
      await GET_STUDENT_PROFILES();
      SelectedStudentProfile.value = updated;
    }
    return updated;
  }

  async function DELETE_STUDENT_PROFILE(id: number) {
    const success = await studentProfileService.deleteStudentProfile(id);
    if (success) {
      await GET_STUDENT_PROFILES();
    }
    return success;
  }

  async function ADD_INSTRUCTOR(id: number, instructorId: number) {
    const updated = await studentProfileService.addInstructor(id, instructorId);
    if (updated) {
      SelectedStudentProfile.value = updated;
    }
    return updated;
  }

  async function REMOVE_INSTRUCTOR(id: number, instructorId: number) {
    const updated = await studentProfileService.removeInstructor(id, instructorId);
    if (updated) {
      SelectedStudentProfile.value = updated;
    }
    return updated;
  }

  return {
    StudentProfiles, SelectedStudentProfile, MyStudentProfile,
    STUDENT_PROFILES, SELECTED_STUDENT_PROFILE, MY_STUDENT_PROFILE,
    GET_STUDENT_PROFILES, GET_STUDENT_PROFILE, GET_MY_STUDENT_PROFILE,
    CREATE_STUDENT_PROFILE, UPDATE_STUDENT_PROFILE, DELETE_STUDENT_PROFILE,
    ADD_INSTRUCTOR, REMOVE_INSTRUCTOR
  }
});
