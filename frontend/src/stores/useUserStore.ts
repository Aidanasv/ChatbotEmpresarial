import type { User, UserState } from "@/types/User";
import axios from "axios";
import { defineStore } from "pinia";

export const useUserStore = defineStore('user', {
    state: (): UserState => ({
        user: null,
        limit: 5,
        offset: 0,
        userList: []
    }),
    actions: {
        async getUsers() {
            try {
                const response = await axios.get<User[]>('http://localhost:5267/api/user', {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const data: User[] = response.data;
                this.userList = data;
                this.limit = data.length;
                this.offset = 0;
            } catch (error) {
                console.error("Error fetching users:", error);
            }
        },

        async createUser(newUser: Omit<User, 'id' | 'createdAt'>) {
            try {
                const response = await axios.post<User>('http://localhost:5267/api/user', newUser, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const createdUser: User = response.data;
                this.userList.unshift(createdUser);
            } catch (error) {
                console.error("Error creating user:", error);
            }
        },

        async updateUser(updatedData: Partial<Omit<User,'createdAt'>>) {
            try {
                const response = await axios.put<User>(`http://localhost:5267/api/user/${updatedData.id}`, updatedData, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                const updatedUser: User = response.data;
                const index = this.userList.findIndex(user => user.id === updatedData.id);
                if (index !== -1) {
                    this.userList[index] = updatedUser;
                }
            } catch (error) {
                console.error("Error updating user:", error);
            }
        },

        async deleteUser(userId: number) {
            try {
                await axios.delete(`http://localhost:5267/api/user/${userId}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                this.userList = this.userList.filter(user => user.id !== userId);
            } catch (error) {
                console.error("Error deleting user:", error);
            }
        }   
    }
})