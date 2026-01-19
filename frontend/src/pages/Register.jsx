import { useTranslation } from 'react-i18next';
import '../styles/form.css';
import { Link, useNavigate } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useEffect, useState } from 'react';
import { toast } from 'react-toastify';
import i18next from 'i18next';

export default function Register() {
  const { t } = useTranslation('login');

  const { isLoggedIn, registerUser } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [secondPassword, setSecondPassword] = useState('');

  useEffect(() => {
    if (typeof isLoggedIn === 'function' ? isLoggedIn() : isLoggedIn) {
      navigate('/');
    }
  }, [isLoggedIn, navigate]);

  function handleRegister(e) {
    e.prevendDefault();

    if (password !== secondPassword) {
      const theme = localStorage.getItem('theme');

      toast.error(i18next.t('passwordNotMatch', { ns: 'toast' }), {
        theme: theme,
      });
    }

    if (typeof registerUser === 'function') {
      registerUser(username, password);
    } else {
      console.warn('registerUser is not a function', registerUser);
    }
  }

  return (
    <main>
      <section className="container">
        <form className="form">
          <h2>{t('titleRegister')}</h2>
          <input
            type="text"
            placeholder={t('login')}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <input
            type="password"
            placeholder={t('password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <input
            type="password"
            placeholder={`${t('retype')} ${t('password')}`}
            value={secondPassword}
            onChange={(e) => setSecondPassword(e.target.value)}
          />
          <button onClick={handleRegister}>{t('buttonRegister')}</button>
          <Link to="/login" className="create-account">
            {t('buttonLogin')}
          </Link>
        </form>
      </section>
    </main>
  );
}
