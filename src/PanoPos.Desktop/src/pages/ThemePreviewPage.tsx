import { useState } from 'react';
import { Badge } from '../components/Badge';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Panel } from '../components/Panel';
import { Spinner } from '../components/Spinner';
import './ThemePreviewPage.css';

export function ThemePreviewPage() {
  const [value, setValue] = useState('');
  const [feedback, setFeedback] = useState('Bir butona basarak etkilesimi deneyebilirsiniz.');
  return (
    <main className="preview">
      <div className="preview-content">
        <header className="preview-heading">
          <div><h1>PanoPOS</h1><p>Yeni Desktop altyapisi hazir</p></div>
          <Badge>React + Tauri</Badge>
        </header>
        <Panel aria-labelledby="preview-title">
          <div className="section-heading"><h2 id="preview-title">Tema ve bilesen onizlemesi</h2><Badge variant="success">Gorev 1</Badge></div>
          <p className="description">Gecici gelistirme ekrani. Ticari islem veya API baglantisi yoktur.</p>
          <div className="button-preview">
            <Button onClick={() => setFeedback('Primary buton denendi.')}>Primary</Button>
            <Button variant="secondary" onClick={() => setFeedback('Secondary buton denendi.')}>Secondary</Button>
            <Button variant="success" onClick={() => setFeedback('Success buton denendi.')}>Success</Button>
            <Button variant="warning" onClick={() => setFeedback('Warning buton denendi.')}>Warning</Button>
            <Button variant="danger" onClick={() => setFeedback('Danger buton denendi.')}>Danger</Button>
            <Button disabled>Pasif</Button>
          </div>
          <div className="input-preview">
            <Input label="Ornek metin alani" placeholder="Denemek icin yazin" value={value} onChange={(event) => setValue(event.target.value)} />
            <Button variant="secondary" disabled={!value} onClick={() => setValue('')}>Temizle</Button>
          </div>
          <div className="preview-status"><p role="status">{feedback}</p><Spinner label="Yukleme gorunumu" /></div>
        </Panel>
        <footer>Desktop temeli <span>Acik tema / Yerel sistem fontu</span></footer>
      </div>
    </main>
  );
}
