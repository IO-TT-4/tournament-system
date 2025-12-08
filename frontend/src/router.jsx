import { createBrowserRouter, Navigate } from 'react-router';

import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import App from './App';
import UserProfile from './pages/UserProfile';
import TournamentProfile from './pages/TournamentProfile';

export const router = createBrowserRouter([
  {
    element: <App />,
    children: [
      { path: '*', element: <Navigate to="/" /> },
      { path: '/', element: <Home /> },
      { path: '/login', element: <Login /> },
      { path: '/register', element: <Register /> },
      {
        path: '/user/:id',
        children: [
          {
            path: '',
            element: <UserProfile />,
          },
          { path: 'edit', element: <h1>Hello edit user</h1> },
        ],
      },
      {
        path: '/tournament/:id',
        children: [
          {
            path: '',
            element: <TournamentProfile />,
          },
          {
            path: 'edit',
            element: <h1>Hello edit tournament</h1>,
          },
        ],
      },
    ],
  },
]);
