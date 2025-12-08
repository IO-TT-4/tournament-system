import { useTranslation } from 'react-i18next';
import '../styles/form.css';
import { Link } from 'react-router';

export default function Register() {
  const { t } = useTranslation('login');
  return (
    <main>
      <section className="container">
        <form className="form">
          <h2>{t('titleRegister')}</h2>
          <input type="text" placeholder={t('login')} />
          <input type="email" placeholder="Email" />
          <input type="password" placeholder={t('password')} />
          <input
            type="password"
            placeholder={`${t('retype')} ${t('password')}`}
          />
          <button>{t('buttonRegister')}</button>
          <Link to="/login" className="create-account">
            {t('buttonLogin')}
          </Link>
        </form>
      </section>
    </main>
  );
}
