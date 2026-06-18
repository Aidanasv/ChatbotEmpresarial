<template>
  <v-container fluid class="fill-height auth-container d-flex align-center justify-center py-16">
    <div class="auth-wrapper text-center">

      <h1 class="text-h4 font-weight-bold mb-2 auth-title">
        {{ isLogin ? 'Accede a tu panel' : 'Crea tu cuenta' }}
      </h1>
      <p class="text-medium-emphasis mb-8">
        {{ isLogin ? 'Gestiona tus chatbots empresariales' : 'Empieza a automatizar hoy mismo' }}
      </p>

      <v-card elevation="2" class="auth-card pa-8 rounded-xl mx-auto">

        <div class="auth-toggle d-flex mb-8 pa-1 rounded-lg">
          <v-btn flat :class="{ 'active-toggle': isLogin }" class="flex-grow-1 rounded-md text-none transition-swing"
            @click="isLogin = true">
            Iniciar sesión
          </v-btn>
          <v-btn flat :class="{ 'active-toggle': !isLogin }" class="flex-grow-1 rounded-md text-none transition-swing"
            @click="isLogin = false">
            Registrarse
          </v-btn>
        </div>

        <v-form @submit.prevent="handleSubmit">
          <v-expand-transition>
            <div v-if="!isLogin" class="text-left mb-4">
              <label class="auth-label">Nombre completo</label>
              <v-text-field v-model="name" placeholder="Juan Pérez" variant="solo" flat bg-color="grey-lighten-4"
                prepend-inner-icon="mdi-account-outline" hide-details class="rounded-lg mt-1"></v-text-field>
            </div>
          </v-expand-transition>

          <div class="text-left mb-4">
            <label class="auth-label">Correo electrónico</label>
            <v-text-field v-model="email" placeholder="tu@empresa.com" variant="solo" flat bg-color="grey-lighten-4"
              prepend-inner-icon="mdi-email-outline" hide-details class="rounded-lg mt-1"></v-text-field>
          </div>

          <div class="text-left mb-8">
            <label class="auth-label">Contraseña</label>
            <v-text-field v-model="password" :placeholder="isLogin ? '••••••••' : 'Mínimo 8 caracteres'" type="password"
              variant="solo" flat bg-color="grey-lighten-4" prepend-inner-icon="mdi-lock-outline" hide-details
              class="rounded-lg mt-1"></v-text-field>
          </div>

          <div v-if="isLogin" class="d-flex justify-end mb-6 mt-n4">
            <v-btn variant="text" class="text-none px-0" color="primary" @click="openForgotPasswordDialog">
              Olvidé mi contraseña
            </v-btn>
          </div>

          <v-btn color="primary" size="x-large" block rounded="lg" type="submit"
            class="text-none font-weight-bold auth-submit-btn">
            {{ isLogin ? 'Iniciar sesión' : 'Crear cuenta' }}
          </v-btn>
        </v-form>
      </v-card>

      <v-dialog v-model="isForgotPasswordOpen" max-width="430">
        <v-card rounded="xl">
          <div class="px-6 pt-6 pb-3 d-flex align-center justify-space-between">
            <div>
              <div class="text-h6 font-weight-bold">Recuperar contraseña</div>
              <div class="text-body-2 text-medium-emphasis">Te enviaremos un enlace a tu correo.</div>
            </div>

            <v-btn icon variant="text" @click="closeForgotPasswordDialog">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>

          <v-card-text class="pt-2">
            <v-form @submit.prevent="handleForgotPassword" class="d-flex flex-column ga-4">
              <v-text-field v-model="forgotPasswordEmail" label="Correo electrónico" variant="outlined"
                prepend-inner-icon="mdi-email-outline" hide-details="auto" required />

              <div v-if="forgotPasswordError" class="text-body-2 text-error">{{ forgotPasswordError }}</div>

              <div class="d-flex justify-end ga-3 pt-2">
                <v-btn variant="text" class="text-none" @click="closeForgotPasswordDialog">Cancelar</v-btn>
                <v-btn type="submit" color="primary" class="text-none" :loading="isSendingResetEmail">Enviar
                  enlace</v-btn>
              </div>
            </v-form>
          </v-card-text>
        </v-card>
      </v-dialog>

      <v-btn variant="text" prepend-icon="mdi-arrow-left" class="mt-8 text-none text-medium-emphasis" to="/">
        Volver al inicio
      </v-btn>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'


const route = useRoute()
const router = useRouter()

const isLogin = ref(true)
const name = ref('')
const email = ref('')
const password = ref('')
const isForgotPasswordOpen = ref(false)
const forgotPasswordEmail = ref('')
const forgotPasswordError = ref('')
const isSendingResetEmail = ref(false)

const updateModeFromQuery = () => {
  if (route.query.mode === 'signup') {
    isLogin.value = false
  } else if (route.query.mode === 'signin') {
    isLogin.value = true
  }
}

onMounted(() => {
  updateModeFromQuery()
})

watch(() => route.query.mode, () => {
  updateModeFromQuery()
})

const openForgotPasswordDialog = () => {
  forgotPasswordEmail.value = email.value
  forgotPasswordError.value = ''
  isForgotPasswordOpen.value = true
}

const closeForgotPasswordDialog = () => {
  isForgotPasswordOpen.value = false
  forgotPasswordError.value = ''
  isSendingResetEmail.value = false
}

const handleForgotPassword = async () => {
  forgotPasswordError.value = ''

  if (!/.+@.+\..+/.test(forgotPasswordEmail.value)) {
    forgotPasswordError.value = 'Ingresa un correo válido.'
    return
  }

  isSendingResetEmail.value = true
  const authStore = useAuthStore()
  const sent = await authStore.forgotPassword({ email: forgotPasswordEmail.value.trim() })
  isSendingResetEmail.value = false

  if (sent) {
    closeForgotPasswordDialog()
  }
}

const handleSubmit = async () => {
  const authStore = useAuthStore()

  if (isLogin.value) {
    await authStore.login({ email: email.value, password: password.value })

    if (authStore.isAuthenticated && authStore.isSuperAdmin) {
      router.push({ path: '/dashboard/admin' })
    } else if (authStore.isAuthenticated && authStore.companyId) {
      router.push({ path: '/dashboard' })
    } else if (authStore.isAuthenticated) {
      router.push({ path: '/setup' })
    }
  } else {
    await authStore.register({ userName: name.value, email: email.value, password: password.value })
    if (authStore.isAuthenticated && authStore.isSuperAdmin) {
      router.push({ path: '/dashboard/admin' })
    } else if (authStore.isAuthenticated) {
      router.push({ path: '/setup' })
    }
  }
}
</script>