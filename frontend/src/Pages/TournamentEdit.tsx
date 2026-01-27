import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useTranslation } from 'react-i18next';
import { getTournamentById, updateTournament, addModerator, removeModerator, searchUsers, addParticipant, removeParticipant, withdrawParticipant, getAuditLogs } from '../services/AuthService';
import type { UpdateTournamentRequest, Participant, AuditLog, TournamentAuditLog } from '../services/AuthService';
import { toast } from 'react-toastify';
import '../assets/styles/createTournament.css';
import '../assets/styles/forms.css'; // Ensure forms.css is loaded for buttons
import Emblem from '../Components/Emblem';
import HtmlEditor from '../Components/HtmlEditor';

export default function TournamentEdit() {
    const { t } = useTranslation('mainPage');
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);
    const [formData, setFormData] = useState({
        name: '',
        gameCode: '', 
        gameName: '',
        systemType: '', 
        maxPlayers: 16,
        startDate: '',
        endDate: '',
        isOnline: false,
        city: '',
        address: '',
        emblem: 'default',
        description: '',
        numberOfRounds: 0,
        playerLimit: 0,
        winPoints: undefined as number | undefined,
        drawPoints: undefined as number | undefined,
        lossPoints: undefined as number | undefined,
        enableMatchEvents: false
    });

    const [moderators, setModerators] = useState<{ id: string, username: string }[]>([]);
    const [participants, setParticipants] = useState<Participant[]>([]);
    
    // Search states
    const [currentModQuery, setCurrentModQuery] = useState('');
    const [searchResults, setSearchResults] = useState<{ id: string, username: string }[]>([]);
    
    // Participant manual add state
    const [participantQuery, setParticipantQuery] = useState('');
    const [participantSearchResults, setParticipantSearchResults] = useState<{ id: string, username: string }[]>([]);

    // Audit Logs
    const [matchAuditLogs, setMatchAuditLogs] = useState<AuditLog[]>([]);
    const [tournamentAuditLogs, setTournamentAuditLogs] = useState<TournamentAuditLog[]>([]);
    const [showAuditLogs, setShowAuditLogs] = useState(false);

    const fetchTournament = async () => {
         if (!id) return;
         setLoading(true);
         const data = await getTournamentById(id);
         if (data) {
             const toLocalISO = (dateStr: string) => {
                 const d = new Date(dateStr);
                 if (isNaN(d.getTime())) return '';
                 const offset = d.getTimezoneOffset() * 60000;
                 return (new Date(d.getTime() - offset)).toISOString().slice(0, 16);
             };

             setFormData({
                 name: data.title,
                 gameCode: data.game.code,
                 gameName: data.game.name,
                 systemType: data.systemType,
                 maxPlayers: data.playerLimit,
                 playerLimit: data.playerLimit,
                 startDate: toLocalISO(data.date),
                 endDate: '',
                 isOnline: data.location === 'Online',
                 city: data.location !== 'Online' ? data.location : '',
                 address: '',
                 emblem: data.emblem,
                 description: data.details || '',
                 numberOfRounds: data.numberOfRounds || 0,
                 winPoints: data.winPoints,
                 drawPoints: data.drawPoints,
                 lossPoints: data.lossPoints,
                 enableMatchEvents: data.enableMatchEvents ?? false
             });

             if (data.moderatorIds) {
                 setModerators(data.moderatorIds.map(mid => ({ id: mid, username: `ID: ${mid.substring(0,8)}...` })));
             }
             
             if (data.participants) {
                 setParticipants(data.participants);
             }
         }
         setLoading(false);
    };

    useEffect(() => {
        fetchTournament();
    }, [id]);

    // ... Handlers ...
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleCheckbox = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData(prev => ({ ...prev, isOnline: e.target.checked }));
    };

    const handleDescriptionChange = (val: string) => {
        setFormData(prev => ({ ...prev, description: val }));
    };

    // Moderator Logic
    const handleSearch = async (val: string) => {
        setCurrentModQuery(val);
        if (val.length > 2) {
           const results = await searchUsers(val);
           setSearchResults(results);
        } else {
           setSearchResults([]);
        }
    };

    const handleAddModClick = async () => {
        const user = searchResults.find(u => u.username === currentModQuery);
        if (user && id) {
            const success = await addModerator(id, user.id);
            if (success) {
                 toast.success(`Added ${user.username}`);
                 setModerators(prev => [...prev.filter(m => m.id !== user.id), user]);
                 setCurrentModQuery('');
                 setSearchResults([]);
            } else {
                 toast.error("Failed to add moderator.");
            }
        }
    };

    const handleRemoveMod = async (userId: string) => {
        if (!id) return;
        if (confirm("Remove this moderator?")) {
            const success = await removeModerator(id, userId);
            if (success) {
                toast.success("Moderator removed.");
                setModerators(prev => prev.filter(m => m.id !== userId));
            } else {
                toast.error("Failed to remove moderator.");
            }
        }
    };

    // Participant Logic
    const handleParticipantSearch = async (val: string) => {
        setParticipantQuery(val);
        if (val.length > 2) {
           const results = await searchUsers(val);
           setParticipantSearchResults(results);
        } else {
           setParticipantSearchResults([]);
        }
    };

    const handleAddParticipant = async () => {
        const usernameToAdd = participantQuery;
        if (!id || !usernameToAdd) return;
        
        // This expects Username string to be valid in backend (must be registered user)
        const success = await addParticipant(id, usernameToAdd); 
        if (success) {
            toast.success("Participant added!");
            setParticipantQuery('');
            setParticipantSearchResults([]);
            // Refresh list because we need ID and status etc.
            const data = await getTournamentById(id);
            if (data && data.participants) setParticipants(data.participants);
        } else {
            toast.error("Failed to add. Ensure username exists and not already added.");
        }
    };

    const handleRemoveParticipant = async (userId: string) => {
        if (!id) return;
        if (confirm(t('confirm.removeParticipant'))) {
            const success = await removeParticipant(id, userId);
            if (success) {
                toast.success("Participant removed.");
                setParticipants(prev => prev.filter(p => p.id !== userId));
            } else {
                toast.error("Failed to remove participant.");
            }
        }
    };

    const handleWithdrawParticipant = async (userId: string) => {
        if (!id) return;
        if (confirm(t('confirm.withdrawParticipant'))) {
            const success = await withdrawParticipant(id, userId);
            if (success) {
                toast.success("Participant withdrawn.");
                setParticipants(prev => prev.map(p => p.id === userId ? { ...p, isWithdrawn: true } : p));
            } else {
                toast.error("Failed to withdraw participant.");
            }
        }
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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!formData.name || !formData.startDate) {
            toast.error(t('fillAllFields') || "Please fill all required fields.");
            return;
        }

        const payload: UpdateTournamentRequest = {
            name: formData.name,
            startDate: new Date(formData.startDate),
            endDate: formData.endDate ? new Date(formData.endDate) : new Date(formData.startDate),
            playerLimit: Number(formData.maxPlayers),
            description: formData.description,
            emblem: formData.emblem,
            winPoints: formData.winPoints,
            drawPoints: formData.drawPoints,
            lossPoints: formData.lossPoints,
            enableMatchEvents: formData.enableMatchEvents
        };

        const result = await updateTournament(id!, payload);
        if (result) {
            toast.success(t('tournamentUpdated'));
            navigate(`/tournament/${id}`);
        } else {
            toast.error(t('updateError'));
        }
    };

    if (loading && !formData.name) return <div className="loading-container">{t('loading')}...</div>;

    const previewData = {
        title: formData.name,
        date: formData.startDate ? new Date(formData.startDate).toLocaleDateString() : 'Date',
        emblem: formData.emblem,
        game: { name: formData.gameName, code: formData.gameCode },
        location: formData.isOnline ? 'Online' : (formData.city || 'City'),
        systemType: formData.systemType,
        numberOfRounds: formData.numberOfRounds,
        playerLimit: formData.maxPlayers,
    };

    return (
        <div className="create-tournament-page">
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

            <div className="create-tournament-container">
                <h2>{t('tournament.edit.title')}</h2>
                <form onSubmit={handleSubmit}>
                    
                    <div className="form-group">
                        <label>{t('tournament.create.nameLabel')}</label>
                        <input name="name" value={formData.name} onChange={handleChange} required minLength={5} />
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>{t('tournament.create.maxPlayers')}</label>
                            <input type="number" name="maxPlayers" value={formData.maxPlayers} onChange={handleChange} min={2} />
                        </div>
                         <div className="form-group">
                             <label>{t('tournament.create.emblem')}</label>
                             <select name="emblem" value={formData.emblem} onChange={handleChange}>
                                 <option value="default">{t('emblem.default')}</option>
                                 <option value="checkmate">{t('emblem.chessPawn')}</option>
                                 <option value="shield">{t('emblem.shield')}</option>
                                 <option value="diamond">{t('emblem.diamond')}</option>
                             </select>
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>{t('tournament.create.startDate')}</label>
                            <input type="datetime-local" name="startDate" value={formData.startDate} onChange={handleChange} required />
                        </div>
                        <div className="form-group">
                            <label>{t('tournament.create.endDate')}</label>
                            <input type="datetime-local" name="endDate" value={formData.endDate} onChange={handleChange} />
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
                                <input name="city" value={formData.city} onChange={handleChange} />
                            </div>
                        </div>
                    )}

                    <div className="form-group">
                        <label>{t('tournament.create.description')}</label>
                        <HtmlEditor value={formData.description} onChange={handleDescriptionChange} />
                    </div>

                    {/* Participants Management Section */}
                    <div className="form-group management-section">
                        <label>{t('participants')} ({participants.length})</label>
                        
                        {/* Manual Add */}
                        <div className="moderator-input-group">
                             <input 
                                list="participant-datalist-edit"
                                value={participantQuery} 
                                onChange={(e) => handleParticipantSearch(e.target.value)} 
                                placeholder={t('common.searchUserAdd')} 
                                style={{flex: 1}}
                              />
                              <datalist id="participant-datalist-edit">
                                  {participantSearchResults.map(user => (
                                      <option key={user.id} value={user.username} />
                                  ))}
                              </datalist>
                             <button onClick={handleAddParticipant} type="button" className="btn-add-green">
                                + {t('common.add')}
                             </button>
                        </div>

                        {/* List */}
                        <div className="management-list">
                            {participants.length === 0 && <div className="no-data-msg">{t('tournament.participants.empty')}</div>}
                            {participants.map(p => (
                                <div key={p.id} className="management-item">
                                    <span className={`management-name ${p.isWithdrawn ? 'status-withdrawn' : 'status-active'}`}>
                                        {p.username} {p.isWithdrawn && t('status.withdrawn_parens')}
                                    </span>
                                    <div className="management-actions">
                                        {!p.isWithdrawn && (
                                            <button 
                                                type="button" 
                                                onClick={() => handleWithdrawParticipant(p.id)}
                                                className="btn-action-withdraw"
                                                title="Withdraw (Forfeit)"
                                            >
                                                ⚠ Withdraw
                                            </button>
                                        )}
                                        <button 
                                            type="button" 
                                            onClick={() => handleRemoveParticipant(p.id)}
                                            className="btn-action-remove"
                                            title="Remove (Unregister)"
                                        >
                                            🗑
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Moderator Management Section */}
                    <div className="form-group management-section">
                        <label>{t('tournament.create.moderators')}</label>
                        <div className="moderator-input-group">
                          <input 
                            list="moderator-datalist-edit"
                            value={currentModQuery} 
                            onChange={(e) => handleSearch(e.target.value)} 
                            placeholder={t('common.searchUserPlaceholder')} 
                            style={{flex: 1}}
                          />
                          <datalist id="moderator-datalist-edit">
                              {searchResults.map(user => (
                                  <option key={user.id} value={user.username} />
                              ))}
                          </datalist>
                          <button onClick={handleAddModClick} type="button" className="btn-add-secondary">{t('common.add')}</button>
                        </div>
                        <div className="moderators-list">
                          {moderators.map(mod => (
                            <span key={mod.id} className="moderator-tag">
                              {mod.username} 
                              <button type="button" onClick={() => handleRemoveMod(mod.id)} className="remove-mod-btn">x</button>
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
                        <p style={{fontSize: '0.8rem', color: '#888', marginTop: '5px'}}>
                                {calcScoringPreset() === 'raw' 
                                    ? t('tournament.scoring.explanationRaw') 
                                    : t('tournament.scoring.explanationPoints')}
                        </p>
                    </div>

                    {/* Match Events Toggle */}
                    <div className="form-group management-section">
                        <label className="checkbox-label">
                            <input 
                                type="checkbox" 
                                checked={formData.enableMatchEvents} 
                                onChange={(e) => setFormData(prev => ({...prev, enableMatchEvents: e.target.checked}))} 
                            />
                            {t('tournament.create.enableMatchEvents')}
                        </label>
                        <p style={{fontSize: '0.8rem', color: '#888', marginTop: '5px'}}>
                            Włączenie tej opcji aktywuje Live Match Dashboard z osią czasu i przyciskami akcji. Wyłączenie oznacza prosty edytor wyników.
                        </p>
                    </div>

                    {/* Audit Log Section */}
                    <div className="form-group management-section">
                        <label>{t('audit.title')}</label>
                        <button 
                            type="button" 
                            className="btn-add-secondary" 
                            style={{marginBottom: '1rem'}}
                            onClick={async () => {
                                if (!showAuditLogs && id) {
                                    const response = await getAuditLogs(id);
                                    setMatchAuditLogs(response.matchAudits);
                                    setTournamentAuditLogs(response.tournamentAudits);
                                }
                                setShowAuditLogs(!showAuditLogs);
                            }}
                        >
                            {showAuditLogs ? t('audit.hide') : t('audit.view')}
                        </button>
                        
                        {showAuditLogs && (
                            <div className="management-list" style={{maxHeight: '400px', overflow: 'auto'}}>
                                {matchAuditLogs.length === 0 && tournamentAuditLogs.length === 0 && <div className="no-data-msg">{t('audit.empty')}</div>}
                                
                                {/* Tournament Action Logs */}
                                {tournamentAuditLogs.length > 0 && (
                                    <>
                                        <div style={{fontWeight: 'bold', marginBottom: '0.5rem', borderBottom: '1px solid #ddd', paddingBottom: '0.5rem'}}>{t('audit.tournamentActions')}</div>
                                        {tournamentAuditLogs.map(log => (
                                            <div key={log.id} className="management-item" style={{flexDirection: 'column', alignItems: 'flex-start', gap: '5px'}}>
                                                <div style={{fontSize: '0.85rem', color: '#888'}}>
                                                    {new Date(log.timestamp).toLocaleString()} - <strong>{log.actionType}</strong>
                                                </div>
                                                <div>
                                                    {log.targetUsername && <span>{t('audit.target')} <strong>{log.targetUsername}</strong></span>}
                                                </div>
                                                {log.details && <div style={{fontSize: '0.8rem', color: '#666'}}>{log.details}</div>}
                                                <div style={{fontSize: '0.75rem', color: '#666'}}>
                                                    {t('audit.by')} {log.performedByUsername || log.performedById.substring(0, 8) + '...'}
                                                </div>
                                            </div>
                                        ))}
                                    </>
                                )}
                                
                                {/* Match Result Logs */}
                                {matchAuditLogs.length > 0 && (
                                    <>
                                        <div style={{fontWeight: 'bold', margin: '0.5rem 0', borderBottom: '1px solid #ddd', paddingBottom: '0.5rem'}}>{t('audit.matchChanges')}</div>
                                        {matchAuditLogs.map(log => (
                                            <div key={log.id} className="management-item" style={{flexDirection: 'column', alignItems: 'flex-start', gap: '5px'}}>
                                                <div style={{fontSize: '0.85rem', color: '#888'}}>
                                                    {new Date(log.modifiedAt).toLocaleString()} - <strong>{log.changeType}</strong>
                                                </div>
                                                <div>
                                                    Match: <code>{log.matchId.substring(0, 8)}...</code>
                                                </div>
                                                <div>
                                                    {log.oldScoreA !== null ? (
                                                        <span style={{textDecoration: 'line-through', color: '#999', marginRight: '10px'}}>
                                                            {log.oldScoreA} - {log.oldScoreB}
                                                        </span>
                                                    ) : null}
                                                    <strong style={{color: '#4CAF50'}}>{log.newScoreA} - {log.newScoreB}</strong>
                                                </div>
                                                <div style={{fontSize: '0.75rem', color: '#666'}}>
                                                    {t('audit.by')} {log.modifiedBy.substring(0, 8)}...
                                                </div>
                                            </div>
                                        ))}
                                    </>
                                )}
                            </div>
                        )}
                    </div>

                    <div className="form-actions">
                        <button type="submit" className="submit-btn">{t('common.saveChanges')}</button>
                        <button type="button" className="btn-cancel" onClick={() => navigate(-1)}>{t('common.cancel')}</button>
                    </div>

                </form>
            </div>
        </div>
    );
}
