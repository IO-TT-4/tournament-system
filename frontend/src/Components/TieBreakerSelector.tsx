

interface TieBreakerSelectorProps {
  selected: string[];
  onChange: (selected: string[]) => void;
  systemType: string; // SWISS only for now?
}

const AVAILABLE_TIE_BREAKERS = [
  { code: 'BUCHHOLZ', label: 'Buchholz' },
  { code: 'SONNEBORN_BERGER', label: 'Sonneborn-Berger' },
  { code: 'DIRECT_MATCH', label: 'Direct Match' },
  { code: 'WINS', label: 'Number of Wins' },
  { code: 'PROGRESSIVE', label: 'Progressive Score' }
];

export default function TieBreakerSelector({ selected, onChange, systemType }: TieBreakerSelectorProps) {
  // If not Swiss, maybe hide? Or allow for others if implemented.
  if (systemType !== 'SWISS') return null;

  const handleToggle = (code: string) => {
    if (selected.includes(code)) {
      onChange(selected.filter(s => s !== code));
    } else {
      onChange([...selected, code]);
    }
  };

  const moveUp = (index: number) => {
    if (index === 0) return;
    const newSelected = [...selected];
    [newSelected[index - 1], newSelected[index]] = [newSelected[index], newSelected[index - 1]];
    onChange(newSelected);
  };

  const moveDown = (index: number) => {
    if (index === selected.length - 1) return;
    const newSelected = [...selected];
    [newSelected[index + 1], newSelected[index]] = [newSelected[index], newSelected[index + 1]];
    onChange(newSelected);
  };

  return (
    <div className="tie-breaker-selector" style={{ marginTop: '1rem' }}>
      <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 500 }}>
        Tie Breakers (Order Priority)
      </label>
      
      {/* Selected List with Reordering */}
      <div className="selected-list" style={{ display: 'flex', flexDirection: 'column', gap: '5px', marginBottom: '1rem' }}>
        {selected.length === 0 && <span style={{ color: '#888', fontStyle: 'italic' }}>No tie breakers selected (Random fallback)</span>}
        
        {selected.map((code, index) => {
          const tb = AVAILABLE_TIE_BREAKERS.find(t => t.code === code);
          return (
            <div key={code} style={{ 
              display: 'flex', 
              alignItems: 'center', 
              background: 'rgba(255,255,255,0.1)', 
              padding: '8px', 
              borderRadius: '4px',
              justifyContent: 'space-between' 
            }}>
              <span>{index + 1}. {tb?.label || code}</span>
              <div style={{ display: 'flex', gap: '5px' }}>
                <button type="button" onClick={() => moveUp(index)} disabled={index === 0} style={{ padding: '2px 6px', fontSize: '0.8rem' }}>↑</button>
                <button type="button" onClick={() => moveDown(index)} disabled={index === selected.length - 1} style={{ padding: '2px 6px', fontSize: '0.8rem' }}>↓</button>
                <button type="button" onClick={() => handleToggle(code)} style={{ padding: '2px 6px', fontSize: '0.8rem', background: '#e74c3c' }}>×</button>
              </div>
            </div>
          );
        })}
      </div>

      {/* Available Toggle List */}
      <div className="available-list" style={{ display: 'flex', flexWrap: 'wrap', gap: '10px' }}>
          {AVAILABLE_TIE_BREAKERS.map(tb => {
             const isSelected = selected.includes(tb.code);
             if (isSelected) return null; // Hide if already selected
             return (
               <button 
                key={tb.code} 
                type="button" 
                onClick={() => handleToggle(tb.code)}
                style={{ 
                    padding: '5px 10px', 
                    borderRadius: '15px', 
                    border: '1px solid rgba(255,255,255,0.2)', 
                    background: 'transparent',
                    color: '#ccc',
                    cursor: 'pointer'
                }}
               >
                 + {tb.label}
               </button>
             );
          })}
      </div>
    </div>
  );
}
