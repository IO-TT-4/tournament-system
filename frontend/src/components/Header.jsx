import DarkModeToggle from './DarkModeToggle';

import '../styles/header.css';
import logo from '../assets/logo/gflow_light_transparent_64px.png';
import { Link } from 'react-router';
import NavBar from './NavBar';

export default function Header() {
  return (
    <header>
      <div className="logo">
        <Link to="/">
          <img src={logo} alt="logo" />
          <span>G-Flow</span>
        </Link>
      </div>
      <NavBar />
    </header>
  );
}
