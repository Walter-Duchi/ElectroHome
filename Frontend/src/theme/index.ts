import { createTheme, Theme } from '@mui/material/styles';
import { esES } from '@mui/material/locale';

// Paleta de colores corporativa optimizada
const corporateColors = {
  primary: {
    main: '#0056b3',
    light: '#4d84d6',
    dark: '#003b82',
    contrastText: '#ffffff',
  },
  secondary: {
    main: '#28a745',
    light: '#5fd85f',
    dark: '#007a2e',
    contrastText: '#ffffff',
  },
  error: {
    main: '#dc3545',
    light: '#e57373',
    dark: '#c62828',
  },
  warning: {
    main: '#ffc107',
    light: '#ffd54f',
    dark: '#ff8f00',
  },
  info: {
    main: '#17a2b8',
    light: '#4fc3f7',
    dark: '#0288d1',
  },
  success: {
    main: '#28a745',
    light: '#81c784',
    dark: '#388e3c',
  },
  background: {
    default: '#f8f9fa',
    paper: '#ffffff',
  },
  text: {
    primary: '#212529',
    secondary: '#6c757d',
    disabled: '#adb5bd',
  },
  grey: {
    50: '#f8f9fa',
    100: '#e9ecef',
    200: '#dee2e6',
    300: '#ced4da',
    400: '#adb5bd',
    500: '#6c757d',
    600: '#495057',
    700: '#343a40',
    800: '#212529',
    900: '#121416',
  },
  divider: '#e9ecef',
  action: {
    active: '#495057',
    hover: 'rgba(0, 86, 179, 0.04)',
    selected: 'rgba(0, 86, 179, 0.08)',
    disabled: '#adb5bd',
    disabledBackground: 'rgba(0, 0, 0, 0.12)',
  },
};

// Tipografía corporativa optimizada para todos los navegadores
const corporateTypography = {
  fontFamily: [
    'Inter',
    '-apple-system',
    'BlinkMacSystemFont',
    '"Segoe UI"',
    'Roboto',
    '"Helvetica Neue"',
    'Arial',
    'sans-serif',
    '"Apple Color Emoji"',
    '"Segoe UI Emoji"',
    '"Segoe UI Symbol"',
  ].join(','),
  htmlFontSize: 16,
  fontSize: 14,
  fontWeightLight: 300,
  fontWeightRegular: 400,
  fontWeightMedium: 500,
  fontWeightBold: 600,
  h1: {
    fontSize: '2.5rem',
    fontWeight: 700,
    lineHeight: 1.2,
    letterSpacing: '-0.01562em',
  },
  h2: {
    fontSize: '2rem',
    fontWeight: 600,
    lineHeight: 1.3,
    letterSpacing: '-0.00833em',
  },
  h3: {
    fontSize: '1.75rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  h4: {
    fontSize: '1.5rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  h5: {
    fontSize: '1.25rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  h6: {
    fontSize: '1rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  subtitle1: {
    fontSize: '1rem',
    fontWeight: 500,
    lineHeight: 1.75,
  },
  subtitle2: {
    fontSize: '0.875rem',
    fontWeight: 500,
    lineHeight: 1.57,
  },
  body1: {
    fontSize: '1rem',
    lineHeight: 1.6,
  },
  body2: {
    fontSize: '0.875rem',
    lineHeight: 1.6,
  },
  button: {
    fontSize: '0.875rem',
    fontWeight: 600,
    textTransform: 'none',
    letterSpacing: '0.02857em',
  },
  caption: {
    fontSize: '0.75rem',
    lineHeight: 1.66,
  },
  overline: {
    fontSize: '0.75rem',
    fontWeight: 600,
    lineHeight: 2.66,
    letterSpacing: '0.08333em',
    textTransform: 'uppercase',
  },
};

// Configuración de componentes optimizada para consistencia entre navegadores
const components = {
  MuiCssBaseline: {
    styleOverrides: {
      // Normalización para consistencia entre navegadores
      html: {
        WebkitFontSmoothing: 'antialiased',
        MozOsxFontSmoothing: 'grayscale',
        boxSizing: 'border-box',
        WebkitTextSizeAdjust: '100%',
        msTextSizeAdjust: '100%',
        scrollBehavior: 'smooth',
      },
      '*, *::before, *::after': {
        boxSizing: 'inherit',
        margin: 0,
        padding: 0,
      },
      body: {
        margin: 0,
        backgroundColor: '#f8f9fa',
        fontFamily: corporateTypography.fontFamily,
        fontSize: '14px',
        lineHeight: '1.6',
        color: corporateColors.text.primary,
        overflowX: 'hidden',
      },
      '#root': {
        minHeight: '100vh',
        display: 'flex',
        flexDirection: 'column',
      },
      // Estilos consistentes para enlaces
      a: {
        color: corporateColors.primary.main,
        textDecoration: 'none',
        '&:hover': {
          textDecoration: 'underline',
        },
      },
      // Estilos para imágenes responsivas
      img: {
        maxWidth: '100%',
        height: 'auto',
        display: 'block',
      },
      // Normalización de formularios
      'input, button, textarea, select': {
        fontFamily: 'inherit',
        fontSize: 'inherit',
        lineHeight: 'inherit',
      },
      // Scrollbar consistente
      '::-webkit-scrollbar': {
        width: '10px',
        height: '10px',
      },
      '::-webkit-scrollbar-track': {
        background: corporateColors.grey[100],
      },
      '::-webkit-scrollbar-thumb': {
        background: corporateColors.grey[400],
        borderRadius: '5px',
        '&:hover': {
          background: corporateColors.grey[500],
        },
      },
      // Para Firefox
      scrollbarWidth: 'thin',
      scrollbarColor: `${corporateColors.grey[400]} ${corporateColors.grey[100]}`,
    },
  },
  MuiButton: {
    defaultProps: {
      disableElevation: true,
      disableRipple: false,
    },
    styleOverrides: {
      root: {
        borderRadius: '8px',
        textTransform: 'none',
        fontWeight: 600,
        padding: '8px 24px',
        transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
        minHeight: '40px',
        '&:hover': {
          transform: 'translateY(-1px)',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
        },
        '&:active': {
          transform: 'translateY(0)',
        },
      },
      containedPrimary: {
        background: `linear-gradient(135deg, ${corporateColors.primary.main} 0%, ${corporateColors.primary.dark} 100%)`,
        '&:hover': {
          background: `linear-gradient(135deg, ${corporateColors.primary.light} 0%, ${corporateColors.primary.main} 100%)`,
        },
      },
      containedSecondary: {
        background: `linear-gradient(135deg, ${corporateColors.secondary.main} 0%, ${corporateColors.secondary.dark} 100%)`,
        '&:hover': {
          background: `linear-gradient(135deg, ${corporateColors.secondary.light} 0%, ${corporateColors.secondary.main} 100%)`,
        },
      },
      outlined: {
        borderWidth: '2px',
        '&:hover': {
          borderWidth: '2px',
        },
      },
      sizeSmall: {
        padding: '6px 16px',
        minHeight: '32px',
        fontSize: '0.8125rem',
      },
      sizeLarge: {
        padding: '10px 32px',
        minHeight: '48px',
        fontSize: '0.9375rem',
      },
    },
  },
  MuiTextField: {
    defaultProps: {
      variant: 'outlined',
      size: 'medium',
      fullWidth: true,
    },
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: '8px',
          transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
          '&:hover .MuiOutlinedInput-notchedOutline': {
            borderColor: corporateColors.primary.light,
            borderWidth: '2px',
          },
          '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
            borderColor: corporateColors.primary.main,
            borderWidth: '2px',
          },
        },
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0, 0, 0, 0.08)',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        border: `1px solid ${corporateColors.divider}`,
        overflow: 'hidden',
        '&:hover': {
          boxShadow: '0 8px 24px rgba(0, 0, 0, 0.12)',
          transform: 'translateY(-2px)',
        },
      },
    },
  },
  MuiPaper: {
    defaultProps: {
      elevation: 0,
    },
    styleOverrides: {
      root: {
        backgroundImage: 'none',
        borderRadius: '12px',
      },
      outlined: {
        border: `1px solid ${corporateColors.divider}`,
      },
    },
  },
  MuiAppBar: {
    defaultProps: {
      elevation: 0,
      color: 'default',
    },
    styleOverrides: {
      root: {
        backgroundColor: '#ffffff',
        borderBottom: `1px solid ${corporateColors.divider}`,
        backdropFilter: 'blur(10px)',
        WebkitBackdropFilter: 'blur(10px)',
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        borderRight: `1px solid ${corporateColors.divider}`,
        boxShadow: '2px 0 24px rgba(0, 0, 0, 0.08)',
      },
    },
  },
  MuiAlert: {
    styleOverrides: {
      root: {
        borderRadius: '8px',
        alignItems: 'center',
        padding: '12px 16px',
      },
      icon: {
        alignItems: 'center',
      },
    },
  },
  MuiDialog: {
    styleOverrides: {
      paper: {
        borderRadius: '16px',
        margin: '16px',
        overflow: 'hidden',
      },
    },
  },
  MuiMenu: {
    styleOverrides: {
      paper: {
        borderRadius: '8px',
        boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)',
        border: `1px solid ${corporateColors.divider}`,
        marginTop: '8px',
      },
    },
  },
  MuiLinearProgress: {
    styleOverrides: {
      root: {
        borderRadius: '3px',
        height: '6px',
        backgroundColor: corporateColors.grey[200],
      },
      bar: {
        borderRadius: '3px',
      },
    },
  },
  MuiChip: {
    styleOverrides: {
      root: {
        borderRadius: '6px',
        fontWeight: 500,
        height: 'auto',
        minHeight: '32px',
      },
    },
  },
  MuiAvatar: {
    styleOverrides: {
      root: {
        width: '40px',
        height: '40px',
        fontSize: '1rem',
      },
    },
  },
  MuiDivider: {
    styleOverrides: {
      root: {
        borderColor: corporateColors.divider,
      },
    },
  },
  MuiListItem: {
    styleOverrides: {
      root: {
        borderRadius: '8px',
        margin: '2px 0',
        '&:hover': {
          backgroundColor: corporateColors.action.hover,
        },
        '&.Mui-selected': {
          backgroundColor: corporateColors.action.selected,
          '&:hover': {
            backgroundColor: corporateColors.action.selected,
          },
        },
      },
    },
  },
  MuiTableCell: {
    styleOverrides: {
      root: {
        borderBottom: `1px solid ${corporateColors.divider}`,
        padding: '16px',
      },
      head: {
        backgroundColor: corporateColors.grey[50],
        fontWeight: 600,
        color: corporateColors.text.primary,
      },
    },
  },
  MuiTableRow: {
    styleOverrides: {
      root: {
        '&:hover': {
          backgroundColor: corporateColors.action.hover,
        },
        '&.Mui-selected': {
          backgroundColor: corporateColors.action.selected,
        },
      },
    },
  },
  MuiBadge: {
    styleOverrides: {
      badge: {
        fontWeight: 600,
        minWidth: '20px',
        height: '20px',
      },
    },
  },
  MuiStepper: {
    styleOverrides: {
      root: {
        padding: '24px 0',
      },
    },
  },
  MuiStepIcon: {
    styleOverrides: {
      root: {
        '&.Mui-completed': {
          color: corporateColors.success.main,
        },
        '&.Mui-active': {
          color: corporateColors.primary.main,
        },
      },
    },
  },
  MuiSkeleton: {
    defaultProps: {
      animation: 'wave',
    },
    styleOverrides: {
      root: {
        backgroundColor: corporateColors.grey[200],
      },
    },
  },
};

// Configuración de breakpoints para responsividad consistente
const breakpoints = {
  values: {
    xs: 0,
    sm: 600,
    md: 960,
    lg: 1280,
    xl: 1920,
  },
};

// Configuración de sombras consistentes
const shadows = [
  'none',
  '0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24)',
  '0 3px 6px rgba(0,0,0,0.16), 0 3px 6px rgba(0,0,0,0.23)',
  '0 10px 20px rgba(0,0,0,0.1), 0 6px 6px rgba(0,0,0,0.1)',
  '0 14px 28px rgba(0,0,0,0.12), 0 10px 10px rgba(0,0,0,0.08)',
  '0 19px 38px rgba(0,0,0,0.14), 0 15px 12px rgba(0,0,0,0.06)',
  '0 24px 48px rgba(0,0,0,0.16), 0 20px 20px rgba(0,0,0,0.08)',
  ...Array(19).fill('none'),
];

// Crear tema corporativo principal
export const corporateTheme: Theme = createTheme(
  {
    palette: corporateColors,
    typography: corporateTypography,
    components,
    breakpoints,
    shadows,
    shape: {
      borderRadius: 8,
    },
    spacing: 8,
    transitions: {
      duration: {
        shortest: 150,
        shorter: 200,
        short: 250,
        standard: 300,
        complex: 375,
        enteringScreen: 225,
        leavingScreen: 195,
      },
      easing: {
        easeInOut: 'cubic-bezier(0.4, 0, 0.2, 1)',
        easeOut: 'cubic-bezier(0.0, 0, 0.2, 1)',
        easeIn: 'cubic-bezier(0.4, 0, 1, 1)',
        sharp: 'cubic-bezier(0.4, 0, 0.6, 1)',
      },
    },
    zIndex: {
      mobileStepper: 1000,
      speedDial: 1050,
      appBar: 1100,
      drawer: 1200,
      modal: 1300,
      snackbar: 1400,
      tooltip: 1500,
    },
  },
  esES
);

// Tema oscuro corporativo
export const darkTheme: Theme = createTheme(
  {
    palette: {
      mode: 'dark',
      primary: corporateColors.primary,
      secondary: corporateColors.secondary,
      background: {
        default: '#121416',
        paper: '#1e2125',
      },
      text: {
        primary: '#e9ecef',
        secondary: '#adb5bd',
        disabled: '#6c757d',
      },
      divider: 'rgba(255, 255, 255, 0.12)',
      action: {
        active: '#e9ecef',
        hover: 'rgba(255, 255, 255, 0.08)',
        selected: 'rgba(255, 255, 255, 0.16)',
        disabled: 'rgba(255, 255, 255, 0.3)',
        disabledBackground: 'rgba(255, 255, 255, 0.12)',
      },
    },
    typography: corporateTypography,
    components,
    breakpoints,
    shadows: [
      'none',
      '0 1px 3px rgba(0,0,0,0.3), 0 1px 2px rgba(0,0,0,0.4)',
      '0 3px 6px rgba(0,0,0,0.4), 0 3px 6px rgba(0,0,0,0.5)',
      '0 10px 20px rgba(0,0,0,0.3), 0 6px 6px rgba(0,0,0,0.2)',
      '0 14px 28px rgba(0,0,0,0.35), 0 10px 10px rgba(0,0,0,0.25)',
      '0 19px 38px rgba(0,0,0,0.4), 0 15px 12px rgba(0,0,0,0.3)',
      '0 24px 48px rgba(0,0,0,0.45), 0 20px 20px rgba(0,0,0,0.35)',
      ...Array(19).fill('none'),
    ],
    shape: {
      borderRadius: 8,
    },
  },
  esES
);

// Función de utilidad para usar el tema en componentes
export const themeUtils = {
  getSpacing: (multiple: number) => `${multiple * 8}px`,
  getTransition: (property: string = 'all') => 
    `${property} 0.3s cubic-bezier(0.4, 0, 0.2, 1)`,
  getBorderRadius: (size: 'sm' | 'md' | 'lg' | 'xl' = 'md') => {
    const sizes = {
      sm: '4px',
      md: '8px',
      lg: '12px',
      xl: '16px',
    };
    return sizes[size];
  },
  getBoxShadow: (level: 0 | 1 | 2 | 3 | 4 | 5 | 6 = 1) => shadows[level],
};

export default corporateTheme;
