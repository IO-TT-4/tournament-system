import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router';
import { getTournamentById, getStandings, getMatches } from '../services/AuthService';
import type { Tournament, StandingsEntry, Match } from '../services/AuthService';
import '../assets/styles/index.css';
import '../assets/styles/tournamentParticipant.css';

export default function TournamentParticipant() {
    const { id, userId } = useParams<{ id: string, userId: string }>();
    const navigate = useNavigate();

    const [tournament, setTournament] = useState<Tournament | null>(null);
    const [stats, setStats] = useState<StandingsEntry | null>(null);
    const [matches, setMatches] = useState<Match[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            if (!id || !userId) return;
            
            setLoading(true);
            try {
                // Parallel fetch
                const [tData, sData, mData] = await Promise.all([
                    getTournamentById(id),
                    getStandings(id),
                    getMatches(id)
                ]);

                if (tData) setTournament(tData);
                
                if (sData) {
                    const userStats = sData.find(s => s.userId === userId);
                    setStats(userStats || null);
                }

                if (mData) {
                    // Filter matches involving this user
                    const userMatches = mData.filter(m => 
                        m.playerHomeId === userId || m.playerAwayId === userId
                    ).sort((a,b) => a.roundNumber - b.roundNumber);
                    setMatches(userMatches);
                }
            } catch (error) {
                console.error("Error fetching participant data:", error);
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [id, userId]);

    if (loading) return <div className="loading-container">Loading profile...</div>;
    if (!tournament || !stats) return <div className="container tp-not-found">Participant not found in this tournament.</div>;

    return (
        <div className="container tp-container">
            <button 
                onClick={() => navigate(`/tournament/${id}`)} 
                className="tp-back-btn"
            >
                ← Back to Tournament
            </button>

            <div className="glass-panel tp-header-panel">
                <div className="tp-profile-header">
                    <div className="tp-avatar-large">
                        {stats.username.charAt(0).toUpperCase()}
                    </div>
                    <div>
                        <h1 className="tp-username">{stats.username}</h1>
                        <Link to={`/user/${userId}`} className="tp-view-profile-link">
                            View Full Profile →
                        </Link>
                        <p className="tp-tournament-ref">Tournament: <strong className="tp-tournament-name">{tournament.title}</strong></p>
                         {stats.isWithdrawn && <span className="tp-withdrawn-tag">WITHDRAWN</span>}
                    </div>
                </div>

                <div className="tp-stats-grid">
                    <div className="tp-stat-box">
                        <div className="tp-stat-label">Rank</div>
                        <div className="tp-stat-value">#{stats.ranking}</div>
                    </div>
                    <div className="tp-stat-box">
                        <div className="tp-stat-label">Score</div>
                        <div className="tp-stat-value tp-stat-score">{stats.score}</div>
                    </div>
                    <div className="tp-stat-box">
                        <div className="tp-stat-label">Wins</div>
                        <div className="tp-stat-value">{stats.wins}</div>
                    </div>
                    <div className="tp-stat-box">
                        <div className="tp-stat-label">Draws</div>
                        <div className="tp-stat-value">{stats.draws}</div>
                    </div>
                    <div className="tp-stat-box">
                        <div className="tp-stat-label">Losses</div>
                        <div className="tp-stat-value tp-stat-loss">{stats.losses}</div>
                    </div>
                </div>

                {/* Detailed TieBreakers if Swiss */}
                <div className="tp-tiebreakers">
                    Buchholz: {stats.buchholz} | SB: {stats.tieBreakerValues['SONNEBORN_BERGER']?.toFixed(2)} | Progressive: {stats.tieBreakerValues['PROGRESSIVE']}
                </div>
            </div>

            <h2 className="tp-history-header">Match History</h2>
            <div className="glass-panel tp-match-list">
                {matches.length === 0 ? (
                    <div style={{padding:'20px', textAlign:'center', color:'#888'}}>No matches played yet.</div>
                ) : (
                    matches.map(m => {
                        const isHome = m.playerHomeId === userId;
                        const opponentName = isHome ? m.playerAwayName : m.playerHomeName;
                        const myScore = m.result ? (isHome ? m.result.scoreA : m.result.scoreB) : '-';
                        const oppScore = m.result ? (isHome ? m.result.scoreB : m.result.scoreA) : '-';
                        
                        let resultColor = '#ccc';
                        let resultText = 'PENDING';
                        
                        if (m.isCompleted && m.result) {
                            if (myScore > oppScore) { resultColor = '#67c23a'; resultText = 'WIN'; }
                            else if (myScore < oppScore) { resultColor = '#f56c6c'; resultText = 'LOSS'; }
                            else { resultColor = '#e6a23c'; resultText = 'DRAW'; }
                        }

                        if (m.playerAwayName === 'BYE' || m.playerHomeName === 'BYE') {
                            resultText = 'BYE';
                            resultColor = '#409eff';
                        }

                        return (
                            <div key={m.id} className="tp-match-item">
                                <div className="tp-match-round">Round {m.roundNumber}</div>
                                <div className="tp-match-vs">
                                    <span className="tp-vs-label">vs</span>
                                    <span className="tp-opponent-name">{opponentName || 'Unknown'}</span>
                                    {/* Side Indicator */}
                                    {m.playerAwayName !== 'BYE' && m.playerHomeName !== 'BYE' && (
                                        <span className={`tp-side-badge ${isHome ? 'tp-side-white' : 'tp-side-black'}`}>
                                            {isHome ? 'White' : 'Black'}
                                        </span>
                                    )}
                                </div>
                                <div className="tp-match-result">
                                    <div className="tp-result-line">
                                        <span className="tp-result-tag" style={{color: resultColor}}>{resultText}</span>
                                        <span>{myScore} - {oppScore}</span>
                                    </div>
                                    <div className="tp-table-info">Table {m.tableNumber}</div>
                                </div>
                            </div>
                        );
                    })
                )}
            </div>
        </div>
    );
}
