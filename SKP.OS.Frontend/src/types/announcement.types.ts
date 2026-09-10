export interface AnnouncementDto {
    id: number;
    title: string;
    message: string;
    createdAt: string;
    isActive: boolean;
}

export interface CreateAnnouncementDto {
    title: string;
    message: string;
    isActive: boolean;
}

export interface UpdateAnnouncementDto {
    title: string;
    message: string;
    isActive: boolean;
}