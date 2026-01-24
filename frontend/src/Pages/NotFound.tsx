import { Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import '../assets/styles/error.css';

function NotFound() {
  const { t } = useTranslation('mainPage');

  return (
    <div className="error-page">
      <div className="error-background" />
      
      <div className="error-decoration decoration-1" />
      <div className="error-decoration decoration-2" />

      <div className="error-content">
        <h1 className="error-code">404</h1>
        <h2 className="error-title">{t('notFound.title')}</h2>
        <p className="error-message">
          {t('notFound.message')}
        </p>
        
        <Link to="/" className="error-home-btn">
          <svg
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M19 12H5M12 19l-7-7 7-7" />
          </svg>
          {t('notFound.backHome')}
        </Link>
      </div>
    </div>
  );
}

export default NotFound;
