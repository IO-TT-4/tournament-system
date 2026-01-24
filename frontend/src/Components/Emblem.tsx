import type { MouseEventHandler } from 'react';
import '../assets/styles/emblem.css';

const supportedEmblems = ['chess-pawn', 'diamond', 'shield', 'default'];

function Emblem({
  title,
  date,
  emblem,
  game,
  location,
  details,
  active,
  callBack,
}: {
  title: string;
  date: string;
  emblem: string;
  game: string;
  location: string;
  details?: string;
  active?: boolean;
  callBack?: MouseEventHandler;
}) {  
  if (!supportedEmblems.includes(emblem)) {
    console.warn(`Unsupported emblem: ${emblem}. Using 'default' instead.`);
    emblem = 'default';
  }

  return (
    <div
      className={`emblem ${emblem} ${active ? 'active' : 'inactive'}`}
      onClick={callBack}>
      <div className="mask-layer"></div>
      <div className="text-container">
        <div className="game-name">{game}</div>
        <div className="location-name">{location}</div>
        <div className="title">{title}</div>
        <div className="date">{date}</div>
        <div className="info">{details}</div>
      </div>
    </div>
  );
}

export default Emblem;
