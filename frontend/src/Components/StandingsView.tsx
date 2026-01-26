import { useEffect, useState } from 'react';
import { getStandings, getTournamentById } from '../services/AuthService';
import type { StandingsEntry } from '../services/AuthService';
import { useTranslation } from 'react-i18next';

const TB_LABELS: Record<string, string> = {
    'BUCHHOLZ': 'Buchholz',
    'SONNEBORN_BERGER': 'Sonneborn-Berger',
    'PROGRESSIVE': 'Progressive',
    'WINS': 'Wins',
    'DIRECT_MATCH': 'Direct Match'
};

export default function StandingsView({ tournamentId }: { tournamentId: string }) {
    const { t } = useTranslation('mainPage');
    const [standings, setStandings] = useState<StandingsEntry[]>([]);
    const [tieBreakers, setTieBreakers] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadData = async () => {
            setLoading(true);
            const [stdData, tournData] = await Promise.all([
                getStandings(tournamentId),
                getTournamentById(tournamentId)
            ]);
            
            setStandings(stdData);
            if (tournData && tournData.tieBreakers && tournData.tieBreakers.length > 0) {
                setTieBreakers(tournData.tieBreakers);
            } else {
                setTieBreakers(['BUCHHOLZ']); // Default
            }
            setLoading(false);
        };
        loadData();
    }, [tournamentId]);

    if (loading) return <div>{t('loading')}</div>;

    if (standings.length === 0) return <div className="no-data">{t('noStandings') || "No standings available yet."}</div>;

    return (
        <div className="standings-table-container">
            <table className="standings-table" style={{ width: '100%', borderCollapse: 'collapse', marginTop: '1rem' }}>
                <thead>
                    <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.1)', textAlign: 'left' }}>
                        <th style={{ padding: '10px' }}>#</th>
                        <th style={{ padding: '10px' }}>{t('player') || 'Player'}</th>
                        <th style={{ padding: '10px' }}>{t('score') || 'Score'}</th>
                        
                        {tieBreakers.map(tb => (
                            <th key={tb} style={{ padding: '10px' }}>{TB_LABELS[tb] || tb}</th>
                        ))}

                        <th style={{ padding: '10px' }}>{t('matches') || 'Matches'}</th>
                        <th style={{ padding: '10px' }}>W-D-L</th>
                    </tr>
                </thead>
                <tbody>
                    {standings.map((entry, index) => (
                        <tr key={entry.userId} style={{ borderBottom: '1px solid rgba(255,255,255,0.05)', background: index % 2 === 0 ? 'rgba(255,255,255,0.02)' : 'transparent' }}>
                            <td style={{ padding: '10px' }}>{index + 1}</td>
                            <td style={{ padding: '10px', display: 'flex', alignItems: 'center', gap: '10px' }}>
                                <div style={{ width: '24px', height: '24px', borderRadius: '50%', background: '#3498db', display: 'flex', justifyContent: 'center', alignItems: 'center', fontSize: '0.8rem' }}>
                                    {entry.username.charAt(0).toUpperCase()}
                                </div>
                                {entry.username} {entry.isWithdrawn && <span style={{color:'red'}}>(Withdrawn)</span>}
                            </td>
                            <td style={{ padding: '10px', fontWeight: 'bold', color: '#f1c40f' }}>{entry.score}</td>
                            
                            {tieBreakers.map(tb => (
                                <td key={tb} style={{ padding: '10px', color: '#aaa' }}>
                                    {entry.tieBreakerValues ? (entry.tieBreakerValues[tb] ?? 0) : (tb === 'BUCHHOLZ' ? entry.buchholz : '-')}
                                </td>
                            ))}

                            <td style={{ padding: '10px' }}>{entry.matchesPlayed}</td>
                            <td style={{ padding: '10px' }}>{entry.wins}-{entry.draws}-{entry.losses}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
