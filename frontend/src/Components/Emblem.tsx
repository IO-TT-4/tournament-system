import type { MouseEventHandler } from 'react';
import { useTranslation } from 'react-i18next';
import '../assets/styles/emblem.css';

const supportedEmblems = ['chess-pawn', 'diamond', 'shield', 'default'];

function Emblem({
  title,
  date,
  emblem,
  game,
  location,
  systemType,
  numberOfRounds,
  playerLimit,
  active,
  callBack,
}: {
  title: string;
  date: string;
  emblem: string;
  game: string;
  location: string;
  systemType?: string;
  numberOfRounds?: number;
  playerLimit?: number;
  active?: boolean;
  callBack?: MouseEventHandler;
}) {  
  const { t } = useTranslation('mainPage');

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
        <div className="info">
           <div className="info-main">{systemType ? t(`systemTypes.${systemType}`) : ''}</div>
           <div className="info-sub">
             {numberOfRounds && <span>{numberOfRounds} {t('rnds') || 'Rnds'}</span>}
             {numberOfRounds && playerLimit && <span className="separator">•</span>}
             {playerLimit && <span>{t('max') || 'Max'}: {playerLimit}</span>}
           </div>
        </div>
      </div>
    </div>
  );
}

export default Emblem;
