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
    tieBreakers: [] as string[]
  });

  const [moderators, setModerators] = useState<{ id: string, username: string }[]>([]);
  const [currentModQuery, setCurrentModQuery] = useState('');
  const [searchResults, setSearchResults] = useState<{ id: string, username: string }[]>([]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
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
          toast.error(t('selectUserFromList') || 'Please select a valid user from the search');
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
      tieBreakers: formData.systemType === 'SWISS' ? formData.tieBreakers : []
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
      toast.error('Failed to create tournament');
    }
  };

  // Preview Data
  const previewData = {
    id: 'preview',
    title: formData.name || t('tournamentName'),
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
        <label className="preview-label">Preview</label>
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
        <h2>{t('createTournament') || 'Create Tournament'}</h2>
        <form onSubmit={handleSubmit}>
          
          <div className="form-group">
            <label>{t('tournamentName') || 'Tournament Name'}</label>
            <input required name="name" value={formData.name} onChange={handleChange} minLength={5} />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>{t('game') || 'Game'}</label>
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
              <label>{t('systemType') || 'System'}</label>
              <select name="systemType" value={formData.systemType} onChange={handleChange}>
                <option value="SINGLE_ELIMINATION">{t('systemTypes.SINGLE_ELIMINATION')}</option>
                <option value="DOUBLE_ELIMINATION">{t('systemTypes.DOUBLE_ELIMINATION')}</option>
                <option value="SINGLE_ROUND_ROBIN">{t('systemTypes.SINGLE_ROUND_ROBIN')}</option>
                <option value="DOUBLE_ROUND_ROBIN">{t('systemTypes.DOUBLE_ROUND_ROBIN')}</option>
                <option value="SWISS">{t('systemTypes.SWISS')}</option>
              </select>
            </div>
          </div>



// ...

          <div className="form-row">
            <div className="form-group">
              <label>{t('maxPlayers') || 'Max Players'}</label>
              <input type="number" name="maxPlayers" value={formData.maxPlayers} onChange={handleChange} min={2} />
            </div>

            {formData.systemType === 'SWISS' && (
              <div className="form-group">
                <label>{t('rounds') || 'Rounds'}</label>
                <input type="number" name="numberOfRounds" value={formData.numberOfRounds} onChange={handleChange} min={1} />
              </div>
            )}
          </div>
          
           {/* Tie Breakers */}
           <TieBreakerSelector 
              systemType={formData.systemType} 
              selected={formData.tieBreakers} 
              onChange={(newVal) => setFormData(prev => ({ ...prev, tieBreakers: newVal }))} 
           />

          <div className="form-group">
             <label>{t('emblem') || 'Emblem'}</label>
             <select name="emblem" value={formData.emblem} onChange={handleChange}>
               <option value="default">Default</option>
               <option value="chess-pawn">Chess Pawn</option>
               <option value="shield">Shield</option>
               <option value="diamond">Diamond</option>
             </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>{t('startDate')}</label>
              <input type="datetime-local" name="startDate" value={formData.startDate} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label>{t('endDate') || 'End Date'}</label>
              <input type="datetime-local" name="endDate" value={formData.endDate} onChange={handleChange} required />
            </div>
          </div>

          <div className="form-group">
            <label className="checkbox-label">
              <input type="checkbox" checked={formData.isOnline} onChange={handleCheckbox} />
              {t('onlineTournament') || 'Online Tournament'}
            </label>
          </div>

          {!formData.isOnline && (
            <div className="form-row">
              <div className="form-group">
                <label>{t('city') || 'City'}</label>
                <input name="city" value={formData.city} onChange={handleChange} required={!formData.isOnline} />
              </div>
              <div className="form-group">
                <label>{t('address') || 'Address'}</label>
                <input name="address" value={formData.address} onChange={handleChange} />
              </div>
            </div>
          )}
          
          <div className="form-group">
            <label>{t('description') || 'Description'}</label>
            <HtmlEditor value={formData.description} onChange={handleDescriptionChange} />
          </div>

          <div className="form-group">
            <label>{t('moderators') || 'Moderators (User IDs)'}</label>
            <div className="moderator-input-group" style={{ position: 'relative' }}>
              <input 
                list="moderator-datalist"
                value={currentModQuery} 
                onChange={(e) => handleSearch(e.target.value)} 
                placeholder={t('searchUser') || "Search User..."} 
              />
              <datalist id="moderator-datalist">
                  {searchResults.map(user => (
                      <option key={user.id} value={user.username} />
                  ))}
              </datalist>
              <button onClick={handleAddClick} type="button">{t('add') || 'Add'}</button>
            </div>
            <div className="moderators-list">
              {moderators.map(mod => (
                <span key={mod.id} className="moderator-tag">
                  {mod.username} <button type="button" onClick={() => removeMod(mod.id)} className="remove-mod-btn">x</button>
                </span>
              ))}
            </div>
          </div>

          <button type="submit" className="submit-btn">{t('createBtn') || 'Create Tournament'}</button>
        </form>
      </div>
    </div>
  );
}

export default CreateTournament;
