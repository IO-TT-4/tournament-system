import React, { useRef } from 'react';
import '../assets/styles/createTournament.css'; // Reusing styles or specific

interface HtmlEditorProps {
  value: string;
  onChange: (value: string) => void;
}

const HtmlEditor: React.FC<HtmlEditorProps> = ({ value, onChange }) => {
  const editorRef = useRef<HTMLTextAreaElement>(null);

  const insertTag = (tag: string) => {
    if (!editorRef.current) return;
    
    const textarea = editorRef.current;
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const text = textarea.value;
    
    const before = text.substring(0, start);
    const selection = text.substring(start, end);
    const after = text.substring(end);
    
    const newText = `${before}<${tag}>${selection}</${tag}>${after}`;
    onChange(newText);
    
    // Focus back and attempt to keep selection? simple set needed
  };

  return (
    <div className="html-editor">
      <div className="editor-toolbar">
         <button type="button" onClick={() => insertTag('b')}><b>B</b></button>
         <button type="button" onClick={() => insertTag('i')}><i>I</i></button>
         <button type="button" onClick={() => insertTag('u')}><u>U</u></button>
         <button type="button" onClick={() => insertTag('h3')}>H3</button>
         <button type="button" onClick={() => insertTag('p')}>P</button>
      </div>
      <textarea 
        ref={editorRef}
        value={value} 
        onChange={(e) => onChange(e.target.value)} 
        className="editor-textarea"
        rows={6}
      />
    </div>
  );
};

export default HtmlEditor;
