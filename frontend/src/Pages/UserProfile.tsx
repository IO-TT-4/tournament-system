import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getUser, getUserTournaments } from '../services/AuthService';
import type { Tournament } from '../services/AuthService';
import '../assets/styles/userProfile.css';

interface UserProfileData {
    id: string;
    username: string;
    email: string;
}

const UserProfile = () => {
    const { userId } = useParams<{ userId: string }>();
    const { t } = useTranslation('mainPage');
    
    const [user, setUser] = useState<UserProfileData | null>(null);
    const [tournaments, setTournaments] = useState<Tournament[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (userId) {
            loadData(userId);
        }
    }, [userId]);

    const loadData = async (uid: string) => {
        setLoading(true);
        const [uData, tData] = await Promise.all([
            getUser(uid),
            getUserTournaments(uid)
        ]);

        if (uData) setUser(uData);
        if (tData) setTournaments(tData);
        setLoading(false);
    };

    if (loading) return <div className="loading-container">{t('loading') || 'Loading...'}</div>;
    if (!user) return <div className="error-container">{t('userNotFound') || 'User not found'}</div>;

    const activeTournaments = tournaments.filter(t => t.status === 'active');
    const upcomingTournaments = tournaments.filter(t => t.status === 'upcoming');
    const completedTournaments = tournaments.filter(t => t.status === 'completed');

    const formatDate = (dateStr: string) => {
        if (!dateStr || dateStr === 'Invalid Date') return 'TBA';
        return dateStr;
    };

    const renderTournamentList = (list: Tournament[], title: string, icon: string) => {
        if (list.length === 0) return null;
        return (
            <div className="profile-section">
                <h3 className="profile-section-title">
                    <span>{icon}</span> {title}
                </h3>
                <div className="profile-tournaments-grid">
                    {list.map(tournament => (
                        <Link to={`/tournament/${tournament.id}`} key={tournament.id} className="profile-tournament-card">
                            <div className="pt-card-header">
                                <span className="pt-game-badge">{tournament.game.name}</span>
                                <span className={`pt-status-badge pt-status-${tournament.status}`}>
                                    {tournament.status.toUpperCase()}
                                </span>
                            </div>
                            <div className="pt-card-body">
                                <h3 className="pt-card-title">{tournament.title}</h3>
                                <div className="pt-card-details">
                                    <div className="pt-detail-item">
                                        <span className="pt-icon">📅</span>
                                        <span>{formatDate(tournament.date)}</span>
                                    </div>
                                    <div className="pt-detail-item">
                                        <span className="pt-icon">📍</span>
                                        <span>{tournament.location || 'Online'}</span>
                                    </div>
                                    <div className="pt-detail-item">
                                        <span className="pt-icon">👥</span>
                                        <span>{tournament.playerLimit ? `${tournament.playerLimit} ${t('slots')}` : t('open')}</span>
                                    </div>
                                    <div className="pt-detail-item">
                                        <span className="pt-icon">🏆</span>
                                        <span>{tournament.systemType}</span>
                                    </div>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            </div>
        );
    };

    return (
        <div className="user-profile-page">
            <div className="profile-header-card">
                <div className="profile-avatar-wrapper">
                    {user.username.charAt(0).toUpperCase()}
                </div>
                <h1 className="profile-username">{user.username}</h1>
                <div className="profile-stats-meta">
                    <div className="stat-item">
                        <span className="stat-value">{tournaments.length}</span>
                        <span className="stat-label">{t('tournamentsJoined') || 'Tournaments'}</span>
                    </div>
                </div>
            </div>

            <div className="profile-content">
                {tournaments.length === 0 ? (
                    <div className="no-data-msg">
                        {t('noTournamentsFound') || 'No tournaments found for this user.'}
                    </div>
                ) : (
                    <>
                        {renderTournamentList(activeTournaments, t('activeTournaments') || 'Active Tournaments', '🔴')}
                        {renderTournamentList(upcomingTournaments, t('upcomingTournaments') || 'Upcoming Tournaments', '📅')}
                        {renderTournamentList(completedTournaments, t('completedTournaments') || 'History', '🏁')}
                    </>
                )}
            </div>
        </div>
    );
};

export default UserProfile;
