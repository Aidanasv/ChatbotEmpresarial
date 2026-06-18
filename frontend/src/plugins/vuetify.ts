import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'

type VuetifyThemeOptions = {
  isEmbed: boolean
  embedPrimary: string
  lightPrimary: string
  darkPrimary: string
}

const DEFAULT_PRIMARY = '#536DFE'

const BASE_LIGHT_COLORS = {
  secondary: '#1976D2',
  success: '#4CAF50',
  surface: '#ffffff',
  'surface-hover': '#f1f3f5',
  'surface-dark': '#1a1a1a',
  'surface-dark-alt': '#2c2c2c',
}

const BASE_DARK_COLORS = {
  secondary: '#1976D2',
  success: '#4CAF50',
  surface: '#1e1e1e',
  'surface-hover': '#2a2a2a',
  'surface-dark': '#0d0d0d',
  'surface-dark-alt': '#2c2c2c',
}

const getDefaultThemeOptions = (): VuetifyThemeOptions => ({
  isEmbed: window.location.pathname.startsWith('/my-chatbot/'),
  embedPrimary: DEFAULT_PRIMARY,
  lightPrimary: DEFAULT_PRIMARY,
  darkPrimary: DEFAULT_PRIMARY,
})

export const buildThemeConfig = (options: Partial<VuetifyThemeOptions> = {}) => {
  const resolved = { ...getDefaultThemeOptions(), ...options }

  return {
    defaultTheme: resolved.isEmbed ? 'embedTheme' : 'light',
    themes: {
      embedTheme: {
        dark: false,
        colors: {
          ...BASE_LIGHT_COLORS,
          primary: resolved.embedPrimary,
          background: '#00000000',
        },
      },
      light: {
        colors: {
          ...BASE_LIGHT_COLORS,
          primary: resolved.lightPrimary,
          background: '#f8f9fa',
        },
      },
      dark: {
        colors: {
          ...BASE_DARK_COLORS,
          primary: resolved.darkPrimary,
          background: '#121212',
        },
      },
    },
  }
}

export const createAppVuetify = (options: Partial<VuetifyThemeOptions> = {}) => {
  return createVuetify({
    theme: buildThemeConfig(options),
  })
}

export default createAppVuetify()