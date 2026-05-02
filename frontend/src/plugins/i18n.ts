import { createI18n } from 'vue-i18n'

const messages = {
  es: {},
  en: {},
}

export default createI18n({
  legacy: false,
  locale: 'es',
  fallbackLocale: 'en',
  messages,
})
