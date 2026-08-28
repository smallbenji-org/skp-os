export interface InfoEntryDto {
    id: number;
    title: string;
    content: string;
    createdAt: string;
    isPinned: boolean;
    instructorProfileId: number;
}

export interface CreateInfoEntryDto {
    title: string;
    content: string;
    isPinned: boolean;
    instructorProfileId: number;
}

export interface UpdateInfoEntryDto {
    title: string;
    content: string;
    isPinned: boolean;
    instructorProfileId: number;
}
