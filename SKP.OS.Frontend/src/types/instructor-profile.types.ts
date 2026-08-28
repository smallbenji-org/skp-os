import type { UserDto } from "./user.types";
import type { StudentProfileDto } from "./student-profile.types";

export interface InstructorProfileDto {
    id: number;
    applicationUserId: string;
    user: UserDto | null;
    studentProfiles: StudentProfileDto[];
}

export interface CreateInstructorProfileDto {
    applicationUserId: string;
}

export interface UpdateInstructorProfileDto {
    applicationUserId: string;
}
