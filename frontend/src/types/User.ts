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
}