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

const BracketView = ({ tournamentId, isOrganizer, isModerator }: BracketViewProps) => {
    const { t } = useTranslation('mainPage');
    const [matches, setMatches] = useState<Match[]>([]);
    const [loading, setLoading] = useState(true);
    const [editingMatchId, setEditingMatchId] = useState<string | null>(null);

    useEffect(() => {
        loadMatches();
    }, [tournamentId]);

    const loadMatches = async () => {
        setLoading(true);
        const data = await getMatches(tournamentId);
        if (data) setMatches(data);
        setLoading(false);
    };

    const handleEditClick = (match: Match) => {
        setEditingMatchId(match.id);
    };

    const saveResult = async (matchId: string, val: string) => {
        let sA = 0; 
        let sB = 0; 
        let fType = 'Normal';

        if (val === '1:0') { sA = 1; sB = 0; }
        else if (val === '0:1') { sA = 0; sB = 1; }
        else if (val === '+:-') { sA = 1; sB = 0; fType = 'Walkover'; }
        else if (val === '-:+') { sA = 0; sB = 1; fType = 'Walkover'; }
        else return;

        const success = await submitMatchResult(matchId, sA, sB, fType);
        if (success) {
            toast.success(t('bracket.resultSaved'));
            setEditingMatchId(null);
            loadMatches();
        } else {
            toast.error(t('bracket.saveError'));
        }
    };

    if (loading) return <div>{t('loading')}</div>;
    if (matches.length === 0) return <div>{t('bracket.noMatches')}</div>;

    const canEdit = isOrganizer || isModerator;

    const renderMatchCard = (match: Match) => (
         <div key={match.id} style={{
             background: '#2c3e50',
             padding: '10px',
             borderRadius: '8px',
             border: '1px solid #444',
             position: 'relative',
             boxShadow: '0 4px 6px rgba(0,0,0,0.3)',
             minWidth: '200px'
         }}>
             {editingMatchId === match.id ? (
                 <div style={{display:'flex', flexDirection:'column', gap:'5px'}}>
                     <select onChange={(e) => saveResult(match.id, e.target.value)} style={{background:'#333', color:'#fff', padding:'5px', borderRadius:'4px', border:'1px solid #555'}}>
                         <option value="" disabled selected>Select Result</option>
                         <option value="1:0">1 - 0</option>
                         <option value="0:1">0 - 1</option>
                         <option value="+:-">Walkover (Home Win)</option>
                         <option value="-:+">Walkover (Away Win)</option>
                     </select>
                     <button onClick={() => setEditingMatchId(null)} style={{background:'#e74c3c', border:'none', color:'#fff', cursor:'pointer', padding:'4px', borderRadius:'4px'}}>Cancel</button>
                 </div>
             ) : (
                 <>
                     <div style={{display:'flex', justifyContent:'space-between', marginBottom:'5px', fontWeight: match.result?.scoreA === 1 ? 'bold' : 'normal', color: match.result?.scoreA === 1 ? '#2ecc71' : '#fff'}}>
                         <span style={{maxWidth:'140px', overflow:'hidden', textOverflow:'ellipsis', whiteSpace:'nowrap'}} title={match.playerHomeName}>{match.playerHomeName || 'Bye'}</span>
                         <span>{match.result?.scoreA}</span>
                     </div>
                     <div style={{display:'flex', justifyContent:'space-between', fontWeight: match.result?.scoreB === 1 ? 'bold' : 'normal', color: match.result?.scoreB === 1 ? '#2ecc71' : '#fff'}}>
                         <span style={{maxWidth:'140px', overflow:'hidden', textOverflow:'ellipsis', whiteSpace:'nowrap'}} title={match.playerAwayName}>{match.playerAwayName || 'Bye'}</span>
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
                                 fontSize:'0.75rem',
                                 padding:'4px',
                                 borderRadius:'4px'
                             }}
                         >
                             Edit Result
                         </button>
                     )}
                 </>
             )}
         </div>
    );

    // Filter matches into Winners (<1000) and Losers (>=1000) brackets
    const allWb = matches.filter(m => (m.tableNumber || 0) < 1000);
    const allLb = matches.filter(m => (m.tableNumber || 0) >= 1000);
    
    const renderBracketRow = (matchList: Match[], title: string, color: string) => {
        if (matchList.length === 0) return null;
        
        // Use Set to ensure unique rounds and sort them
        const rounds = Array.from(new Set(matchList.map(m => m.roundNumber))).sort((a,b) => a - b);
        
        return (
            <div style={{marginBottom: '40px'}}>
                <h3 style={{
                    color: '#fff', 
                    marginLeft: '10px', 
                    borderLeft: `5px solid ${color}`, 
                    paddingLeft: '15px',
                    marginBottom: '15px',
                    textTransform: 'uppercase',
                    letterSpacing: '1px'
                }}>
                    {title}
                </h3>
                <div className="custom-scrollbar" style={{
                    display: 'flex', 
                    gap: '40px', 
                    overflowX: 'auto', 
                    padding: '20px', 
                    background: '#1e272e', 
                    borderRadius: '12px',
                    border: '1px solid #333'
                }}>
                    {rounds.map(round => {
                        const roundMatches = matchList
                            .filter(m => m.roundNumber === round)
                            .sort((a,b) => (a.tableNumber || 0) - (b.tableNumber || 0));
                        
                        return (
                            <div key={round} style={{ 
                                display: 'flex', 
                                flexDirection: 'column', 
                                minWidth: '220px',
                                flexShrink: 0,
                                gap: '20px'
                            }}>
                                 <h4 style={{
                                     textAlign:'center', 
                                     marginBottom:'10px', 
                                     color:'#aaa', 
                                     background:'rgba(0,0,0,0.2)', 
                                     padding:'5px', 
                                     borderRadius:'4px'
                                 }}>
                                    {t('bracket.round', { round })}
                                 </h4>
                                 {roundMatches.map(match => renderMatchCard(match))}
                            </div>
                        );
                    })}
                </div>
            </div>
        );
    };

    return (
        <div className="bracket-container" style={{padding: '20px'}}>
            {renderBracketRow(allWb, t('bracket.winners'), '#2ecc71')}
            {renderBracketRow(allLb, t('bracket.losers'), '#e74c3c')}
        </div>
    );
};

export default BracketView;
