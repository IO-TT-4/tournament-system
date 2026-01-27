import React from 'react';
import '../assets/styles/modal.css';

interface ConfirmationModalProps {
    isOpen: boolean;
    title: string;
    message: string;
    onConfirm: (inputValue?: string) => void;
    onClose: () => void;
    confirmText?: string;
    cancelText?: string;
    isDanger?: boolean;
    showInput?: boolean;
    inputPlaceholder?: string;
}

const ConfirmationModal: React.FC<ConfirmationModalProps> = ({ 
    isOpen, title, message, onConfirm, onClose, 
    confirmText = "Confirm", cancelText = "Cancel", isDanger = false,
    showInput = false, inputPlaceholder = ""
}) => {
    const [inputValue, setInputValue] = React.useState('');

    React.useEffect(() => {
        if (isOpen) setInputValue('');
    }, [isOpen]);

    if (!isOpen) return null;

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <h3>{title}</h3>
                <p>{message}</p>
                
                {showInput && (
                    <input 
                        type="text" 
                        className="modal-input"
                        placeholder={inputPlaceholder}
                        value={inputValue}
                        onChange={(e) => setInputValue(e.target.value)}
                        autoFocus
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') {
                                onConfirm(inputValue);
                                onClose();
                            }
                        }}
                    />
                )}

                <div className="modal-actions">
                    <button className="modal-btn modal-btn-secondary" onClick={onClose}>
                        {cancelText}
                    </button>
                    <button 
                        className={`modal-btn ${isDanger ? 'modal-btn-danger' : 'modal-btn-primary'}`} 
                        onClick={() => {
                            onConfirm(showInput ? inputValue : undefined);
                            onClose();
                        }}
                        disabled={showInput && !inputValue.trim()}
                    >
                        {confirmText}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ConfirmationModal;
