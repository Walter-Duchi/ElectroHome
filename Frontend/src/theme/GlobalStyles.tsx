import { GlobalStyles as MuiGlobalStyles } from '@mui/material';

export const GlobalStyles = () => (
  <MuiGlobalStyles
    styles={{
      'html, body, #root': {
        height: '100%',
        width: '100%',
      },
      'input[type=number]::-webkit-inner-spin-button, input[type=number]::-webkit-outer-spin-button': {
        WebkitAppearance: 'none',
        margin: 0,
      },
      'input[type=number]': {
        MozAppearance: 'textfield',
      },
      // Estilos para selección de texto consistente
      '::selection': {
        backgroundColor: 'rgba(0, 86, 179, 0.2)',
      },
      '::-moz-selection': {
        backgroundColor: 'rgba(0, 86, 179, 0.2)',
      },
      // Animaciones globales
      '@keyframes fadeIn': {
        from: { opacity: 0 },
        to: { opacity: 1 },
      },
      '@keyframes fadeOut': {
        from: { opacity: 1 },
        to: { opacity: 0 },
      },
      '@keyframes slideInUp': {
        from: { transform: 'translateY(20px)', opacity: 0 },
        to: { transform: 'translateY(0)', opacity: 1 },
      },
      '@keyframes slideInDown': {
        from: { transform: 'translateY(-20px)', opacity: 0 },
        to: { transform: 'translateY(0)', opacity: 1 },
      },
      '@keyframes pulse': {
        '0%': { transform: 'scale(1)' },
        '50%': { transform: 'scale(1.05)' },
        '100%': { transform: 'scale(1)' },
      },
      // Estilos para focus accesible
      '*:focus-visible': {
        outline: '2px solid #0056b3',
        outlineOffset: '2px',
        borderRadius: '4px',
      },
      // Estilos para transiciones suaves
      '*': {
        transition: 'background-color 0.3s ease, border-color 0.3s ease, color 0.3s ease',
      },
      // Excepciones para elementos que no deben tener transición
      'canvas, img, svg, video, iframe': {
        transition: 'none !important',
      },
      // Estilos para impresión
      '@media print': {
        body: {
          backgroundColor: '#fff !important',
          color: '#000 !important',
        },
        'nav, footer, .no-print': {
          display: 'none !important',
        },
      },
    }}
  />
);
