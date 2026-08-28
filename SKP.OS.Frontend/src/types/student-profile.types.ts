import type { StudentType } from "./enums.types";
import type { ContractType } from "./contract-type.types";
import type { ProjectHaul } from "./project-haul.types";
import type { UserDto } from "./user.types";

export interface StudentProfileDto {
    id: number;
    applicationUserId: string;
    user: UserDto | null;
    studentType: StudentType;
    contractType: ContractType;
    isEuxStudent: boolean;
    completedHauls: ProjectHaul[];
}

export interface CreateStudentProfileDto {
    applicationUserId: string;
    studentType: StudentType;
    contractType: ContractType;
    isEuxStudent: boolean;
    completedHauls: ProjectHaul[];
}

export interface UpdateStudentProfileDto {
    studentType: StudentType;
    contractType: ContractType;
    isEuxStudent: boolean;
    completedHauls: ProjectHaul[];
}
