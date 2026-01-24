import { useState, type FormEvent } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useTranslation } from 'react-i18next';
import '../assets/styles/auth.css';

function Login() {
  const { loginUser } = useAuth();
  const { t } = useTranslation('login');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    await loginUser(username, password);
  };

  return (
    <div className="auth-page">
      <div className="auth-background" />
      
      <div className="auth-card">
        <h1>{t('titleLogin')}</h1>
        <p>{t('welcomeBack')}</p>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">{t('login')}</label>
            <input
              type="text"
              id="username"
              placeholder={t('usernamePlaceholder')}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">{t('password')}</label>
            <input
              type="password"
              id="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="auth-submit-btn">
            {t('buttonLogin')}
          </button>
        </form>

        <div className="auth-footer">
          {t('noAccount')}{' '}
          <Link to="/register">{t('buttonRegister')}</Link>
        </div>
      </div>
    </div>
  );
}

export default Login;
