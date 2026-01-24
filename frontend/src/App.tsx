import { useEffect } from 'react';
import { Outlet } from 'react-router';
import { useTranslation } from 'react-i18next';
import Header from './Components/Header';
import './assets/styles/index.css';
import { UserProvider } from './context/useAuth';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

function App() {
  const { t, i18n } = useTranslation('mainPage');

  useEffect(() => {
    document.title = t('title');
    document.documentElement.lang = i18n.language;
  }, [t, i18n.language]);

  return (
    <UserProvider>
      <Header />
      <Outlet />
      <ToastContainer position="bottom-right" theme="dark" />
    </UserProvider>
  );
}

export default App;
