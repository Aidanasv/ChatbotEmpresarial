<template>
  <v-container fluid class="pa-6 pa-md-8 users-container">
    
    <div class="d-flex flex-column flex-md-row justify-space-between align-md-center mb-8">
      <div>
        <h1 class="text-h4 font-weight-bold mb-2">Gestión de Usuarios</h1>
        <p class="text-medium-emphasis">Administra los permisos y accesos a tu plataforma</p>
      </div>
      
      <div class="mt-4 mt-md-0 d-flex gap-3">
        <v-btn color="primary" prepend-icon="mdi-plus" class="text-none font-weight-bold" elevation="0">
          Añadir Usuario
        </v-btn>
      </div>
    </div>

    <v-card class="dash-card" flat>
      <div class="pa-5 border-bottom d-flex flex-column flex-sm-row gap-4 align-sm-center justify-space-between bg-white">
        <v-text-field
          v-model="search"
          density="compact"
          variant="outlined"
          prepend-inner-icon="mdi-magnify"
          placeholder="Buscar por nombre o correo..."
          hide-details
          bg-color="grey-lighten-4"
          class="max-w-sm"
        ></v-text-field>

        <div class="d-flex gap-3">
          <v-select
            v-model="roleFilter"
            :items="['Todos', 'Administrador', 'Agente', 'Lector']"
            density="compact"
            variant="outlined"
            hide-details
            bg-color="grey-lighten-4"
            style="width: 160px;"
          ></v-select>
          <v-btn variant="outlined" color="grey-darken-1" icon="mdi-filter-variant" size="small" class="h-auto px-2"></v-btn>
        </div>
      </div>

      <v-table class="users-table">
        <thead>
          <tr>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Usuario</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Rol</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Estado</th>
            <th class="text-left font-weight-bold text-uppercase text-caption text-medium-emphasis">Último acceso</th>
            <th class="text-right font-weight-bold text-uppercase text-caption text-medium-emphasis">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id" class="user-row">
            <td class="py-3">
              <div class="d-flex align-center">
                <v-avatar :color="user.avatarColor" variant="tonal" size="40" class="mr-4 font-weight-bold">
                  {{ user.name.charAt(0) }}
                </v-avatar>
                <div>
                  <div class="font-weight-bold text-body-2">{{ user.name }}</div>
                  <div class="text-caption text-medium-emphasis">{{ user.email }}</div>
                </div>
              </div>
            </td>
            
            <td>
              <span class="text-body-2">{{ user.role }}</span>
            </td>

            <td>
              <v-chip 
                size="small" 
                variant="flat" 
                :color="getStatusColor(user.status)" 
                class="font-weight-bold text-caption"
              >
                {{ user.status }}
              </v-chip>
            </td>

            <td>
              <span class="text-caption text-medium-emphasis">{{ user.lastLogin }}</span>
            </td>

            <td class="text-right">
              <v-btn variant="text" icon="mdi-pencil-outline" size="small" color="grey-darken-1"></v-btn>
              <v-btn variant="text" icon="mdi-trash-can-outline" size="small" color="error"></v-btn>
            </td>
          </tr>
        </tbody>
      </v-table>

      <div class="pa-4 border-top d-flex justify-space-between align-center bg-white">
        <span class="text-caption text-medium-emphasis">Mostrando 1-5 de 12 usuarios</span>
        <div class="d-flex gap-2">
          <v-btn variant="outlined" size="small" class="text-none" disabled>Anterior</v-btn>
          <v-btn variant="outlined" size="small" class="text-none">Siguiente</v-btn>
        </div>
      </div>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const search = ref('')
const roleFilter = ref('Todos')

const users = ref([
  { id: 1, name: 'Elena Rodríguez', email: 'elena.r@botforge.com', role: 'Administrador', status: 'Activo', lastLogin: 'Hace 2 horas', avatarColor: 'primary' },
  { id: 2, name: 'Marcos Silva', email: 'marcos.s@empresa.com', role: 'Agente', status: 'Activo', lastLogin: 'Hace 5 horas', avatarColor: 'info' },
  { id: 3, name: 'Lucía Méndez', email: 'lucia.mendez@empresa.com', role: 'Agente', status: 'Inactivo', lastLogin: 'Hace 3 días', avatarColor: 'grey' },
  { id: 4, name: 'Javier Costa', email: 'j.costa@externo.com', role: 'Lector', status: 'Activo', lastLogin: 'Ayer', avatarColor: 'success' },
  { id: 5, name: 'Roberto Díaz', email: 'roberto@botforge.com', role: 'Agente', status: 'Bloqueado', lastLogin: 'Hace 1 mes', avatarColor: 'error' },
])

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Activo': return 'success'
    case 'Inactivo': return 'grey-darken-1'
    case 'Bloqueado': return 'error'
    default: return 'primary'
  }
}

const filteredUsers = computed(() => {
  return users.value.filter(user => {
    const matchesSearch = user.name.toLowerCase().includes(search.value.toLowerCase()) || 
                          user.email.toLowerCase().includes(search.value.toLowerCase())
    const matchesRole = roleFilter.value === 'Todos' || user.role === roleFilter.value
    return matchesSearch && matchesRole
  })
})
</script>