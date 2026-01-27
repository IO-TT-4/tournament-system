import { useState, type FormEvent } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useTranslation } from 'react-i18next';
import '../assets/styles/auth.css';

function Register() {
  const { registerUser } = useAuth();
  const { t } = useTranslation('mainPage');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    
    if (password !== confirmPassword) {
      alert(t('auth.passwordsMismatch'));
      return;
    }
    
    await registerUser(email, username, password);
  };

  return (
    <div className="auth-page">
      <div className="auth-background" />
      
      <div className="auth-card">
        <h1>{t('auth.register.title')}</h1>
        <p>{t('auth.joinUs')}</p>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">{t('auth.username')}</label>
            <input
              type="text"
              id="username"
              placeholder={t('auth.usernamePlaceholder')}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">{t('auth.email')}</label>
            <input
              type="email"
              id="email"
              placeholder={t('auth.emailPlaceholder')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">{t('auth.password')}</label>
            <input
              type="password"
              id="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={6}
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">{t('auth.retype')} {t('auth.password')}</label>
            <input
              type="password"
              id="confirmPassword"
              placeholder="••••••••"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              minLength={6}
            />
          </div>

          <button type="submit" className="auth-submit-btn">
            {t('auth.register.button')}
          </button>
        </form>

        <div className="auth-footer">
          {t('auth.hasAccount')}{' '}
          <Link to="/login">{t('auth.login.button')}</Link>
        </div>
      </div>
    </div>
  );
}

export default Register;
