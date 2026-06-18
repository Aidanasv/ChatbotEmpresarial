<template>
    <v-container fluid class="fill-height auth-container d-flex align-center justify-center py-16">
        <div class="auth-wrapper text-center">
            <h1 class="text-h4 font-weight-bold mb-2 auth-title">Restablecer contraseña</h1>
            <p class="text-medium-emphasis mb-8">Crea una nueva contraseña para volver a entrar a tu panel.</p>

            <v-card elevation="2" class="auth-card pa-8 rounded-xl mx-auto">
                <v-form @submit.prevent="handleSubmit" class="text-left">
                    <div class="mb-4">
                        <label class="auth-label">Correo electrónico</label>
                        <v-text-field v-model="email" variant="solo" flat bg-color="grey-lighten-4"
                            prepend-inner-icon="mdi-email-outline" hide-details class="rounded-lg mt-1" readonly />
                    </div>

                    <div class="mb-4">
                        <label class="auth-label">Nueva contraseña</label>
                        <v-text-field v-model="newPassword" type="password" variant="solo" flat
                            bg-color="grey-lighten-4" prepend-inner-icon="mdi-lock-outline" hide-details
                            class="rounded-lg mt-1" />
                    </div>

                    <div class="mb-6">
                        <label class="auth-label">Confirmar contraseña</label>
                        <v-text-field v-model="confirmPassword" type="password" variant="solo" flat
                            bg-color="grey-lighten-4" prepend-inner-icon="mdi-lock-check-outline" hide-details
                            class="rounded-lg mt-1" />
                    </div>

                    <div v-if="errorMessage" class="text-body-2 text-error mb-4">{{ errorMessage }}</div>

                    <v-btn color="primary" size="x-large" block rounded="lg" type="submit"
                        class="text-none font-weight-bold auth-submit-btn" :loading="isSubmitting">
                        Guardar nueva contraseña
                    </v-btn>
                </v-form>
            </v-card>

            <v-btn variant="text" prepend-icon="mdi-arrow-left" class="mt-8 text-none text-medium-emphasis"
                to="/login?mode=signin">
                Volver al login
            </v-btn>
        </div>
    </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const email = computed(() => String(route.query.email || ''))
const token = computed(() => String(route.query.token || ''))
const newPassword = ref('')
const confirmPassword = ref('')
const errorMessage = ref('')
const isSubmitting = ref(false)

const handleSubmit = async () => {
    errorMessage.value = ''

    if (!email.value || !token.value) {
        errorMessage.value = 'El enlace de recuperación es inválido.'
        return
    }

    if (newPassword.value.length < 6) {
        errorMessage.value = 'La nueva contraseña debe tener al menos 6 caracteres.'
        return
    }

    if (newPassword.value !== confirmPassword.value) {
        errorMessage.value = 'La confirmación no coincide con la contraseña.'
        return
    }

    isSubmitting.value = true
    const wasReset = await authStore.resetPassword({
        email: email.value,
        token: token.value,
        newPassword: newPassword.value
    })
    isSubmitting.value = false

    if (wasReset) {
        router.push('/login?mode=signin')
    }
}
</script>