import { createBrowserRouter } from 'react-router';
import App from './App';
import Home from './Pages/Home';
import Login from './Pages/Login';
import Register from './Pages/Register';
import Tournaments from './Pages/Tournaments';
import TournamentDetails from './Pages/TournamentDetails';
import CreateTournament from './Pages/CreateTournament';
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
      { path: '*', element: <NotFound /> },
    ],
  },
]);
