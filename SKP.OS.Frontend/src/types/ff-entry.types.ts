export interface FFEntryDto {
    id: number;
    date: string;
    duration: string;
    note: string;
    studentProfileId: number;
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
