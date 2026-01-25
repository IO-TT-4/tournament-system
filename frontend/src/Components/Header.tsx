import { Link } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useTranslation } from 'react-i18next';
import logo from '../assets/gflow_dark_transparent_64px.png';
import '../assets/styles/header.css';

function Header() {
  const { user, isLoggedIn, logout } = useAuth();
  const { t, i18n } = useTranslation('header');

  const toggleLanguage = () => {
    const nextLang = i18n.language === 'pl' ? 'en' : 'pl';
    i18n.changeLanguage(nextLang);
  };

  return (
    <header>
      <div className="logo">
        <Link to="/">
          <img src={logo} alt={t('menu.logoAlt')} />
          <span>G-Flow</span>
        </Link>
      </div>

      <nav>
        <ul>
          <li>
            <Link to="/tournaments" className="navBtn">
              {t('menu.tournaments')}
            </Link>
          </li>
          {isLoggedIn() ? (
            <>
              <li>
                <Link to="/create-tournament" className="navBtn">
                  {t('menu.createTournament') || 'Create'}
                </Link>
              </li>
              <li className="user-info">
                <span>{user?.username}</span>
              </li>
              <li>
                <button onClick={logout} className="navBtn logoutBtn">
                  {t('menu.authorized.logout')}
                </button>
              </li>
            </>
          ) : (
            <>
              <li>
                <Link to="/login" className="navBtn">
                  {t('menu.guest.login')}
                </Link>
              </li>
            </>
          )}
          <li>
            <button className="lang-switcher" onClick={toggleLanguage}>
              {i18n.language.toUpperCase()}
            </button>
          </li>
        </ul>
      </nav>
    </header>
  );
}

export default Header;
