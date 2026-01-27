import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getTournamentById } from '../services/AuthService';
import { api } from '../api/api';
import '../assets/styles/matchManage.css';

interface MatchData {
    id: string;
    playerHomeId: string;
    playerAwayId: string;
    playerHomeName: string;
    playerAwayName: string;
    scoreA: number | null;
    scoreB: number | null;
    tournamentId: string;
    enableMatchEvents: boolean;
}

interface MatchEvent {
    id: string;
    eventType: string;
    playerId?: string;
    playerName?: string;
    timestamp: string;
    minuteOfPlay?: number;
    description?: string;
}

export default function MatchManage() {
    const { matchId } = useParams<{ matchId: string }>();
    const { t } = useTranslation('mainPage');
    
    const [match, setMatch] = useState<MatchData | null>(null);
    const [events, setEvents] = useState<MatchEvent[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isAuthorized, setIsAuthorized] = useState(false);
    
    // Simple mode state
    const [scoreHome, setScoreHome] = useState<number>(0);
    const [scoreAway, setScoreAway] = useState<number>(0);
    const [saving, setSaving] = useState(false);

    // Manual event form state
    const [manualEvent, setManualEvent] = useState({
        eventType: 'GOAL',
        playerId: '',
        minute: '',
        description: ''
    });
    
    
    const getEventTypeKey = (type: string) => {
        const lower = type.toLowerCase();
        if (type === 'YELLOW_CARD') return 'match.events.types.yellowCard';
        if (type === 'RED_CARD') return 'match.events.types.redCard';
        return `match.events.types.${lower}`;
    };
    
    useEffect(() => {
        loadMatchData();
    }, [matchId]);
    
    const loadMatchData = async () => {
        if (!matchId) return;
        try {
            // Fetch match details
            const matchRes = await api.get(`/match/${matchId}`);
            const matchData = matchRes.data;
            
            // Check tournament for enableMatchEvents and authorization
            const tournamentData = await getTournamentById(matchData.tournamentId);
            if (!tournamentData) {
                setError(t('error.tournamentNotFound'));
                setLoading(false);
                return;
            }
            
            // Check authorization (organizer or moderator)
            const userStr = localStorage.getItem('user');
            const user = userStr ? JSON.parse(userStr) : null;
            const userId = user?.id;
            
            const isOrganizer = tournamentData.organizerId === userId;
            const isModerator = tournamentData.moderatorIds?.includes(userId || '') ?? false;
            
            if (!isOrganizer && !isModerator) {
                setError(t('error.unauthorizedMatch'));
                setIsAuthorized(false);
                setLoading(false);
                return;
            }
            
            setIsAuthorized(true);
            setMatch({
                ...matchData,
                enableMatchEvents: tournamentData.enableMatchEvents ?? false
            });
            setScoreHome(matchData.scoreA ?? 0);
            setScoreAway(matchData.scoreB ?? 0);
            
            // Load events if enabled
            if (tournamentData.enableMatchEvents) {
                const eventsRes = await api.get(`/match/${matchId}/events`);
                setEvents(eventsRes.data);
            }
            
            setLoading(false);
        } catch (err: any) {
            console.error('Error loading match data:', err);
            setError(err.response?.data || 'Failed to load match data');
            setLoading(false);
        }
    };
    
    const addEvent = async (eventType: string, playerId?: string, minute?: number, description?: string) => {
        if (!matchId) return;
        
        try {
            await api.post(`/match/${matchId}/events`, {
                eventType,
                playerId: playerId || undefined,
                minuteOfPlay: minute,
                description
            });
            
            await loadMatchData(); // Reload to get updated score and events
            // Reset manual form if it was used
            setManualEvent(prev => ({ ...prev, minute: '', description: '' }));
        } catch (err) {
            console.error('Error adding event:', err);
        }
    };
    
    const saveSimpleResult = async () => {
        if (!matchId) return;
        setSaving(true);
        
        try {
            await api.post(`/match/${matchId}/result`, {
                scoreA: scoreHome,
                scoreB: scoreAway
            });
            await loadMatchData();
        } catch (err) {
            console.error('Error saving result:', err);
        } finally {
            setSaving(false);
        }
    };
    
    if (loading) return <div className="match-manage-container"><div className="loading">{t('loading')}</div></div>;
    if (error) return <div className="match-manage-container"><div className="error">{error}</div></div>;
    if (!match) return <div className="match-manage-container"><div className="error">{t('error.matchNotFound')}</div></div>;
    if (!isAuthorized) return <div className="match-manage-container"><div className="error">{t('error.unauthorizedMatch')}</div></div>;
    
    return (
        <div className="match-manage-container">
            <div className="match-manage-header">
                <Link to={`/tournament/${match.tournamentId}`} className="back-link">
                    ← {t('common.backToTournament')}
                </Link>
                <h1>{t('match.management.title')}</h1>
            </div>
            
            {/* Match Header */}
            <div className="match-header">
                <div className="player home">
                    <span className="player-name">{match.playerHomeName || t('common.playerA')}</span>
                    <span className="score">{match.scoreA ?? 0}</span>
                </div>
                <div className="vs">vs</div>
                <div className="player away">
                    <span className="score">{match.scoreB ?? 0}</span>
                    <span className="player-name">{match.playerAwayName || t('common.playerB')}</span>
                </div>
            </div>
            
            {match.enableMatchEvents ? (
                /* Live Match Dashboard */
                <div className="live-dashboard">
                    <h2>{t('match.events.title')}</h2>
                    
                    {/* Manual Event Form */}
                    <div className="manual-event-form">
                        <h3>{t('match.events.addManual')}</h3>
                        <div className="form-grid">
                            <div className="form-group">
                                <label>{t('match.events.type')}</label>
                                <select 
                                    value={manualEvent.eventType}
                                    onChange={(e) => setManualEvent({...manualEvent, eventType: e.target.value})}
                                >
                                    <option value="GOAL">⚽ {t('match.events.types.goal')}</option>
                                    <option value="FOUL">🦶 {t('match.events.types.foul')}</option>
                                    <option value="YELLOW_CARD">🟨 {t('match.events.types.yellowCard')}</option>
                                    <option value="RED_CARD">🟥 {t('match.events.types.redCard')}</option>
                                    <option value="SUBSTITUTION">🔄 {t('match.events.types.substitution')}</option>
                                    <option value="OTHER">📝 {t('match.events.types.other')}</option>
                                </select>
                            </div>
                            <div className="form-group">
                                <label>{t('match.events.player')}</label>
                                <select 
                                    value={manualEvent.playerId}
                                    onChange={(e) => setManualEvent({...manualEvent, playerId: e.target.value})}
                                >
                                    <option value="">-- {t('common.none')} --</option>
                                    <option value={match.playerHomeId}>{match.playerHomeName} ({t('common.home')})</option>
                                    <option value={match.playerAwayId}>{match.playerAwayName} ({t('common.away')})</option>
                                </select>
                            </div>
                            <div className="form-group">
                                <label>{t('match.events.minute')}</label>
                                <input 
                                    type="number" 
                                    value={manualEvent.minute}
                                    onChange={(e) => setManualEvent({...manualEvent, minute: e.target.value})}
                                    placeholder={t('match.events.minutePlaceholder')}
                                    min={0}
                                />
                            </div>
                            <div className="form-group full-width">
                                <label>{t('common.description')}</label>
                                <input 
                                    type="text" 
                                    value={manualEvent.description}
                                    onChange={(e) => setManualEvent({...manualEvent, description: e.target.value})}
                                    placeholder={t('match.events.descPlaceholder')}
                                />
                            </div>
                        </div>
                        <button 
                            className="td-btn td-btn-primary add-manual-btn"
                            onClick={() => addEvent(manualEvent.eventType, manualEvent.playerId, parseInt(manualEvent.minute) || undefined, manualEvent.description)}
                        >
                            ➕ {t('match.events.addBtn')}
                        </button>
                    </div>

                    {/* Quick Action Buttons */}
                    <div className="action-buttons">
                        <h3>{t('match.events.quickHits')}</h3>
                        <div className="action-row goals">
                            <button 
                                className="action-btn goal home"
                                onClick={() => addEvent('GOAL', match.playerHomeId)}
                            >
                                ⚽ {t('match.events.types.goal')}<br/>{match.playerHomeName}
                            </button>
                            <button 
                                className="action-btn goal away"
                                onClick={() => addEvent('GOAL', match.playerAwayId)}
                            >
                                ⚽ {t('match.events.types.goal')}<br/>{match.playerAwayName}
                            </button>
                        </div>
                    </div>
                    
                    {/* Event Timeline */}
                    <div className="event-timeline">
                        <h3>{t('match.events.timeline')}</h3>
                        {events.length === 0 ? (
                            <p className="no-events">{t('match.events.noEvents')}</p>
                        ) : (
                            <ul className="events-list">
                                {[...events].sort((a,b) => (b.minuteOfPlay || 0) - (a.minuteOfPlay || 0) || new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()).map(event => (
                                    <li key={event.id} className={`event-item ${event.eventType.toLowerCase()}`}>
                                        <div className="event-icon">
                                            {event.eventType === 'GOAL' ? '⚽' : 
                                             event.eventType === 'YELLOW_CARD' ? '🟨' : 
                                             event.eventType === 'RED_CARD' ? '🟥' : 
                                             event.eventType === 'FOUL' ? '🦶' : '📝'}
                                        </div>
                                        <div className="event-details">
                                            <div className="event-main">
                                                <span className="event-minute">{event.minuteOfPlay ? `${event.minuteOfPlay}'` : ''}</span>
                                                <span className="event-type-label">
                                                    {t(getEventTypeKey(event.eventType)) === getEventTypeKey(event.eventType)
                                                        ? event.eventType 
                                                        : t(getEventTypeKey(event.eventType))}
                                                </span>
                                                {event.playerName && <span className="event-player">- {event.playerName}</span>}
                                            </div>
                                            {event.description && <div className="event-desc">{event.description}</div>}
                                            <div className="event-time-recorded">
                                                {new Date(event.timestamp).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })}
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                    
                    {/* Final Result Override */}
                    <details className="final-result-override">
                        <summary>{t('match.result.override')}</summary>
                        <div className="simple-inputs">
                            <input 
                                type="number" 
                                value={scoreHome}
                                onChange={(e) => setScoreHome(parseInt(e.target.value) || 0)}
                                min={0}
                            />
                            <span>:</span>
                            <input 
                                type="number" 
                                value={scoreAway}
                                onChange={(e) => setScoreAway(parseInt(e.target.value) || 0)}
                                min={0}
                            />
                            <button onClick={saveSimpleResult} disabled={saving}>
                                {saving ? t('common.saving') : t('common.save')}
                            </button>
                        </div>
                    </details>
                </div>
            ) : (
                /* Simple Scoreboard Editor */
                <div className="simple-editor">
                    <h2>{t('match.result.setTitle')}</h2>
                    
                    <div className="simple-form">
                        <div className="score-input-group">
                            <label>{t('match.result.homeScore')}</label>
                            <input 
                                type="number" 
                                value={scoreHome}
                                onChange={(e) => setScoreHome(parseInt(e.target.value) || 0)}
                                min={0}
                                className="score-input"
                            />
                        </div>
                        
                        <div className="score-divider">:</div>
                        
                        <div className="score-input-group">
                            <label>{t('match.result.awayScore')}</label>
                            <input 
                                type="number" 
                                value={scoreAway}
                                onChange={(e) => setScoreAway(parseInt(e.target.value) || 0)}
                                min={0}
                                className="score-input"
                            />
                        </div>
                    </div>
                    
                    <button 
                        className="save-btn"
                        onClick={saveSimpleResult}
                        disabled={saving}
                    >
                        {saving ? t('common.saving') : t('common.saveResult')}
                    </button>
                </div>
            )}
        </div>
    );
}
