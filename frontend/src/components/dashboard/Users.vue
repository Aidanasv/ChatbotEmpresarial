<template>
  <v-container fluid class="pa-6 pa-md-8 bg-grey-lighten-5 h-100">

    <div class="d-flex flex-column flex-md-row justify-space-between align-md-center mb-8">
      <div>
        <h1 class="text-h4 font-weight-bold mb-2">Gestión de Usuarios</h1>
        <p class="text-medium-emphasis">Administra los permisos y accesos a tu plataforma</p>
      </div>

      <div class="mt-4 mt-md-0 d-flex gap-3">
        <v-btn color="primary" prepend-icon="mdi-plus" class="text-none font-weight-bold" elevation="0"
          @click="openCreateModal">
          Añadir Usuario
        </v-btn>
      </div>
    </div>

    <v-card class="rounded-xl border" elevation="0">

      <div class="pa-5 border-b d-flex flex-column flex-sm-row gap-4 align-sm-center justify-space-between bg-white">
        <v-text-field v-model="search" density="compact" variant="outlined" prepend-inner-icon="mdi-magnify"
          placeholder="Buscar por nombre o correo..." hide-details bg-color="grey-lighten-4"
          class="max-w-sm"></v-text-field>

        <div class="d-flex gap-3">
          <v-select v-model="roleFilter" :items="['Todos', 'Administrador', 'Agente', 'Lector']" density="compact"
            variant="outlined" hide-details bg-color="grey-lighten-4" style="width: 160px;"></v-select>
          <v-btn variant="outlined" color="grey-darken-1" icon="mdi-filter-variant" size="small"
            class="h-auto px-2"></v-btn>
        </div>
      </div>

      <div v-if="isLoading" class="d-flex justify-center align-center pa-10 bg-white">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
      </div>

      <div v-else-if="filteredUsers.length === 0"
        class="d-flex flex-column justify-center align-center pa-10 bg-white text-center">
        <v-icon icon="mdi-account-search-outline" size="x-large" color="disabled" class="mb-2"></v-icon>
        <p class="text-body-2 text-medium-emphasis">No se encontraron usuarios que coincidan con la búsqueda.</p>
      </div>

      <v-table v-else class="bg-white">
        <thead>
          <tr>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Usuario</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Rol</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Estado</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Fecha de creación
            </th>
            <th class="text-right font-weight-bold text-uppercase text-caption text-medium-emphasis">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id" class="v-card--link">
            <td class="py-3">
              <div class="d-flex align-center">
                <v-avatar :color="getAvatarColor(user.role)" variant="tonal" size="40" class="mr-4 font-weight-bold">
                  {{ user.userName ? user.userName.charAt(0).toUpperCase() : 'U' }}
                </v-avatar>
                <div>
                  <div class="font-weight-bold text-body-2">{{ user.userName }}</div>
                  <div class="text-caption text-medium-emphasis">{{ user.email }}</div>
                </div>
              </div>
            </td>

            <td>
              <span class="text-body-2">{{ user.role }}</span>
            </td>

            <td>
              <v-chip size="small" variant="flat" :color="getStatusColor(user.status)"
                class="font-weight-bold text-caption">
                {{ user.status }}
              </v-chip>
            </td>

            <td>
              <span class="text-caption text-medium-emphasis">{{ formatDate(user.createdAt) }}</span>
            </td>

            <td class="text-right">
              <v-btn variant="text" icon="mdi-pencil-outline" size="small" color="grey-darken-1"
                @click="openEditModal(user)"></v-btn>

              <v-btn variant="text" icon="mdi-trash-can-outline" size="small" color="error" :loading="isDeleting"
                @click="confirmDelete(user.id)"></v-btn>
            </td>
          </tr>
        </tbody>
      </v-table>

      <div class="pa-4 border-t d-flex justify-space-between align-center bg-white">
        <span class="text-caption text-medium-emphasis">
          Mostrando {{ filteredUsers.length }} de {{ userList.length }} usuarios
        </span>
        <div class="d-flex gap-2">
          <v-btn variant="outlined" size="small" class="text-none" disabled>Anterior</v-btn>
          <v-btn variant="outlined" size="small" class="text-none" disabled>Siguiente</v-btn>
        </div>
      </div>

    </v-card>

    <CreateUserModal v-model="isCreateModalOpen" :userToEdit="selectedUser" @save="handleSaveUser" />

    <CreateUserModal v-model="isCreateModalOpen" :userToEdit="selectedUser" @save="handleSaveUser" />

    <ConfirmModal v-model="isDeleteModalOpen" :loading="isDeleting" @confirm="executeDelete" />

  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useUserStore } from '@/stores/useUserStore'

import CreateUserModal from '@/components/setup/UserForm.vue'
import ConfirmModal from '@/components/utils/ConfirmModal.vue'

const userStore = useUserStore()
const { userList } = storeToRefs(userStore)

const isLoading = ref(true)
const search = ref('')
const roleFilter = ref('Todos')

const isCreateModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const userToDeleteId = ref<number | null>(null)
const isDeleting = ref(false)

const selectedUser = ref<any>(null)

const filteredUsers = computed(() => {
  if (!userList.value) return []

  return userList.value.filter(user => {
    const name = user.userName || ''
    const email = user.email || ''

    const matchesSearch = name.toLowerCase().includes(search.value.toLowerCase()) ||
      email.toLowerCase().includes(search.value.toLowerCase())
    const matchesRole = roleFilter.value === 'Todos' || user.role === roleFilter.value

    return matchesSearch && matchesRole
  })
})

const openCreateModal = () => {
  selectedUser.value = null
  isCreateModalOpen.value = true
}

const openEditModal = (user: any) => {
  selectedUser.value = { ...user }
  isCreateModalOpen.value = true
}

const handleSaveUser = async (payload: { isEdit: boolean, data: any }) => {
  try {
    isLoading.value = true

    if (payload.isEdit) {
      await userStore.updateUser(payload.data)
    } else {
      await userStore.createUser(payload.data)
    }

  } catch (error) {
    console.error("Fallo al guardar el usuario", error)
  } finally {
    isLoading.value = false
  }
}

const confirmDelete = (userId: number) => {
  userToDeleteId.value = userId
  isDeleteModalOpen.value = true
}

const executeDelete = async () => {
  if (userToDeleteId.value === null) return

  try {
    await userStore.deleteUser(userToDeleteId.value)
  } catch (error) {
    console.error("Fallo al borrar el usuario", error)
  } finally {
    userToDeleteId.value = null
    isDeleteModalOpen.value = false
    isDeleting.value = false
  }
}

const getStatusColor = (status: string) => {
  const s = status ? status.toLowerCase() : ''
  switch (s) {
    case 'activo': return 'success'
    case 'inactivo': return 'grey-darken-1'
    case 'bloqueado': return 'error'
    default: return 'primary'
  }
}

const getAvatarColor = (role: string) => {
  const r = role ? role.toLowerCase() : ''
  switch (r) {
    case 'administrador': return 'primary'
    case 'agente': return 'info'
    default: return 'grey'
  }
}

const formatDate = (dateString: Date | string) => {
  if (!dateString) return 'Desconocido'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('es-ES', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  }).format(date)
}

onMounted(async () => {
  try {
    isLoading.value = true
    await userStore.getUsers()
  } catch (error) {
    console.error('Error al cargar la vista de usuarios:', error)
  } finally {
    isLoading.value = false
  }
})
</script>