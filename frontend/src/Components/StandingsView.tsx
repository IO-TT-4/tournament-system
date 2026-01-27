import { useEffect, useState } from 'react';
import { getStandings, getTournamentById } from '../services/AuthService';
import type { StandingsEntry } from '../services/AuthService';
import { useTranslation } from 'react-i18next';

const TB_LABELS: Record<string, string> = {
    'BUCHHOLZ': 'tieBreakers.BUCHHOLZ',
    'SONNEBORN_BERGER': 'tieBreakers.SONNEBORN_BERGER',
    'PROGRESSIVE': 'tieBreakers.PROGRESSIVE',
    'WINS': 'tieBreakers.WINS',
    'DIRECT_MATCH': 'tieBreakers.DIRECT_MATCH'
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

    if (standings.length === 0) return <div className="no-data">{t('standings.none')}</div>;

    return (
        <div className="standings-table-container">
            <table className="standings-table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>{t('common.player')}</th>
                        <th>{t('common.score')}</th>
                        
                        {tieBreakers.map(tb => (
                            <th key={tb}>{t(TB_LABELS[tb]) || tb}</th>
                        ))}

                        <th>{t('common.matches')}</th>
                        <th>W-D-L</th>
                    </tr>
                </thead>
                <tbody>
                    {standings.map((entry, index) => (
                        <tr key={entry.userId} className="standings-row">
                            <td>{index + 1}</td>
                            <td 
                                className="st-player-cell"
                                onClick={() => window.location.href = `/tournament/${tournamentId}/participant/${entry.userId}`}
                                title={t('common.viewProfile')}
                            >
                                <div className="st-avatar">
                                    {entry.username.charAt(0).toUpperCase()}
                                </div>
                                <span className={entry.isWithdrawn ? "st-struck" : ""}>
                                    {entry.username}
                                </span>
                                {entry.isWithdrawn && <span className="st-withdrawn">{t('status.withdrawn_parens')}</span>}
                            </td>
                            <td className="st-score">{entry.score}</td>
                            
                            {tieBreakers.map(tb => (
                                <td key={tb} className="st-tiebreaker">
                                    {entry.tieBreakerValues ? (entry.tieBreakerValues[tb] ?? 0) : (tb === 'BUCHHOLZ' ? entry.buchholz : '-')}
                                </td>
                            ))}

                            <td>{entry.matchesPlayed}</td>
                            <td>{entry.wins}-{entry.draws}-{entry.losses}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
