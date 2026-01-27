import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router';
import { createTournament, addModerator, searchUsers } from '../services/AuthService';
import '../assets/styles/createTournament.css';
import { toast } from 'react-toastify';
import Emblem from '../Components/Emblem'; // Import Emblem for preview
import HtmlEditor from '../Components/HtmlEditor'; // Import HtmlEditor
import TieBreakerSelector from '../Components/TieBreakerSelector';

function CreateTournament() {
  const { t } = useTranslation('mainPage');
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    name: '',
    gameCode: 'CHESS', // Default, but user can type
    gameName: 'Chess',
    systemType: 'SINGLE_ELIMINATION',
    maxPlayers: 16,
    startDate: '',
    endDate: '',
    isOnline: false,
    city: '',
    address: '',
    emblem: 'default',
    description: '',
    numberOfRounds: 5,
    tieBreakers: [] as string[],
    winPoints: undefined as number | undefined,
    drawPoints: undefined as number | undefined,
    lossPoints: undefined as number | undefined,
    registrationMode: 'Open',
    enableMatchEvents: false
  });

  const [moderators, setModerators] = useState<{ id: string, username: string }[]>([]);
  const [currentModQuery, setCurrentModQuery] = useState('');
  const [searchResults, setSearchResults] = useState<{ id: string, username: string }[]>([]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const calcScoringPreset = () => {
    const { winPoints, drawPoints, lossPoints } = formData;
    if (winPoints === undefined && drawPoints === undefined && lossPoints === undefined) return 'raw';
    if (winPoints === 1 && drawPoints === 0.5 && lossPoints === 0) return 'chess';
    if (winPoints === 3 && drawPoints === 1 && lossPoints === 0) return 'football';
    return 'custom';
  };

  const applyScoringPreset = (val: string) => {
    if (val === 'raw') {
        setFormData(prev => ({...prev, winPoints: undefined, drawPoints: undefined, lossPoints: undefined}));
    } else if (val === 'chess') {
        setFormData(prev => ({...prev, winPoints: 1, drawPoints: 0.5, lossPoints: 0}));
    } else if (val === 'football') {
        setFormData(prev => ({...prev, winPoints: 3, drawPoints: 1, lossPoints: 0}));
    } else {
         // Custom, defaults to current or 0
         setFormData(prev => ({
             ...prev, 
             winPoints: prev.winPoints ?? 0, 
             drawPoints: prev.drawPoints ?? 0, 
             lossPoints: prev.lossPoints ?? 0
         }));
    }
  };

  const handleGameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    // Basic logic to map common input types to codes, but allowing custom
    const val = e.target.value;
    // We update name directly. Code is derived or same.
    setFormData(prev => ({ 
      ...prev, 
      gameCode: val.toUpperCase().replace(/\s/g, ''), // Simple code generation 
      gameName: val 
    }));
  };

  const handleDescriptionChange = (val: string) => {
    setFormData(prev => ({ ...prev, description: val }));
  };

  const handleCheckbox = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({ ...prev, isOnline: e.target.checked }));
  };

  const handleSearch = async (val: string) => {
    setCurrentModQuery(val);
    if (val.length > 2) {
       const results = await searchUsers(val);
       setSearchResults(results);
    } else {
       setSearchResults([]);
    }
  };

  const handleAddClick = () => {
      // Find user in search results that matches current input
      const user = searchResults.find(u => u.username === currentModQuery);
      if (user) {
          addMod(user);
      } else {
          // Optional: Allow non-search match if backend supports resolving by exact username later?
          // For now, let's enforce selection from search or exact match in results
          toast.error(t('error.selectUser'));
      }
  };

  const addMod = (user: { id: string, username: string }) => {
    if (!moderators.some(m => m.id === user.id)) {
      setModerators([...moderators, user]);
    }
    setCurrentModQuery('');
    setSearchResults([]);
  };

  const removeMod = (id: string) => {
    setModerators(moderators.filter(m => m.id !== id));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Prepare Request
    const request = {
      name: formData.name,
      gameCode: formData.gameCode,
      gameName: formData.gameName,
      systemType: formData.systemType,
      maxParticipants: Number(formData.maxPlayers),
      startDate: new Date(formData.startDate),
      endDate: new Date(formData.endDate),
      city: formData.isOnline ? 'Online' : formData.city,
      address: formData.isOnline ? undefined : formData.address,
      emblem: formData.emblem,
      organizerId: 'UNKNOWN',
      description: formData.description,
      numberOfRounds: formData.systemType === 'SWISS' ? Number(formData.numberOfRounds) : undefined,
      tieBreakers: formData.systemType === 'SWISS' ? formData.tieBreakers : [],
      winPoints: formData.winPoints,
      drawPoints: formData.drawPoints,
      lossPoints: formData.lossPoints,
      registrationMode: formData.registrationMode,
      enableMatchEvents: formData.enableMatchEvents
    };

    try {
      const response = await createTournament(request);
      if (response && response.data) {
        const tournamentId = response.data.id;
        
        if (moderators.length > 0) {
           for (const mod of moderators) {
             await addModerator(tournamentId, mod.id);
           }
        }

        toast.success(t('tournamentCreated'));
        navigate(`/tournament/${tournamentId}`);
      }
    } catch (error) {
      console.error(error);
      toast.error(t('error.createTournament'));
    }
  };

  // Preview Data
  const previewData = {
    id: 'preview',
    title: formData.name || t('tournament.create.nameLabel'),
    date: formData.startDate ? new Date(formData.startDate).toLocaleDateString() : 'Date',
    emblem: formData.emblem,
    game: { name: formData.gameName, code: formData.gameCode },
    location: formData.isOnline ? 'Online' : (formData.city || 'City'),
    systemType: formData.systemType,
    numberOfRounds: formData.systemType === 'SWISS' ? formData.numberOfRounds : undefined,
    playerLimit: formData.maxPlayers,
    status: 'upcoming' as const,
    details: '', // Deprecated
    organizer: 'Me',
    lat: 0,
    lng: 0
  };

  return (
    <div className="create-tournament-page">
      {/* PREVIEW SECTION */}
      <div className="preview-section">
        <label className="preview-label">{t('common.preview')}</label>
        <div style={{ transform: 'scale(0.9)' }}>
          <Emblem 
            title={previewData.title}
            date={previewData.date}
            emblem={previewData.emblem}
            game={previewData.game.name}
            location={previewData.location}
            systemType={previewData.systemType}
            numberOfRounds={previewData.numberOfRounds}
            playerLimit={previewData.playerLimit}
            active={true} 
            callBack={() => {}} 
          />
        </div>
      </div>

      {/* FORM SECTION */}
      <div className="create-tournament-container">
        <h2>{t('tournament.create.title')}</h2>
        <form onSubmit={handleSubmit}>
          
          <div className="form-group">
            <label>{t('tournament.create.nameLabel')}</label>
            <input required name="name" value={formData.name} onChange={handleChange} minLength={5} />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>{t('tournament.create.gameLabel')}</label>
              <input 
                list="games-list" 
                name="gameName" 
                value={formData.gameName} 
                onChange={handleGameChange} 
                required 
              />
              <datalist id="games-list">
                <option value="Chess">CHESS</option>
                <option value="Counter-Strike 2">CS2</option>
                <option value="League of Legends">LOL</option>
                <option value="Dota 2">DOTA2</option>
                <option value="Teamfight Tactics">TFT</option>
                <option value="Valorant">VALORANT</option>
                <option value="FIFA">FC24</option>
                <option value="StarCraft 2">SC2</option>
                <option value="Rocket League">RL</option>
                <option value="Hearthstone">HS</option>
              </datalist>
            </div>
            <div className="form-group">
              <label>{t('tournament.create.systemLabel')}</label>
              <select name="systemType" value={formData.systemType} onChange={handleChange}>
                <option value="SINGLE_ELIMINATION">{t('systemTypes.SINGLE_ELIMINATION')}</option>
                <option value="DOUBLE_ELIMINATION">{t('systemTypes.DOUBLE_ELIMINATION')}</option>
                <option value="SINGLE_ROUND_ROBIN">{t('systemTypes.SINGLE_ROUND_ROBIN')}</option>
                <option value="DOUBLE_ROUND_ROBIN">{t('systemTypes.DOUBLE_ROUND_ROBIN')}</option>
                <option value="SWISS">{t('systemTypes.SWISS')}</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>{t('tournament.create.maxPlayers')}</label>
              <input type="number" name="maxPlayers" value={formData.maxPlayers} onChange={handleChange} min={2} />
            </div>

            {formData.systemType === 'SWISS' && (
              <div className="form-group">
                <label>{t('tournament.create.rounds')}</label>
                <input type="number" name="numberOfRounds" value={formData.numberOfRounds} onChange={handleChange} min={1} />
              </div>
            )}
            <div className="form-group">
                <label>{t('tournament.create.registrationMode')}</label>
                <select name="registrationMode" value={formData.registrationMode} onChange={handleChange}>
                    <option value="Open">{t('tournament.registration.open')}</option>
                    <option value="ApprovalRequired">{t('tournament.registration.approval')}</option>
                </select>
            </div>
            <div className="form-group">
                <label className="checkbox-label">
                    <input 
                        type="checkbox" 
                        checked={formData.enableMatchEvents} 
                        onChange={(e) => setFormData(prev => ({...prev, enableMatchEvents: e.target.checked}))} 
                    />
                    {t('tournament.create.enableMatchEvents')}
                </label>
            </div>
          </div>
          
           {/* Tie Breakers */}
           <TieBreakerSelector 
              systemType={formData.systemType} 
              selected={formData.tieBreakers} 
              onChange={(newVal) => setFormData(prev => ({ ...prev, tieBreakers: newVal }))} 
           />

          <div className="form-group">
             <label>{t('tournament.create.emblem')}</label>
             <select name="emblem" value={formData.emblem} onChange={handleChange}>
               <option value="default">{t('emblem.default')}</option>
               <option value="chess-pawn">{t('emblem.chessPawn')}</option>
               <option value="shield">{t('emblem.shield')}</option>
               <option value="diamond">{t('emblem.diamond')}</option>
             </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>{t('tournament.create.startDate')}</label>
              <input type="datetime-local" name="startDate" value={formData.startDate} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label>{t('tournament.create.endDate')}</label>
              <input type="datetime-local" name="endDate" value={formData.endDate} onChange={handleChange} required />
            </div>
          </div>

          <div className="form-group">
            <label className="checkbox-label">
              <input type="checkbox" checked={formData.isOnline} onChange={handleCheckbox} />
              {t('tournament.create.isOnline')}
            </label>
          </div>

          {!formData.isOnline && (
            <div className="form-row">
              <div className="form-group">
                <label>{t('tournament.create.city')}</label>
                <input name="city" value={formData.city} onChange={handleChange} required={!formData.isOnline} />
              </div>
              <div className="form-group">
                <label>{t('tournament.create.address')}</label>
                <input name="address" value={formData.address} onChange={handleChange} />
              </div>
            </div>
          )}
          
          <div className="form-group">
            <label>{t('tournament.create.description')}</label>
            <HtmlEditor value={formData.description} onChange={handleDescriptionChange} />
          </div>

          <div className="form-group">
            <label>{t('tournament.create.moderators')}</label>
            <div className="moderator-input-group" style={{ position: 'relative' }}>
              <input 
                list="moderator-datalist"
                value={currentModQuery} 
                onChange={(e) => handleSearch(e.target.value)} 
                placeholder={t('common.searchUserPlaceholder')} 
              />
              <datalist id="moderator-datalist">
                  {searchResults.map(user => (
                      <option key={user.id} value={user.username} />
                  ))}
              </datalist>
              <button onClick={handleAddClick} type="button">{t('common.add')}</button>
            </div>
            <div className="moderators-list">
              {moderators.map(mod => (
                <span key={mod.id} className="moderator-tag">
                  {mod.username} <button type="button" onClick={() => removeMod(mod.id)} className="remove-mod-btn">x</button>
                </span>
              ))}
            </div>
          </div>

        {/* Scoring Rules Section */}
        <div className="form-group management-section">
            <label>{t('tournament.scoring.title')}</label>
            <div className="form-row">
                <div className="form-group">
                    <label>{t('tournament.scoring.preset')}</label>
                    <select 
                        value={calcScoringPreset()} 
                        onChange={(e) => applyScoringPreset(e.target.value)}
                    >
                        <option value="raw">{t('tournament.scoring.raw')}</option>
                        <option value="chess">Chess (1 / 0.5 / 0)</option>
                        <option value="football">League/Football (3 / 1 / 0)</option>
                        <option value="custom">{t('tournament.scoring.custom')}</option>
                    </select>
                </div>
            </div>
            
            {calcScoringPreset() !== 'raw' && (
                <div className="form-row">
                    <div className="form-group">
                        <label>{t('tournament.scoring.winPoints')}</label>
                        <input 
                            type="number" 
                            step="0.5" 
                            value={formData.winPoints ?? 0} 
                            onChange={(e) => setFormData(prev => ({...prev, winPoints: parseFloat(e.target.value)}))}
                        />
                    </div>
                    <div className="form-group">
                        <label>{t('tournament.scoring.drawPoints')}</label>
                        <input 
                            type="number" 
                            step="0.5" 
                            value={formData.drawPoints ?? 0} 
                            onChange={(e) => setFormData(prev => ({...prev, drawPoints: parseFloat(e.target.value)}))}
                        />
                    </div>
                    <div className="form-group">
                        <label>{t('tournament.scoring.lossPoints')}</label>
                        <input 
                            type="number" 
                            step="0.5" 
                            value={formData.lossPoints ?? 0} 
                            onChange={(e) => setFormData(prev => ({...prev, lossPoints: parseFloat(e.target.value)}))}
                        />
                    </div>
                </div>
            )}
        </div>

          <button type="submit" className="submit-btn">{t('tournament.create.submitBtn')}</button>
        </form>
      </div>
    </div>
  );
}

export default CreateTournament;
