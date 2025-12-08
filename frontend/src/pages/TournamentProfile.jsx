import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { getTournament } from '../api/api';
import { handleError } from '../helpers/ErrorHandler';

export default function TournamentProfile() {
  const { id } = useParams();
  const [tournament, setTournament] = useState(null);

  const navigate = useNavigate();

  useEffect(() => {
    getTournament(id)
      .then((res) => {
        if (res) {
          const tournamentObj = {
            title: res?.data?.title,
            id: res?.data?.id,
            metaData: res?.data?.metaData,
          };
          setTournament(tournamentObj);
        }
      })
      .catch((e) => {
        handleError(e);
        navigate('/');
      });
  }, []);

  return (
    <>
      <h1>Tournament Profile</h1>
      <h2>
        {tournament?.id} - {tournament?.title}
      </h2>
      <p>
        Date: {tournament?.metaData?.startDate} -{' '}
        {tournament?.metaData?.endDate}
      </p>
      <p>Rounds: {tournament?.metaData?.numberOfRounds}</p>
      <p>Players: {tournament?.metaData?.numberOfRegisteredPlayers}</p>
    </>
  );
}
