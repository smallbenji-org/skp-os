import type { ProjectHaul } from "./project-haul.types";
import type { StudentType } from "./enums.types";

export interface ProjectTemplateDto {
    id: number;
    title: string;
    shortDescription: string;
    gitRepoUrl: string;
    haul: ProjectHaul;
    studentType: StudentType;
    instructorProfileId: number;
}

export interface CreateProjectTemplateDto {
    title: string;
    shortDescription: string;
    gitRepoUrl: string;
    haul: ProjectHaul;
    studentType: StudentType;
    instructorProfileId: number;
}

export interface UpdateProjectTemplateDto {
    title: string;
    shortDescription: string;
    gitRepoUrl: string;
    haul: ProjectHaul;
    studentType: StudentType;
    instructorProfileId: number;
}
