import { useState, type FormEvent } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useTranslation } from 'react-i18next';
import '../assets/styles/auth.css';

function Register() {
  const { registerUser } = useAuth();
  const { t } = useTranslation('login');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    
    if (password !== confirmPassword) {
      alert(t('passwordsMismatch'));
      return;
    }
    
    await registerUser(email, username, password);
  };

  return (
    <div className="auth-page">
      <div className="auth-background" />
      
      <div className="auth-card">
        <h1>{t('titleRegister')}</h1>
        <p>{t('joinUs')}</p>

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
            <label htmlFor="email">{t('email')}</label>
            <input
              type="email"
              id="email"
              placeholder={t('emailPlaceholder')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
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
              minLength={6}
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">{t('retype')} {t('password')}</label>
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
            {t('buttonRegister')}
          </button>
        </form>

        <div className="auth-footer">
          {t('hasAccount')}{' '}
          <Link to="/login">{t('buttonLogin')}</Link>
        </div>
      </div>
    </div>
  );
}

export default Register;
