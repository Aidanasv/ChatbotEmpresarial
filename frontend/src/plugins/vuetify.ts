import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'

export default createVuetify({
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#536DFE',
          secondary: '#1976D2',
          success: '#4CAF50',
          background: '#f8f9fa',
          surface: '#ffffff',
        }
      },
      dark: {
        colors: {
          primary: '#536DFE',
          secondary: '#1976D2',
          success: '#4CAF50',
        }
      }
    }
  }
})