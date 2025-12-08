import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import DarkModeSwitch from './DarkModeToggle';

import { useAuth } from '../context/useAuth';

export default function NavBar() {
  const { t, i18n } = useTranslation('header');
  const [isOpen, setIsOpen] = useState(false);

  const { isLoggedIn, user, logout } = useAuth();

  function toggleMenu() {
    setIsOpen(!isOpen);
  }

  const noAuthNav = (
    <>
      <li onClick={toggleMenu}>
        <Link to="/login">{t('menu.guest.login')}</Link>
      </li>
      <li onClick={toggleMenu}>
        <Link to="/register">{t('menu.guest.register')}</Link>
      </li>
    </>
  );

  const authNav = (
    <>
      {/* <li>
        <Link to=""></Link>
      </li> */}
      <li className="avatar-dropdown">
        <div className="avatar-wrapper" onClick={() => setIsOpen(!isOpen)}>
          <svg width="60" height="60" viewBox="0 0 60 60" className="avatar">
            <defs>
              <clipPath id="circle-clip">
                <circle cx="30" cy="30" r="30" />
              </clipPath>
              <filter
                id="avatar-shadow"
                x="-50%"
                y="-50%"
                width="200%"
                height="200%">
                <feDropShadow
                  dx="0"
                  dy="1"
                  stdDeviation="1.5"
                  floodOpacity="0.20"
                />
                <feDropShadow
                  dx="0"
                  dy="3"
                  stdDeviation="4"
                  floodOpacity="0.18"
                />
                <feDropShadow
                  dx="0"
                  dy="6"
                  stdDeviation="6"
                  floodOpacity="0.15"
                />
              </filter>
            </defs>

            <g filter="url(#avatar-shadow)" clipPath="url(#circle-clip)">
              <image
                href={user?.avatar || '/default-avatar.png'}
                x="0"
                y="0"
                width="60"
                height="60"
                preserveAspectRatio="xMidYMid slice"
              />
            </g>
          </svg>
        </div>

        {/* Dropdown menu */}
        {isOpen && (
          <ul className="dropdown-menu">
            <li onClick={toggleMenu}>
              <Link to={`/user/${user?.id || ''}`}>
                {t('menu.authorized.myprofile')}
              </Link>
            </li>
            <li onClick={toggleMenu}>
              <Link to={`/user/${user?.id || ''}/edit`}>
                {t('menu.authorized.settings')}
              </Link>
            </li>
            <li
              onClick={() => {
                toggleMenu();
                logout();
              }}>
              {t('menu.authorized.logout')}
            </li>
          </ul>
        )}
      </li>
    </>
  );

  return (
    <nav>
      <div className={`hamburger ${isOpen ? 'open' : ''}`} onClick={toggleMenu}>
        <span></span>
        <span></span>
        <span></span>
      </div>

      <ul className={`nav-menu ${isOpen ? 'active' : ''}`}>
        <li>
          <DarkModeSwitch />
        </li>
        <li className="language-selector">
          <select
            id="language"
            name="language"
            onChange={(e) => i18n.changeLanguage(e.target.value)}>
            <option value="pl">Polski</option>
            <option value="en">English</option>
          </select>
        </li>
        {isLoggedIn() ? authNav : noAuthNav}
      </ul>
    </nav>
  );
}
