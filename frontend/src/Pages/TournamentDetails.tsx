import { useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getTournamentById, trackTournamentActivity, startNextRound, addParticipant, joinTournament, approveParticipant, rejectParticipant, withdrawParticipant } from '../services/AuthService';
import { toast } from 'react-toastify';
import type { Tournament } from '../services/AuthService';
import '../assets/styles/tournamentDetails.css';
import { useAuth } from '../context/useAuth';
import StandingsView from '../Components/StandingsView';
import RoundsView from '../Components/RoundsView';
import BracketView from '../Components/BracketView';

import ConfirmationModal from '../Components/ConfirmationModal';
import UserSearchModal from '../Components/UserSearchModal';

function TournamentDetails() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('mainPage');
  const { user } = useAuth(); // Get current user
  const [tournament, setTournament] = useState<Tournament | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshKey, setRefreshKey] = useState(0);
  
  // Modal State
  const [modalConfig, setModalConfig] = useState<{
      isOpen: boolean;
      title: string;
      message: string;
      onConfirm: (val?: string) => void;
      isDanger: boolean;
      showInput?: boolean;
      inputPlaceholder?: string;
  }>({
      isOpen: false,
      title: '',
      message: '',
      onConfirm: () => {},
      isDanger: false,
      showInput: false,
      inputPlaceholder: ''
  });

  const [searchModalOpen, setSearchModalOpen] = useState(false);

  const closeModal = () => setModalConfig(prev => ({ ...prev, isOpen: false }));

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

  const handleStartRound = () => {
    setModalConfig({
        isOpen: true,
        title: t('confirm.startRound.title'),
        message: t('confirm.startRound.message'),
        isDanger: false,
        onConfirm: async () => {
            if (!id) return;
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
    });
  };

  const handleJoin = async () => {
    if (!id) return;
    const result = await joinTournament(id);
    if (result) {
        if (result.status === 'Joined') toast.success(t('joinedSuccess') || "Successfully Joined!");
        else if (result.status === 'Waitlist') toast.warning(t('waitlistJoined') || "You are on the waitlist.");
        else if (result.status === 'Pending') toast.info(t('pendingRequest') || "Request sent. Waiting for approval.");
        
        setRefreshKey(prev => prev + 1);
    } else {
        toast.error("Failed to join.");
    }
  };

  const handleWithdraw = () => {
    if (!id || !user) return;
    
    setModalConfig({
        isOpen: true,
        title: t('confirm.withdraw.title'),
        message: t('confirm.withdraw.message'),
        isDanger: true,
        onConfirm: async () => {
            const success = await withdrawParticipant(id, user.id);
            if (success) {
                toast.success(t('withdrawSuccess') || "Withdrawn successfully.");
                setRefreshKey(prev => prev + 1);
            } else {
                toast.error("Failed to withdraw.");
            }
        }
    });
  };

  const handleAddParticipant = () => {
    setSearchModalOpen(true);
  };

  const onAddUser = async (username: string) => {
    if (username) {
        setLoading(true);
        const success = await addParticipant(id!, username);
        if (success) {
            toast.success(t('participantAdded'));
            setRefreshKey(prev => prev + 1);
        } else {
            toast.error(t('addParticipantError'));
        }
        setLoading(false);
    }
  };

  const [activeTab, setActiveTab] = useState<'overview' | 'rounds' | 'standings'>('overview');

  if (loading) {
    return <div className="loading-container">{t('loading')}</div>;
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
                          {t('tournament.startNextRound')}
                      </button>
                  )}
                  
                   {canJoin && (
                     <button className="td-btn td-btn-primary" onClick={handleJoin}>
                       {t('tournament.join')}
                     </button>
                   )}
                   
                   {isParticipant && (
                       <button className="td-btn td-btn-danger" onClick={handleWithdraw}>
                           {t('tournament.withdraw')}
                       </button>
                   )}
                   
                   {isOrganizer && (
                       <button className="td-btn td-btn-secondary" onClick={() => window.location.href = `/tournament/edit/${id}`}>
                           ✏️ {t('common.edit')}
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
              {t('tournament.tabs.overview')}
          </button>
          <button 
            className={`td-tab ${activeTab === 'rounds' ? 'active' : ''}`} 
            onClick={() => setActiveTab('rounds')}
          >
              {t('tournament.tabs.rounds')}
          </button>
          <button 
            className={`td-tab ${activeTab === 'standings' ? 'active' : ''}`} 
            onClick={() => setActiveTab('standings')}
          >
              {t('tournament.tabs.standings')}
          </button>
      </div>

      <div className="td-content">
          {activeTab === 'overview' && (
          <div className="td-overview">
             <div className="td-description-box" dangerouslySetInnerHTML={{ __html: tournament.details || t('noDescription') }} />
             
             {/* PENDING REQUESTS (Organizer Only) */}
             {isOrganizer && tournament.participants.some(p => p.status === 'PendingApproval') && (
                 <div className="td-participants-section" style={{
                     marginBottom: '2rem',
                     padding: '1rem',
                     borderRadius: '8px',
                     border: '1px solid rgba(241, 196, 15, 0.3)', // Amber border
                     background: 'rgba(241, 196, 15, 0.05)' // Dark Amber subtle bg
                 }}>
                     <h3 style={{color: '#f1c40f', borderBottomColor: 'rgba(241, 196, 15, 0.2)'}}>⚠️ {t('tournament.pendingRequests')}</h3>
                     <div className="pending-list" style={{display: 'flex', flexDirection: 'column', gap: '0.8rem'}}>
                         {tournament.participants.filter(p => p.status === 'PendingApproval').map(p => (
                             <div key={p.id} className="participant-card" style={{
                                 background: 'rgba(0,0,0,0.2)',
                                 border: '1px solid rgba(241, 196, 15, 0.2)',
                                 width: '100%' // Ensure full width
                             }}>
                                 <div className="p-avatar" style={{background: '#f39c12'}}>{p.username.charAt(0).toUpperCase()}</div>
                                 <span className="p-name" style={{fontSize: '1.1rem'}}>{p.username}</span>
                                 <div className="p-actions" style={{marginLeft: 'auto', display: 'flex', gap: '8px'}}>
                                     <button className="td-btn" style={{
                                         background: '#27ae60', 
                                         color: 'white',
                                         padding: '6px 16px', 
                                         fontSize: '0.9rem',
                                         fontWeight: 'bold'
                                     }} onClick={async (e) => {
                                         e.stopPropagation();
                                         if(await approveParticipant(id!, p.id)) {
                                             toast.success("Approved!");
                                             setRefreshKey(x => x + 1);
                                         }
                                     }}>{t('common.approve')}</button>
                                     <button className="td-btn" style={{
                                         background: '#c0392b', 
                                         color: 'white',
                                         padding: '6px 16px', 
                                         fontSize: '0.9rem',
                                         fontWeight: 'bold'
                                     }} onClick={async (e) => {
                                         e.stopPropagation();
                                         if(await rejectParticipant(id!, p.id)) {
                                             toast.success("Rejected");
                                             setRefreshKey(x => x + 1);
                                         }
                                     }}>{t('common.reject')}</button>
                                 </div>
                             </div>
                         ))}
                     </div>
                 </div>
             )}

             {/* WAITLIST */}
             {tournament.participants.some(p => p.status === 'Waitlist') && (
                 <div className="td-participants-section" style={{
                     marginBottom: '2rem',
                     padding: '1rem',
                     borderRadius: '8px',
                     border: '1px solid rgba(52, 152, 219, 0.3)', 
                     background: 'rgba(52, 152, 219, 0.05)'
                 }}>
                     <h3 style={{color: '#3498db', borderBottomColor: 'rgba(52, 152, 219, 0.2)'}}>⏳ {t('tournament.waitlist')}</h3>
                     <div className="participant-list">
                         {tournament.participants.filter(p => p.status === 'Waitlist').map(p => (
                             <div key={p.id} className="participant-card" style={{background: 'rgba(0,0,0,0.2)'}}>
                                 <div className="p-avatar" style={{background: '#7f8c8d'}}>{p.username.charAt(0).toUpperCase()}</div>
                                 <span className="p-name" style={{color: '#bdc3c7'}}>{p.username}</span>
                                 <span style={{fontSize:'0.8rem', marginLeft:'auto', fontStyle:'italic', color: '#7f8c8d'}}>
                                      Waiting
                                 </span>
                             </div>
                         ))}
                     </div>
                 </div>
             )}

             <div className="td-participants-section">
                 <div style={{display:'flex', justifyContent:'space-between', alignItems:'center'}}>
                    <h3>{t('participants')} ({tournament.participants.filter(p => p.status === 'Confirmed').length})</h3>
                    {isOrganizer && (
                        <button className="td-btn td-btn-secondary" onClick={handleAddParticipant} style={{fontSize:'0.8rem', padding:'4px 8px'}}>
                            + {t('tournament.addGeneric')}
                        </button>
                    )}
                 </div>
                 <div className="participant-list">
                     {tournament.participants.filter(p => p.status === 'Confirmed').length > 0 ? (
                         tournament.participants.filter(p => p.status === 'Confirmed').map(p => (
                             <div 
                                 key={p.id} 
                                 className="participant-card" 
                                 onClick={() => window.location.href = `/tournament/${id}/participant/${p.id}`}
                                 style={{cursor: 'pointer'}}
                                 title="View Profile"
                             >
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
      <ConfirmationModal 
          isOpen={modalConfig.isOpen}
          title={modalConfig.title}
          message={modalConfig.message}
          onConfirm={modalConfig.onConfirm}
          onClose={closeModal}
          isDanger={modalConfig.isDanger}
          showInput={modalConfig.showInput}
          inputPlaceholder={modalConfig.inputPlaceholder}
      />
      <UserSearchModal 
        isOpen={searchModalOpen}
        onClose={() => setSearchModalOpen(false)}
        onAdd={onAddUser}
        title={t('modal.addParticipant.title')}
      />
    </div>
  );
}

export default TournamentDetails;
