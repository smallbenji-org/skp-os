import type { StudentProfileDto } from "./student-profile.types";
import type { ProjectTemplateDto } from "./project-template.types";

export interface ProjectDto {
    id: number;
    title: string;
    shortDescription: string;
    evaluation: string;
    conclusion: string;
    perspektivering: string;
    gitRepoUrl: string;
    isCustomProject: boolean;
    projectTemplateId: number | null;
    projectTemplate: ProjectTemplateDto | null;
    students: StudentProfileDto[];
}

export interface CreateProjectDto {
    title: string;
    shortDescription: string;
    gitRepoUrl: string;
    isCustomProject: boolean;
    projectTemplateId: number | null;
}

export interface UpdateProjectDto {
    title: string;
    shortDescription: string;
    evaluation: string;
    conclusion: string;
    perspektivering: string;
    gitRepoUrl: string;
    isCustomProject: boolean;
    projectTemplateId: number | null;
}
