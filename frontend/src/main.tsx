import React from 'react';
import ReactDOM from 'react-dom/client';
import { GlobalStyles } from './theme/GlobalStyles';
import { ThemeProvider } from './context/ThemeContext';
import App from './App';

// Importar fuentes de Google Fonts para consistencia
const link = document.createElement('link');
link.href = 'https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap';
link.rel = 'stylesheet';
document.head.appendChild(link);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ThemeProvider>
      <GlobalStyles />
      <App />
    </ThemeProvider>
  </React.StrictMode>
);
