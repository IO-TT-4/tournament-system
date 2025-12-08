import { useTranslation } from 'react-i18next';
import '../styles/tournamentCard.css';

export default function TournamentCard({ title, metaData }) {
  const { t } = useTranslation('mainPage');
  return (
    <div className="tournament-card">
      <h3>{title}</h3>
      <div className="tournament-card-meta">
        {metaData?.startDate && metaData?.endDate ? (
          <div>
            {t('date')}: {metaData.startDate} - {metaData.endDate}
          </div>
        ) : null}
        {metaData?.numberOfRegisteredPlayers &&
        metaData.numberOfRegisteredPlayers != 0 ? (
          <div>
            {t('rounds')}: {metaData.numberOfRegisteredPlayers}
          </div>
        ) : null}
        {metaData?.numberOfRounds ? (
          <div>
            {t('NoPlayers')}: {metaData.numberOfRounds}
          </div>
        ) : null}
      </div>
    </div>
  );
}
