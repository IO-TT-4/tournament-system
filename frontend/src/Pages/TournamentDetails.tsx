import { useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getTournamentById, trackTournamentActivity, startNextRound, addParticipant } from '../services/AuthService';
import { toast } from 'react-toastify';
import type { Tournament } from '../services/AuthService';
import '../assets/styles/tournamentDetails.css';
import { useAuth } from '../context/useAuth';
import StandingsView from '../Components/StandingsView';
import RoundsView from '../Components/RoundsView';
import BracketView from '../Components/BracketView';

function TournamentDetails() {

  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('mainPage');
  const { user } = useAuth(); // Get current user
  const [tournament, setTournament] = useState<Tournament | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshKey, setRefreshKey] = useState(0);

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
  }, [id, refreshKey]);

  const handleStartRound = async () => {
    if (!id || !tournament) return;
    if (window.confirm(t('confirmStartRound') || "Are you sure you want to start the next round?")) {
      setLoading(true);
      const success = await startNextRound(id);
      if (success) {
          toast.success(t('roundStarted') || "Next round started successfully!");
          setRefreshKey(prev => prev + 1);
      } else {
        toast.error(t('roundStartError') || "Failed to start round.");
      }
      setLoading(false);
    }
  };

  const handleJoin = () => {
    toast.info("Join functionality coming soon!");
    // Implement join logic here
  };

  const handleWithdraw = () => {
    toast.info("Withdraw functionality coming soon!");
    // Implement withdraw logic here
  };

  const handleAddParticipant = async () => {
      const username = prompt(t('enterUsername') || "Enter username to add:");
      if (username) {
          setLoading(true);
          const success = await addParticipant(id!, username);
          if (success) {
              toast.success(t('participantAdded') || "Participant added successfully!");
              const data = await getTournamentById(id!);
              setTournament(data);
          } else {
              toast.error(t('addParticipantError') || "Failed to add participant. Check if user exists.");
          }
          setLoading(false);
      }
  };

  const [activeTab, setActiveTab] = useState<'overview' | 'rounds' | 'standings'>('overview');

  if (loading) {
    return <div className="loading-container">{t('loading') || 'Loading...'}</div>;
  }

  if (!tournament) {
    return <div className="error-container">{t('tournamentNotFound')}</div>;
  }

  const isOrganizer = user?.id === tournament.organizerId;
  const isModerator = user?.id ? tournament.moderatorIds.includes(user.id) : false;
  const isParticipant = user?.id ? tournament.participants.some(p => p.id === user.id) : false;
  const canJoin = user && !isOrganizer && !isModerator && !isParticipant;
  
  const showStartRoundBtn = (isOrganizer || isModerator) && (tournament.status === 'active' || tournament.status === 'upcoming');

  return (
    <div className="tournament-details-page">
      {/* HEADER SECTION */}
      <div className="td-header">
           <div className="td-header-content">
               <div className="td-emblem-container">
                    <img 
                        src={tournament.emblem === 'default' 
                            ? 'https://cdn-icons-png.flaticon.com/512/10604/10604085.png' 
                            : tournament.emblem} 
                        alt="Emblem" 
                        className="td-big-emblem"
                    />
               </div>
               <div className="td-info-main">
                   <h1>{tournament.title}</h1>
                   <div className="td-badges">
                       <span className="td-badge game-badge">{tournament.game.name}</span>
                       <span className={`td-badge status-badge status-${tournament.status}`}>
                           {t(tournament.status)}
                       </span>
                       <span className="td-badge system-badge">{t(`systemTypes.${tournament.systemType}`) || tournament.systemType}</span>
                   </div>
                   <div className="td-meta-row">
                       <span>📅 {tournament.date}</span>
                       <span>📍 {tournament.location}</span>
                       <span>👤 {tournament.participants.length} / {tournament.playerLimit}</span>
                   </div>
               </div>
           </div>

           {/* ACTION BUTTONS */}
           <div className="td-actions-header">
                  {showStartRoundBtn && (
                      <button 
                        className="td-btn td-btn-premium" 
                        onClick={handleStartRound}
                      >
                          {t('startNextRound') || 'Start Next Round'}
                      </button>
                  )}
                  
                   {canJoin && (
                     <button className="td-btn td-btn-primary" onClick={handleJoin}>
                       {t('joinTournament')}
                     </button>
                   )}
                   
                   {isParticipant && (
                       <button className="td-btn td-btn-danger" onClick={handleWithdraw}>
                           {t('withdraw')}
                       </button>
                   )}
                   
                   {isOrganizer && (
                       <button className="td-btn td-btn-secondary" onClick={() => window.location.href = `/tournament/edit/${id}`}>
                           ✏️ {t('edit') || 'Edit'}
                       </button>
                   )}
           </div>
      </div>

      {/* TABS NAVIGATION */}
      <div className="td-tabs">
          <button 
            className={`td-tab ${activeTab === 'overview' ? 'active' : ''}`} 
            onClick={() => setActiveTab('overview')}
          >
              {t('overview') || 'Overview'}
          </button>
          <button 
            className={`td-tab ${activeTab === 'rounds' ? 'active' : ''}`} 
            onClick={() => setActiveTab('rounds')}
          >
              {t('rounds') || 'Rounds'}
          </button>
          <button 
            className={`td-tab ${activeTab === 'standings' ? 'active' : ''}`} 
            onClick={() => setActiveTab('standings')}
          >
              {t('standings') || 'Standings'}
          </button>
      </div>

      <div className="td-content">
          {activeTab === 'overview' && (
              <div className="td-overview">
                 <div className="td-description-box" dangerouslySetInnerHTML={{ __html: tournament.details || t('noDescription') }} />
                 
                 <div className="td-participants-section">
                     <div style={{display:'flex', justifyContent:'space-between', alignItems:'center'}}>
                        <h3>{t('participants')} ({tournament.participants.length})</h3>
                        {isOrganizer && (
                            <button className="td-btn td-btn-secondary" onClick={handleAddParticipant} style={{fontSize:'0.8rem', padding:'4px 8px'}}>
                                + {t('addManual') || 'Add Generic'}
                            </button>
                        )}
                     </div>
                     <div className="participant-list">
                         {tournament.participants.length > 0 ? (
                             tournament.participants.map(p => (
                                 <div key={p.id} className="participant-card">
                                     <div className="p-avatar">{p.username.charAt(0).toUpperCase()}</div>
                                     <span className="p-name">{p.username}</span>
                                 </div>
                             ))
                         ) : <span>{t('noParticipants')}</span>}
                     </div>
                 </div>
              </div>
          )}

          {activeTab === 'rounds' && (
              <div className="td-rounds-view">
                  {tournament.systemType === 'SINGLE_ELIMINATION' || tournament.systemType === 'DOUBLE_ELIMINATION' ? (
                      <BracketView 
                        key={refreshKey} 
                        tournamentId={id!} 
                        isOrganizer={isOrganizer}
                        isModerator={isModerator}
                      />
                  ) : (
                      <RoundsView 
                        key={refreshKey} 
                        tournamentId={id!} 
                        isOrganizer={isOrganizer}
                        isModerator={isModerator}
                      />
                  )}
              </div>
          )}

          {activeTab === 'standings' && (
              <div className="td-standings-view">
                  <StandingsView tournamentId={id!} />
              </div>
          )}
      </div>
    </div>
  );
}

export default TournamentDetails;
