import React, { useState, useEffect } from 'react';
import '../assets/styles/modal.css';
import { searchUsers } from '../services/AuthService';

interface UserSearchModalProps {
    isOpen: boolean;
    onClose: () => void;
    onAdd: (username: string) => void;
    title?: string;
}

const UserSearchModal: React.FC<UserSearchModalProps> = ({ isOpen, onClose, onAdd, title = "Add Participant" }) => {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState<{ id: string, username: string }[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (isOpen) {
            setQuery('');
            setResults([]);
        }
    }, [isOpen]);

    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (query.trim().length > 2) {
                setLoading(true);
                const data = await searchUsers(query);
                setResults(data);
                setLoading(false);
            } else {
                setResults([]);
            }
        }, 300);

        return () => clearTimeout(delayDebounceFn);
    }, [query]);

    if (!isOpen) return null;

    return (
        <div className="modal-overlay">
            <div className="modal-content" style={{maxWidth: '500px'}}>
                <h3>{title}</h3>
                <p style={{marginBottom: '1rem'}}>Search for a user by username:</p>
                
                <input 
                    type="text" 
                    className="modal-input"
                    placeholder="Type username..."
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    autoFocus
                />

                {loading && <div style={{marginBottom: '1rem', color: '#aaa'}}>Searching...</div>}

                <div className="search-results-list">
                    {results.map(user => (
                        <div 
                            key={user.id} 
                            className="search-result-item"
                            onClick={() => {
                                onAdd(user.username);
                                onClose();
                            }}
                        >
                            <div className="result-avatar">{user.username.charAt(0).toUpperCase()}</div>
                            <span>{user.username}</span>
                            <button className="modal-btn modal-btn-primary" style={{marginLeft: 'auto', padding: '4px 10px', fontSize: '0.8rem'}}>
                                Add
                            </button>
                        </div>
                    ))}
                    {query.length > 2 && !loading && results.length === 0 && (
                        <div style={{color: '#aaa', fontStyle: 'italic', marginBottom: '1rem'}}>No users found.</div>
                    )}
                </div>

                <div className="modal-actions" style={{marginTop: '1rem'}}>
                    <button className="modal-btn modal-btn-secondary" onClick={onClose}>
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
};

export default UserSearchModal;
