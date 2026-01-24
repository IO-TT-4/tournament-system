import { Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import '../assets/styles/tournamentCard.css';

interface TournamentCardProps {
  id: string;
  title: string;
  date: string;
  game: {
    name: string;
    code: string;
  };
  location: string;
  details: string;
  status?: 'active' | 'upcoming' | 'completed';
}

function TournamentCard({ id, title, date, game, location, details, status = 'upcoming' }: TournamentCardProps) {
  const { t } = useTranslation('mainPage');

  return (
    <Link to={`/tournament/${id}`} className="tournament-card-link">
      <div className="tournament-card">
        <div className="tournament-card-header">
          <div>
            <h3>{title}</h3>
            <span className="tournament-game">{t(`games.${game.code}`)}</span>
          </div>
          <span className={`tournament-status status-${status}`}>
            {status === 'active' && t('active')}
            {status === 'upcoming' && t('comingSoon')}
            {status === 'completed' && t('completed')}
          </span>
        </div>

        <div className="tournament-info">
          <div className="info-row">
            <span className="info-label">{t('startDate')}:</span>
            <span className="info-value">{date}</span>
          </div>
          <div className="info-row">
            <span className="info-label">{t('location')}:</span>
            <span className="info-value">{location}</span>
          </div>
          <div className="info-row">
            <span className="info-label">{t('details')}:</span>
            <span className="info-value">{details}</span>
          </div>
        </div>
      </div>
    </Link>
  );
}

export default TournamentCard;
