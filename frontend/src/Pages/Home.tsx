import { useEffect, useState, useRef, useLayoutEffect } from 'react';
import { useTranslation } from 'react-i18next';
import '../assets/styles/home.css';
import Emblem from '../Components/Emblem';
import { getTournaments } from '../services/AuthService';

import cs2Bg from '../assets/backgrounds/cs2-bg.jpg';
import chessBg from '../assets/backgrounds/chess-bg.jpg';
import lolBg from '../assets/backgrounds/lol-bg.jpg';
import homm3Bg from '../assets/backgrounds/homm3-bg.jpg';
import wotBg from '../assets/backgrounds/wot-bg.jpg';
import defaultBg from '../assets/backgrounds/default-bg.jpg';

const GAME_BACKGROUNDS: Record<string, string> = {
  cs2: cs2Bg,
  chess: chessBg,
  lol: lolBg,
  homm3: homm3Bg,
  wot: wotBg,
  default: defaultBg,
};

function Home() {
  const { t } = useTranslation('mainPage');
  const [tournaments, setTournaments] = useState<any[]>([]);
  const [activeIndex, setActiveIndex] = useState(0);
  const [offset, setOffset] = useState(0);

  const containerRef = useRef<HTMLDivElement>(null);

  /* ====== TŁO ====== */
  const [prevBg, setPrevBg] = useState<string | null>(null);
  const [isFading, setIsFading] = useState(false);
  const lastBgRef = useRef<string | null>(null);

  /* ====== FETCH ====== */
  useEffect(() => {
    const fetchTournaments = async () => {
      const response = await getTournaments({ limit: 10 });
      const data = response?.data || [];
      const tripled = [...data, ...data, ...data];

      setTournaments(tripled);
      setActiveIndex(data.length);

      const firstBg = data[0]
        ? GAME_BACKGROUNDS[data[0].game.code.toLowerCase()] || GAME_BACKGROUNDS.default
        : GAME_BACKGROUNDS.default;

      lastBgRef.current = firstBg;
    };

    fetchTournaments();
  }, []);

  /* ====== AKTUALNE TŁO (POCHODNE, NIE STAN) ====== */
  const activeTournament = tournaments[activeIndex];

  const currentBg = activeTournament
    ? GAME_BACKGROUNDS[activeTournament.game.code.toLowerCase()] || GAME_BACKGROUNDS.default
    : null;

  /* ====== FADE LOGIC (BEZ WARNINGU) ====== */
  useEffect(() => {
    if (!currentBg) return;

    if (lastBgRef.current && lastBgRef.current !== currentBg) {
      setPrevBg(lastBgRef.current);
      setIsFading(true);

      const t = setTimeout(() => setIsFading(false), 800);
      lastBgRef.current = currentBg;

      return () => clearTimeout(t);
    }

    lastBgRef.current = currentBg;
  }, [currentBg]);

  /* ====== CENTROWANIE KARUZELI ====== */
  useLayoutEffect(() => {
    const calculateOffset = () => {
      if (!containerRef.current) return;

      const container = containerRef.current;
      const activeChild = container.children[activeIndex] as HTMLElement;

      if (!activeChild) return;

      const cardRect = activeChild.getBoundingClientRect();
      const viewportCenter = window.innerWidth / 2;
      const cardCenter = cardRect.left + cardRect.width / 2;

      setOffset((prev) => prev + (viewportCenter - cardCenter));
    };

    const t = setTimeout(calculateOffset, 0);
    window.addEventListener('resize', calculateOffset);

    return () => {
      clearTimeout(t);
      window.removeEventListener('resize', calculateOffset);
    };
  }, [activeIndex]);

  /* ====== NAWIGACJA ====== */
  const handlePrev = () => {
    setActiveIndex((prev) => {
      const next = prev - 1;
      return next < tournaments.length / 3
        ? next + tournaments.length / 3
        : next;
    });
  };

  const handleNext = () => {
    setActiveIndex((prev) => {
      const next = prev + 1;
      return next >= (tournaments.length / 3) * 2
        ? next - tournaments.length / 3
        : next;
    });
  };

  const handleClick = (index: number) => {
    setActiveIndex(() => {
      if (index < tournaments.length / 3) return index + tournaments.length / 3;
      if (index >= (tournaments.length / 3) * 2)
        return index - tournaments.length / 3;
      return index;
    });
  };

  const handleScrollDown = () => {
    window.scrollTo({
      top: window.innerHeight,
      behavior: 'smooth',
    });
  };

  return (
    <main>
      <div className="carousel-wrapper">
        {/* ===== TŁO ===== */}
        <div className="background-container">
          {prevBg && <img src={prevBg} className="bg-layer prev" alt="" />}

          {currentBg && (
            <img
              src={currentBg}
              className={`bg-layer current ${isFading ? 'fade-in' : ''}`}
              alt=""
            />
          )}

          <div className="vignette" />
        </div>

        <h1>{t('currentTournaments')}</h1>

        <button className="carousel-btn prev" onClick={handlePrev}>
          &#10094;
        </button>

        <div
          ref={containerRef}
          id="tournaments-container"
          style={{ transform: `translateX(${offset}px)` }}>
          {tournaments.map((item, i) => (
            <div key={i} className="emblem-anchor">
              <Emblem
                {...item}
                title={item.title}
                game={item.game.name}
                location={item.location}
                active={i === activeIndex}
                callBack={() => handleClick(i)}
              />
            </div>
          ))}
        </div>

        <button className="carousel-btn next" onClick={handleNext}>
          &#10095;
        </button>
        <div className="scroll-down-wrapper">
          <button className="scroll-down-btn" onClick={handleScrollDown}>
            <span className="scroll-icon">
              <svg
                width="18"
                height="18"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round">
                <path d="M6 9l6 6 6-6" />
              </svg>
            </span>
          </button>
        </div>
      </div>

      <div className="container">
        <section className="hero-section">
          <h1>{t('title')}</h1>
          <p className="hero-description">{t('description')}</p>
        </section>
      </div>
    </main>
  );
}

export default Home;
