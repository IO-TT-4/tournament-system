import { createBrowserRouter } from 'react-router';
import App from './App';
import Home from './Pages/Home';
import Login from './Pages/Login';
import Register from './Pages/Register';
import Tournaments from './Pages/Tournaments';
import TournamentDetails from './Pages/TournamentDetails';
import CreateTournament from './Pages/CreateTournament';
import TournamentEdit from './Pages/TournamentEdit';
import TournamentParticipant from './Pages/TournamentParticipant';
import MatchDetails from './Pages/MatchDetails';
import MatchManage from './Pages/MatchManage';
import UserProfile from './Pages/UserProfile';
import NotFound from './Pages/NotFound';
import ProtectedRoute from './Components/ProtectedRoute';

export const router = createBrowserRouter([
  {
    element: <App />,
    children: [
      { path: '/', element: <Home /> },
      { path: '/login', element: <Login /> },
      { path: '/register', element: <Register /> },
      { path: '/tournaments', element: <Tournaments /> },
      { path: '/create-tournament', element: <ProtectedRoute><CreateTournament /></ProtectedRoute> },
      { path: '/tournament/:id', element: <TournamentDetails /> },
      { path: '/tournament/:id/participant/:userId', element: <TournamentParticipant /> },
      { path: '/tournament/edit/:id', element: <ProtectedRoute><TournamentEdit /></ProtectedRoute> },
      { path: '/match/:matchId', element: <MatchDetails /> },
      { path: '/match/:matchId/manage', element: <ProtectedRoute><MatchManage /></ProtectedRoute> },
      { path: '/user/:userId', element: <UserProfile /> },
      { path: '*', element: <NotFound /> },
    ],
  },
]);
