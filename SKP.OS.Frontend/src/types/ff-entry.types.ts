export interface FFEntryDto {
    id: number;
    date: string;
    duration: string;
    note: string;
    studentProfileId: number;
    createdBy?: string;
    validFrom?: string;
    validTo?: string;
}

export interface InstructorFreeEntryDto {
    id: number;
    date: string;
    createdBy?: string;
    createdDate?: string;
    period: string;
    duration: string;
    category: string;
    note: string;
    studentProfileId?: number;
}

export interface CreateFFEntryDto {
    date: string;
    duration: string;
    note: string;
    studentProfileId: number;
}

export interface UpdateFFEntryDto {
    date: string;
    duration: string;
    note: string;
    studentProfileId: number;
}
