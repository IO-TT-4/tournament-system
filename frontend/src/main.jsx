import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './i18n.jsx';
import { RouterProvider } from 'react-router';
import { router } from './router.jsx';

import './styles/main.css';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
);
