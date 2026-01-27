import { useEffect, useState } from 'react';
import { Link } from 'react-router';
import { getMatches, submitMatchResult } from '../services/AuthService';
import type { Match } from '../services/AuthService';
import { useTranslation } from 'react-i18next';
import { toast } from 'react-toastify';

interface RoundsViewProps {
    tournamentId: string;
    isOrganizer: boolean;
    isModerator: boolean;
}

export default function RoundsView({ tournamentId, isOrganizer, isModerator }: RoundsViewProps) {
    const { t } = useTranslation('mainPage');
    const [matches, setMatches] = useState<Match[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedRound, setSelectedRound] = useState<number>(0);
    const [editingMatchId, setEditingMatchId] = useState<string | null>(null);
    const [scoreA, setScoreA] = useState<number>(0);
    const [scoreB, setScoreB] = useState<number>(0);

    const [finishType, setFinishType] = useState<string>('Normal');

    const fetchMatches = async () => {
         setLoading(true);
         const data = await getMatches(tournamentId);
         setMatches(data);
         
         // Auto-select first incomplete round, or last round if all completed
         if (selectedRound === 0 && data.length > 0) {
             const incompleteMatch = data.filter(m => !m.isCompleted).sort((a,b) => a.roundNumber - b.roundNumber)[0];
             if (incompleteMatch) {
                 setSelectedRound(incompleteMatch.roundNumber);
             } else {
                 const maxRound = Math.max(...data.map(m => m.roundNumber));
                 setSelectedRound(maxRound);
             }
         }
         setLoading(false);
    };

    useEffect(() => {
        fetchMatches();
    }, [tournamentId]);

    const handleEditClick = (match: Match) => {
        setEditingMatchId(match.id);
        const r = match.result;
        setScoreA(r?.scoreA || 0);
        setScoreB(r?.scoreB || 0);
        setFinishType(r?.finishType || 'Normal');
    };

    const handleResultChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const val = e.target.value;
        if (val === "1:0") {
            setScoreA(1); setScoreB(0); setFinishType('Normal');
        } else if (val === "0:1") {
            setScoreA(0); setScoreB(1); setFinishType('Normal');
        } else if (val === "0.5:0.5") {
            setScoreA(0.5); setScoreB(0.5); setFinishType('Normal');
        } else if (val === "+:-") {
            setScoreA(1); setScoreB(0); setFinishType('Walkover'); // Home wins walkover
        } else if (val === "-:+") {
            setScoreA(0); setScoreB(1); setFinishType('Walkover'); // Away wins walkover
        }
    };

    const handleSaveResult = async (matchId: string) => {
        const success = await submitMatchResult(matchId, scoreA, scoreB, finishType);
        if (success) {
            toast.success(t('bracket.resultSaved'));
            setEditingMatchId(null);
            fetchMatches(); // Reload to show new result
        } else {
            toast.error(t('bracket.saveError'));
        }
    };

    if (loading && matches.length === 0) return <div>{t('loading')}</div>;
    if (matches.length === 0) return <div className="no-data">{t('bracket.noMatches')}</div>;

    const rounds = Array.from(new Set(matches.map(m => m.roundNumber))).sort((a,b) => a - b);
    const displayedMatches = matches.filter(m => m.roundNumber === selectedRound);

    const canEdit = isOrganizer || isModerator;

    return (
        <div className="rounds-view">
            {/* Round Filter - Wrapped Layout */}
            <div className="rounds-filter" style={{ display: 'flex', gap: '10px', marginBottom: '1rem', flexWrap: 'wrap', paddingBottom: '5px' }}>
                {rounds.map(r => (
                    <button 
                        key={r} 
                        className={`td-btn ${selectedRound === r ? 'td-btn-primary' : 'td-btn-secondary'}`}
                        onClick={() => setSelectedRound(r)}
                    >
                        {t('common.round')} {r}
                    </button>
                ))}
            </div>

            {/* Matches List */}
            <div className="matches-list" style={{ display: 'grid', gap: '10px' }}>
                {displayedMatches.map(match => (
                    <div key={match.id} className="match-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'rgba(255,255,255,0.05)', padding: '1rem', borderRadius: '8px' }}>
                        <div style={{ width: '40px', fontWeight: 'bold', color: '#666' }}>
                            #{match.tableNumber}
                        </div>
                        <div className="player-home" style={{ flex: 1, textAlign: 'right', fontWeight: match.result && match.result.scoreA > match.result.scoreB ? 'bold' : 'normal' }}>
                            {match.playerHomeName || t('common.playerA')}
                        </div>
                        
                        <div className="match-center" style={{ display:'flex', flexDirection:'column', alignItems:'center', margin: '0 20px' }}>
                            {editingMatchId === match.id ? (
                                    <div className="score-edit" style={{ display:'flex', gap:'5px', alignItems:'center' }}>
                                        <input 
                                            type="number" 
                                            value={scoreA}
                                            onChange={(e) => setScoreA(parseFloat(e.target.value) || 0)}
                                            style={{ width: '50px', padding:'5px', borderRadius:'4px', background:'#333', color:'#fff', border:'1px solid #555', textAlign: 'center' }}
                                        />
                                        <span>:</span>
                                        <input 
                                            type="number" 
                                            value={scoreB}
                                            onChange={(e) => setScoreB(parseFloat(e.target.value) || 0)}
                                            style={{ width: '50px', padding:'5px', borderRadius:'4px', background:'#333', color:'#fff', border:'1px solid #555', textAlign: 'center' }}
                                        />
                                        <select 
                                             value={finishType}
                                             onChange={(e) => setFinishType(e.target.value)}
                                             style={{ padding:'5px', borderRadius:'4px', background:'#333', color:'#fff', border:'1px solid #555', maxWidth: '80px', fontSize: '0.8rem' }}
                                        >
                                            <option value="Normal">Normal</option>
                                            <option value="Walkover">Walkover</option>
                                        </select>
                                        <button className="td-btn td-btn-primary" onClick={() => handleSaveResult(match.id)} style={{ padding:'5px 10px', fontSize:'0.8rem' }}>💾</button>
                                        <button className="td-btn td-btn-secondary" onClick={() => setEditingMatchId(null)} style={{ padding:'5px 10px', fontSize:'0.8rem' }}>❌</button>
                                    </div>
                            ) : (
                                <>
                                    <div className="match-score" style={{ padding: '5px 15px', background: '#222', borderRadius: '4px', fontWeight: 'bold' }}>
                                        {match.isCompleted ? (
                                            match.result?.finishType === 'Walkover' ? 
                                                (match.result.scoreA === 1 ? `+ : - (${t('bracket.walkover')})` : `- : + (${t('bracket.walkover')})`) :
                                                `${match.result?.scoreA} - ${match.result?.scoreB}`
                                        ) : 'vs'}
                                    </div>
                                    
                                    {/* Action Buttons */}
                                    <div style={{ display: 'flex', gap: '5px', marginTop: '5px' }}>
                                        <Link 
                                            to={`/match/${match.id}`}
                                            className="td-btn td-btn-secondary" 
                                            style={{ padding:'2px 8px', fontSize:'0.7rem', textDecoration: 'none', display: 'flex', alignItems: 'center' }}
                                            title={t('common.details')}
                                        >
                                            👁️
                                        </Link>
                                        
                                        {canEdit && (
                                            <>
                                                <button 
                                                    className="td-btn td-btn-secondary" 
                                                    style={{ padding:'2px 8px', fontSize:'0.7rem' }}
                                                    onClick={() => handleEditClick(match)}
                                                >
                                                    ✏️ {t('match.score')}
                                                </button>
                                                <Link 
                                                    to={`/match/${match.id}/manage`}
                                                    className="td-btn td-btn-primary" 
                                                    style={{ padding:'2px 8px', fontSize:'0.7rem', textDecoration: 'none', display: 'flex', alignItems: 'center' }}
                                                >
                                                    ⚙️ {t('match.manage')}
                                                </Link>
                                            </>
                                        )}
                                    </div>
                                </>
                            )}
                        </div>

                        <div className="player-away" style={{ flex: 1, textAlign: 'left', fontWeight: match.result && match.result.scoreB > match.result.scoreA ? 'bold' : 'normal' }}>
                            {match.playerAwayName || t('common.playerB')}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}
