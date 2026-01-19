import { useTranslation } from 'react-i18next';
import '../styles/mainPage.css';

import TournamentCard from '../components/TournamentCard';
import { Link } from 'react-router';
import { useEffect, useState } from 'react';

import { getUpcomingTournaments, getCurrentTournaments } from '../api/api';
import { handleError } from '../helpers/ErrorHandler';

export default function MainPage() {
  const { t } = useTranslation('mainPage');
  const [currentTournamentList, setCurrentTournamentList] = useState([]);
  const [upcomingTournamentList, setUpcomingTournamentList] = useState([]);

  useEffect(() => {
    getCurrentTournaments()
      .then((res) => {
        if (res) {
          setCurrentTournamentList(res?.data || []);
        }
      })
      .catch((e) => {
        handleError(e);
      });

    getUpcomingTournaments()
      .then((res) => {
        if (res) {
          setUpcomingTournamentList(res?.data || []);
        }
      })
      .catch((e) => {
        handleError(e);
      });
  }, []);

  const currentTournaments = currentTournamentList.map(createTournamentCard);
  const upcomingTournaments = upcomingTournamentList.map(createTournamentCard);

  function createTournamentCard({ id, name, metaData }) {
    return (
      <Link to={`/tournament/${id}`} key={id}>
        <TournamentCard title={name} metaData={metaData} />
      </Link>
    );
  }

  return (
    <main>
      <section id="info-container" className="container">
        <h1>{t('title')}</h1>
        <p>{t('description')}</p>
      </section>
      <section id="main-page-content">
        <article id="current-tournaments" className="container">
          <h2>{t('currentTournaments')}</h2>
          <div className="tournament-cards">{currentTournaments}</div>
        </article>
        <article id="future-tournaments" className="container">
          <h2>{t('futureTournaments')}</h2>
          <div className="tournament-cards">{upcomingTournaments}</div>
        </article>
      </section>
    </main>
  );
}
