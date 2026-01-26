import { useEffect, useState } from 'react';
import { getMatches, submitMatchResult } from '../services/AuthService';
import type { Match } from '../services/AuthService';
import { useTranslation } from 'react-i18next';
import { toast } from 'react-toastify';

interface BracketViewProps {
    tournamentId: string;
    isOrganizer: boolean;
    isModerator: boolean;
}

export default function BracketView({ tournamentId, isOrganizer, isModerator }: BracketViewProps) {
    const { t } = useTranslation('mainPage');
    const [matches, setMatches] = useState<Match[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Edit state
    const [editingMatchId, setEditingMatchId] = useState<string | null>(null);
    const [scoreA, setScoreA] = useState<number>(0);
    const [scoreB, setScoreB] = useState<number>(0);
    const [finishType, setFinishType] = useState<string>('Normal');

    useEffect(() => {
        loadMatches();
    }, [tournamentId]);

    const loadMatches = async () => {
        setLoading(true);
        const data = await getMatches(tournamentId);
        // Sort matches by round then position
        setMatches(data.sort((a,b) => {
            if (a.roundNumber !== b.roundNumber) return a.roundNumber - b.roundNumber;
            return (a.tableNumber || 0) - (b.tableNumber || 0); // fallback to tableNumber/position
        }));
        setLoading(false);
    };

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
        } else if (val === "+:-") {
            setScoreA(1); setScoreB(0); setFinishType('Walkover');
        } else if (val === "-:+") {
            setScoreA(0); setScoreB(1); setFinishType('Walkover');
        }
    };

    const handleSaveResult = async (matchId: string) => {
        const success = await submitMatchResult(matchId, scoreA, scoreB, finishType);
        if (success) {
            toast.success("Result saved!");
            setEditingMatchId(null);
            loadMatches();
        } else {
            toast.error("Failed to save result.");
        }
    };

    const canEdit = isOrganizer || isModerator;
    
    // Group by Round
    const roundNumbers = Array.from(new Set(matches.map(m => m.roundNumber))).sort((a,b) => a - b);
    
    const renderMatchCard = (match: Match) => (
         <div key={match.id} style={{
             background: '#2c3e50',
             padding: '10px',
             borderRadius: '8px',
             border: '1px solid #444',
             position: 'relative',
             boxShadow: '0 4px 6px rgba(0,0,0,0.3)'
         }}>
             {editingMatchId === match.id ? (
                 <div style={{display:'flex', flexDirection:'column', gap:'5px'}}>
                     <select onChange={handleResultChange} style={{background:'#333', color:'#fff', padding:'5px'}}>
                         <option value="" disabled selected>Select Result</option>
                         <option value="1:0">1 - 0</option>
                         <option value="0:1">0 - 1</option>
                         <option value="+:-">Walkover A</option>
                         <option value="-:+">Walkover B</option>
                     </select>
                     <div style={{display:'flex', gap:'5px'}}>
                        <button onClick={() => handleSaveResult(match.id)} style={{flex:1, background:'#27ae60', border:'none', color:'#fff', cursor:'pointer'}}>Save</button>
                        <button onClick={() => setEditingMatchId(null)} style={{flex:1, background:'#e74c3c', border:'none', color:'#fff', cursor:'pointer'}}>Cancel</button>
                     </div>
                 </div>
             ) : (
                 <>
                     <div style={{display:'flex', justifyContent:'space-between', marginBottom:'5px', fontWeight: match.result?.scoreA === 1 ? 'bold' : 'normal', color: match.result?.scoreA === 1 ? '#2ecc71' : '#fff'}}>
                         <span>{match.playerHomeName || 'Bye'}</span>
                         <span>{match.result?.scoreA}</span>
                     </div>
                     <div style={{display:'flex', justifyContent:'space-between', fontWeight: match.result?.scoreB === 1 ? 'bold' : 'normal', color: match.result?.scoreB === 1 ? '#2ecc71' : '#fff'}}>
                         <span>{match.playerAwayName || 'Bye'}</span>
                         <span>{match.result?.scoreB}</span>
                     </div>
                     
                     {canEdit && !match.isCompleted && (
                         <button 
                             onClick={() => handleEditClick(match)}
                             style={{
                                 width:'100%', 
                                 marginTop:'8px', 
                                 background:'rgba(255,255,255,0.1)', 
                                 border:'none', 
                                 color:'#ccc', 
                                 cursor:'pointer',
                                 fontSize:'0.8rem'
                             }}
                         >
                             Edit Result
                         </button>
                     )}
                 </>
             )}
         </div>
    );

    if (loading) return <div>{t('loading')}</div>;
    if (matches.length === 0) return <div>{t('noMatches') || "No matches yet."}</div>;

    return (
        <div className="bracket-container" style={{ overflowX: 'auto', padding: '20px' }}>
            <div style={{ display: 'flex', minHeight: '600px', gap: '50px' }}>
                {roundNumbers.map(round => {
                    // Filter matches for this round and sort by Position/Table
                    const roundMatches = matches
                        .filter(m => m.roundNumber === round)
                        .sort((a,b) => (a.tableNumber || 0) - (b.tableNumber || 0));

                    const wbMatches = roundMatches.filter(m => (m.tableNumber || 0) < 1000);
                    const lbMatches = roundMatches.filter(m => (m.tableNumber || 0) >= 1000);

                    return (
                        <div key={round} style={{ 
                            display: 'flex', 
                            flexDirection: 'column', 
                            minWidth: '220px',
                            flexShrink: 0
                        }}>
                             <h4 style={{textAlign:'center', marginBottom:'10px', color:'#aaa'}}>Round {round}</h4>
                             
                             {/* Winners Bracket Section */}
                             <div style={{
                                 flex: 1,
                                 display: 'flex',
                                 flexDirection: 'column',
                                 justifyContent: 'space-around',
                                 gap: '20px',
                                 marginBottom: lbMatches.length > 0 ? '40px' : '0'
                             }}>
                                 {wbMatches.map(match => (
                                     renderMatchCard(match)
                                 ))}
                             </div>

                             {/* Losers Bracket Section */}
                             {lbMatches.length > 0 && (
                                 <div style={{
                                     flex: 1,
                                     display: 'flex',
                                     flexDirection: 'column',
                                     justifyContent: 'space-around',
                                     gap: '20px',
                                     borderTop: '2px dashed #444',
                                     paddingTop: '20px'
                                 }}>
                                     <div style={{textAlign:'center', fontSize:'0.8rem', color:'#666', marginBottom:'10px'}}>Losers Bracket</div>
                                     {lbMatches.map(match => (
                                         renderMatchCard(match)
                                     ))}
                                 </div>
                             )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
