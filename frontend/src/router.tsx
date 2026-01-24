import { createBrowserRouter } from 'react-router';
import App from './App';
import Home from './Pages/Home';
import Login from './Pages/Login';
import Register from './Pages/Register';
import Tournaments from './Pages/Tournaments';
import TournamentDetails from './Pages/TournamentDetails';
import NotFound from './Pages/NotFound';

export const router = createBrowserRouter([
  {
    element: <App />,
    children: [
      { path: '/', element: <Home /> },
      { path: '/login', element: <Login /> },
      { path: '/register', element: <Register /> },
      { path: '/tournaments', element: <Tournaments /> },
      { path: '/tournament/:id', element: <TournamentDetails /> },
      { path: '*', element: <NotFound /> },
    ],
  },
]);
