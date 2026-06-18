export interface User {
    id: number;
    userName: string;
    email: string;
    role: string;
    status: string;
    createdAt: Date;
}


export interface UserState {
    user: User | null;
    limit: number;
    offset: number;
    userList: User[];
    total: number;
}

export interface UsersQuery {
    search?: string;
    role?: string;
    page?: number;
    pageSize?: number;
}

export interface UsersPagedResponse {
    items: User[];
    total: number;
    page: number;
    pageSize: number;
}