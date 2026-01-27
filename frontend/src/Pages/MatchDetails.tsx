import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router';
import { useTranslation } from 'react-i18next';
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

export default function MatchDetails() {
    const { matchId } = useParams<{ matchId: string }>();
    const { t } = useTranslation('mainPage');
    
    const [match, setMatch] = useState<MatchData | null>(null);
    const [events, setEvents] = useState<MatchEvent[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadMatchData();
        
        // Poll for updates every 3 seconds
        const interval = setInterval(loadMatchData, 3000);
        return () => clearInterval(interval);
    }, [matchId]);
    
    const loadMatchData = async () => {
        if (!matchId) return;
        try {
            const matchRes = await api.get(`/match/${matchId}`);
            const matchData = matchRes.data;
            
            setMatch(matchData);
            
            if (matchData.enableMatchEvents) {
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
    
    if (loading && !match) return <div className="match-manage-container"><div className="loading">{t('common.loading')}</div></div>;
    if (error && !match) return <div className="match-manage-container"><div className="error">{error}</div></div>;
    if (!match) return <div className="match-manage-container"><div className="error">{t('error.matchNotFound')}</div></div>;

    return (
        <div className="match-manage-container">
            <div className="match-manage-header">
                <Link to={`/tournament/${match.tournamentId}`} className="back-link">
                    ← {t('common.backToTournament')}
                </Link>
                <h1>{t('match.details.title')}</h1>
            </div>
            
            {/* Match Header */}
            <div className="match-header">
                <div className="player home">
                    <Link to={`/tournament/${match.tournamentId}/participant/${match.playerHomeId}`} className="player-name-link">
                        <span className="player-name">{match.playerHomeName || t('match.playerA_fallback')}</span>
                    </Link>
                    <span className="score">{match.scoreA ?? 0}</span>
                </div>
                <div className="vs">vs</div>
                <div className="player away">
                    <span className="score">{match.scoreB ?? 0}</span>
                    <Link to={`/tournament/${match.tournamentId}/participant/${match.playerAwayId}`} className="player-name-link">
                         <span className="player-name">{match.playerAwayName || t('match.playerB_fallback')}</span>
                    </Link>
                </div>
            </div>
            
            {match.enableMatchEvents && (
                /* Live Match Dashboard (Read Only) */
                <div className="live-dashboard">
                    <h2>{t('match.events.title')}</h2>
                    
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
                                                <span className="event-type-label">{event.eventType}</span>
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
                </div>
            )}
        </div>
    );
}
