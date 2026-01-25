import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getTournamentById, trackTournamentActivity } from '../services/AuthService';
import type { Tournament } from '../services/AuthService';
import '../assets/styles/tournaments.css'; // Reusing existing styles for now or create new ones

function TournamentDetails() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('mainPage');
  const [tournament, setTournament] = useState<Tournament | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchTournament = async () => {
      if (!id) return;
      setLoading(true);
      
      // Track view
      trackTournamentActivity(id, 'view');

      const data = await getTournamentById(id);
      console.log('Tournament Details received:', data);
      setTournament(data);
      setLoading(false);
    };

    fetchTournament();
  }, [id]);

  if (loading) {
    return <div className="loading-container">Loading...</div>;
  }

  if (!tournament) {
    return (
      <div className="error-container">
        <h2>{t('tournamentNotFound')}</h2>
        <Link to="/tournaments" className="back-link">Back to Tournaments</Link>
      </div>
    );
  }

  return (
    <div className="tournament-details-page">
       <div className="tournaments-background" />
       
       <div className="container" style={{ position: 'relative', zIndex: 1, paddingTop: '100px' }}>
          <Link to="/tournaments" className="back-btn">&larr; {t('back') || 'Back'}</Link>
          
          <div className="details-card">
            <div className="details-header">
                <h1>{tournament.title}</h1>
                <span className={`status-badge ${tournament.status}`}>{tournament.status}</span>
            </div>
            
            <div className="details-grid">
                <div className="detail-item">
                    <strong>{t('game') || 'Game'}:</strong> {tournament.game.name}
                </div>
                <div className="detail-item">
                    <strong>{t('startDate')}:</strong> {tournament.date}
                </div>
                <div className="detail-item">
                    <strong>{t('location')}:</strong> {tournament.location}
                </div>
                <div className="detail-item">
                    <strong>{t('systemType') || 'System'}:</strong> {tournament.systemType}
                </div>
                {tournament.numberOfRounds && (
                    <div className="detail-item">
                        <strong>{t('rounds') || 'Rounds'}:</strong> {tournament.numberOfRounds}
                    </div>
                )}
                <div className="detail-item">
                    <strong>{t('maxPlayers') || 'Max Players'}:</strong> {tournament.playerLimit}
                </div>
            </div>

            <div className="details-map-placeholder">
                {/* Map integration could go here */}
                {tournament.lat && tournament.lng && (
                    <div className="map-info">
                        Coordinates: {tournament.lat}, {tournament.lng}
                    </div>
                )}
            </div>
          </div>
       </div>
    </div>
  );
}

export default TournamentDetails;
