import { Outlet } from 'react-router';
import { UserProvider } from './context/useAuth';
import Footer from './components/Fotter';
import { ToastContainer } from 'react-toastify';
import Header from './components/Header';

import 'react-toastify/ReactToastify.css';

function App() {
  return (
    <>
      <UserProvider>
        <Header />
        <Outlet />
        <ToastContainer
          position="bottom-right"
          autoClose={3000}
          pauseOnHover={false}
        />
        <Footer />
      </UserProvider>
    </>
  );
}

export default App;
