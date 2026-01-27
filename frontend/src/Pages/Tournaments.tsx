import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { getTournaments } from '../services/AuthService';
import type { TournamentParams, Tournament } from '../services/AuthService';
import TournamentCard from '../Components/TournamentCard';
import '../assets/styles/tournaments.css';


function Tournaments() {
  const { t } = useTranslation('mainPage');
  const [tournaments, setTournaments] = useState<Tournament[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);

  // Filter States
  const [searchTerm, setSearchTerm] = useState('');
  const [gameFilter, setGameFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [cityQuery, setCityQuery] = useState('');
  const [radius, setRadius] = useState(50);
  const [sortBy, setSortBy] = useState('date-asc');
  const [page, setPage] = useState(1);
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [debouncedCityQuery, setDebouncedCityQuery] = useState('');

  // Debounce logic
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchTerm), 300);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedCityQuery(cityQuery), 300);
    return () => clearTimeout(timer);
  }, [cityQuery]);


  const fetchResults = useCallback(async (isLoadMore = false) => {
    setLoading(true);
    const params: TournamentParams = {
      searchTerm: debouncedSearch,
      discipline: gameFilter,

      status: statusFilter,
      location: {
        city: debouncedCityQuery,
        radius: debouncedCityQuery ? radius : undefined,
      },
      sortBy,
      page: isLoadMore ? page + 1 : 1,
      limit: 20,
    };

    const response = await getTournaments(params);
    if (response) {
      if (isLoadMore) {
        setTournaments((prev) => [...prev, ...response.data] as Tournament[]);
        setPage((p) => p + 1);
      } else {
        setTournaments(response.data as Tournament[]);
        setPage(1);
      }
      setTotalCount(response.total);
    }
    setLoading(false);
  }, [debouncedSearch, gameFilter, statusFilter, debouncedCityQuery, radius, sortBy, page]);

  // Initial fetch and fetch on filter change
  useEffect(() => {
    // We reset page to 1 on any filter change
    fetchResults(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch, gameFilter, statusFilter, debouncedCityQuery, radius, sortBy]);


  const clearFilters = () => {
    setSearchTerm('');
    setGameFilter('all');
    setStatusFilter('all');
    setCityQuery('');
    setRadius(50);
    setSortBy('date-asc');
  };

  const isFiltered = searchTerm !== '' || gameFilter !== 'all' || statusFilter !== 'all' || cityQuery !== '';

  // Disciplines list for the dropdown - in a real app this would be another API call
  const disciplines = ['chess', 'cs2', 'lol', 'volleyball', 'football', 'poker'];

  return (
    <div className="tournaments-page">
      <div className="tournaments-background" />

      <div className="tournaments-container">
        <div className="tournaments-header">
          <h1>{t('allTournaments')}</h1>
          <p>{t('browseTournaments')}</p>
        </div>

        <div className="tournaments-controls">
          <div className="controls-row primary-controls">
            <div className="search-box">
              <svg className="search-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
              </svg>
              <input
                type="text"
                placeholder={t('searchTournaments')}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            <div className="location-group">
              <div className="city-input">
                <input
                  type="text"
                  placeholder={t('city')}
                  value={cityQuery}
                  onChange={(e) => setCityQuery(e.target.value)}
                />
              </div>
              <div className="radius-selector">
                <select value={radius} onChange={(e) => setRadius(Number(e.target.value))} disabled={!cityQuery}>
                  <option value={10}>10 {t('km')}</option>
                  <option value={25}>25 {t('km')}</option>
                  <option value={50}>50 {t('km')}</option>
                  <option value={100}>100 {t('km')}</option>
                  <option value={250}>250 {t('km')}</option>
                </select>
              </div>
            </div>
          </div>

          <div className="controls-row secondary-controls">
            <div className="filters-group">
              <div className="filter-item">
                <label>{t('filterByGame')}:</label>
                <select value={gameFilter} onChange={(e) => setGameFilter(e.target.value)} className="filter-select">
                  <option value="all">{t('all')}</option>
                  {disciplines.map((d) => (
                    <option key={d} value={d}>{t(`games.${d}`)}</option>
                  ))}
                </select>
              </div>

              <div className="filter-item">
                <label>{t('filter.status')}:</label>
                <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="filter-select">
                  <option value="all">{t('all')}</option>
                  <option value="active">{t('active')}</option>
                  <option value="upcoming">{t('comingSoon')}</option>
                  <option value="completed">{t('completed')}</option>
                </select>
              </div>

              <div className="filter-item">
                <label>{t('sortBy')}:</label>
                <select value={sortBy} onChange={(e) => setSortBy(e.target.value)} className="filter-select">
                  <option value="date-asc">{t('dateAsc')}</option>
                  <option value="date-desc">{t('dateDesc')}</option>
                </select>
              </div>
            </div>
          </div>

          <div className="controls-footer">
            <div className="results-count">
              {t('showingResults', { count: tournaments.length, total: totalCount })}
            </div>
            {isFiltered && (
              <button className="clear-btn" onClick={clearFilters}>
                {t('clearFilters')}
              </button>
            )}
          </div>
        </div>

        {tournaments.length > 0 ? (
          <>
            <div className="tournaments-grid">
              {tournaments.map((tournament, index) => (
                <TournamentCard key={`${tournament.id}-${index}`} {...tournament} title={tournament.title} />
              ))}
            </div>
            {tournaments.length < totalCount && (
              <div className="load-more-container">
                <button className="load-more-btn" onClick={() => fetchResults(true)} disabled={loading}>
                  {loading ? '...' : t('loadMore')}
                </button>
              </div>
            )}
          </>
        ) : (
          <div className="tournaments-empty">
            <h2>{t('noTournamentsFound')}</h2>
            <p>{t('tryChangingCriteria')}</p>
            {isFiltered && (
              <button className="clear-btn-large" onClick={clearFilters}>
                {t('clearFilters')}
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default Tournaments;
