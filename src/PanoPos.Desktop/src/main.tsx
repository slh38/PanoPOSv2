import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ThemePreviewPage } from './pages/ThemePreviewPage';
import './styles/tokens.css';
import './styles/global.css';

const root = document.getElementById('root');
if (!root) throw new Error('Application root is missing.');

createRoot(root).render(
  <StrictMode>
    <ThemePreviewPage />
  </StrictMode>,
);
