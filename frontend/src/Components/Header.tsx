import { useState } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../context/useAuth';
import { useTranslation } from 'react-i18next';
import logo from '../assets/gflow_dark_transparent_64px.png';
import '../assets/styles/header.css';

function Header() {
  const { user, isLoggedIn, logout } = useAuth();
  const { t, i18n } = useTranslation('mainPage');
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleLanguage = () => {
    const nextLang = i18n.language === 'pl' ? 'en' : 'pl';
    i18n.changeLanguage(nextLang);
    setIsMenuOpen(false);
  };

  const toggleMenu = () => {
    setIsMenuOpen(!isMenuOpen);
  };

  const closeMenu = () => {
    setIsMenuOpen(false);
  };

  return (
    <header>
      <div className="logo">
        <Link to="/" onClick={closeMenu}>
          <img src={logo} alt={t('menu.logoAlt')} />
          <span>G-Flow</span>
        </Link>
      </div>

      <nav className={isMenuOpen ? 'active' : ''}>
        <ul>
          <li>
            <Link to="/tournaments" className="navBtn" onClick={closeMenu}>
              {t('menu.tournaments')}
            </Link>
          </li>
          {isLoggedIn() ? (
            <>
              <li>
                <Link to="/create-tournament" className="navBtn" onClick={closeMenu}>
                  {t('menu.createTournament') || 'Create'}
                </Link>
              </li>
              <li className="user-info">
                <Link to={`/user/${user?.id}`} className="navBtn" onClick={closeMenu}>
                  {user?.username}
                </Link>
              </li>
              <li>
                <button onClick={() => { logout(); closeMenu(); }} className="navBtn logoutBtn">
                  {t('menu.authorized.logout')}
                </button>
              </li>
            </>
          ) : (
            <>
              <li>
                <Link to="/login" className="navBtn" onClick={closeMenu}>
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

      <button 
        className={`hamburger ${isMenuOpen ? 'active' : ''}`} 
        onClick={toggleMenu}
        aria-label="Menu"
      >
        <span className="bar"></span>
        <span className="bar"></span>
        <span className="bar"></span>
      </button>
    </header>
  );
}

export default Header;
