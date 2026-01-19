import { useTranslation } from 'react-i18next';
import '../styles/form.css';
import { Link, useNavigate } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useEffect, useState } from 'react';

export default function Login() {
  const { t } = useTranslation('login');
  const { isLoggedIn, loginUser } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  useEffect(() => {
    if (typeof isLoggedIn === 'function' ? isLoggedIn() : isLoggedIn) {
      navigate('/');
    }
  }, [isLoggedIn, navigate]);

  function handleLogin(e) {
    e.preventDefault();
    if (typeof loginUser === 'function') {
      loginUser(username, password);
    } else {
      console.warn('loginUser is not a function', loginUser);
    }
  }

  return (
    <main>
      <section>
        <form className="form">
          <h2>{t('titleLogin')}</h2>
          <input
            type="text"
            placeholder={`${t('login')}`}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <input
            type="password"
            placeholder={t('password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <Link to="/forgot-password" className="forgot">
            {t('forgotPassword')}
          </Link>
          <button onClick={handleLogin}>{t('buttonLogin')}</button>
          <Link to="/register" className="create-account">
            {t('createAccount')}
          </Link>
        </form>
      </section>
    </main>
  );
}
