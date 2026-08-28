export interface LogbookEntryDto {
    id: number;
    date: string;
    entry: string;
    hasSearchedForJob: boolean;
    studentProfileId: number;
}

export interface CreateLogbookEntryDto {
    date: string;
    entry: string;
    hasSearchedForJob: boolean;
    studentProfileId: number;
}

export interface UpdateLogbookEntryDto {
    date: string;
    entry: string;
    hasSearchedForJob: boolean;
    studentProfileId: number;
}
