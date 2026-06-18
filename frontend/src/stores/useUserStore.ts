import type { User, UserState, UsersPagedResponse, UsersQuery } from "@/types/User";
import axios from "axios";
import { defineStore } from "pinia";
import { useUiStore } from "./useUiStore";

export const useUserStore = defineStore('user', {
    state: (): UserState => ({
        user: null,
        limit: 10,
        offset: 0,
        userList: [],
        total: 0
    }),
    actions: {
        async getUsers(query: UsersQuery = {}) {
            try {
                const page = query.page ?? 1;
                const pageSize = query.pageSize ?? this.limit;
                const response = await axios.get<UsersPagedResponse>('http://localhost:5267/api/user', {
                    params: {
                        search: query.search ?? '',
                        role: query.role ?? '',
                        page,
                        pageSize
                    },
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });

                const data = response.data;
                this.userList = data.items;
                this.total = data.total;
                this.limit = data.pageSize;
                this.offset = (data.page - 1) * data.pageSize;
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al obtener la lista de usuarios. Por favor, inténtalo de nuevo.',
                    "Error al cargar usuarios",
                    3000
                );
                console.error("Error fetching users:", error);
            }
        },

        async createUser(newUser: Omit<User, 'id' | 'createdAt'>) {
            try {
                await axios.post<User>('http://localhost:5267/api/user', newUser, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                useUiStore().showSuccess("Usuario creado correctamente.", "Éxito", 3000);
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al crear el usuario. Por favor, verifica los datos e inténtalo de nuevo.',
                    "Error al crear",
                    3000
                );
                console.error("Error creating user:", error);
            }
        },

        async updateUser(updatedData: Partial<Omit<User, 'createdAt'>>) {
            try {
                await axios.put<User>(`http://localhost:5267/api/user/${updatedData.id}`, updatedData, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                useUiStore().showSuccess("Usuario actualizado correctamente.", "Éxito", 3000);
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al actualizar los datos del usuario. Por favor, inténtalo de nuevo.',
                    "Error al actualizar",
                    3000
                );
                console.error("Error updating user:", error);
            }
        },

        async deleteUser(userId: number) {
            try {
                await axios.delete(`http://localhost:5267/api/user/${userId}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
                });
                useUiStore().showSuccess("Usuario eliminado correctamente.", "Éxito", 3000);
            } catch (error) {
                useUiStore().showError(
                    'Ocurrió un error al eliminar el usuario. Por favor, inténtalo de nuevo.',
                    "Error al eliminar",
                    3000
                );
                console.error("Error deleting user:", error);
            }
        }
    }
})