import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import mainPagePl from './locales/pl/mainPage.json';
import mainPageEn from './locales/en/mainPage.json';
import HeaderPl from './locales/pl/header.json';
import HeaderEn from './locales/en/header.json';
import LoginAndRegisterPl from './locales/pl/loginAndRegister.json';
import LoginAndRegisterEn from './locales/en/loginAndRegister.json';
import ToastPl from './locales/pl/toast.json';
import ToastEn from './locales/en/toast.json';

i18n.use(initReactI18next).init({
  resources: {
    pl: {
      mainPage: mainPagePl,
      header: HeaderPl,
      login: LoginAndRegisterPl,
      toast: ToastPl,
    },
    en: {
      mainPage: mainPageEn,
      header: HeaderEn,
      login: LoginAndRegisterEn,
      toast: ToastEn,
    },
  },
  lng: localStorage.getItem('i18nextLng') || 'pl',
  fallbackLng: 'en',
  defaultNS: 'mainPage', // Fallback to mainPage if no namespace specified
  interpolation: {
    escapeValue: false,
  },
});

i18n.on('languageChanged', (lng) => {
  localStorage.setItem('i18nextLng', lng);
});

export default i18n;
