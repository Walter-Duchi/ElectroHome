import { createTheme, Theme } from '@mui/material/styles';
import { esES } from '@mui/material/locale';

// Colores corporativos para Megacorporación
const corporateColors = {
  primary: {
    main: '#0056b3', // Azul corporativo serio
    light: '#4d84d6',
    dark: '#003b82',
    contrastText: '#ffffff',
  },
  secondary: {
    main: '#28a745', // Verde corporativo
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
};

// Tipografía corporativa
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
  ].join(','),
  h1: {
    fontSize: '2.5rem',
    fontWeight: 700,
    lineHeight: 1.2,
  },
  h2: {
    fontSize: '2rem',
    fontWeight: 600,
    lineHeight: 1.3,
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
    lineHeight: 1.6,
  },
  subtitle2: {
    fontSize: '0.875rem',
    fontWeight: 500,
    lineHeight: 1.6,
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
    textTransform: 'none',
    fontWeight: 600,
  },
};

// Componentes personalizados
const components = {
  MuiCssBaseline: {
    styleOverrides: {
      body: {
        scrollBehavior: 'smooth',
      },
      '*': {
        boxSizing: 'border-box',
      },
      a: {
        textDecoration: 'none',
        color: 'inherit',
      },
    },
  },
  MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        textTransform: 'none',
        fontWeight: 600,
        padding: '8px 24px',
        transition: 'all 0.3s ease',
      },
      containedPrimary: {
        boxShadow: '0 4px 12px rgba(0, 86, 179, 0.2)',
        '&:hover': {
          boxShadow: '0 6px 20px rgba(0, 86, 179, 0.3)',
          transform: 'translateY(-1px)',
        },
      },
      containedSecondary: {
        boxShadow: '0 4px 12px rgba(40, 167, 69, 0.2)',
        '&:hover': {
          boxShadow: '0 6px 20px rgba(40, 167, 69, 0.3)',
          transform: 'translateY(-1px)',
        },
      },
    },
    defaultProps: {
      disableElevation: true,
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        boxShadow: '0 2px 12px rgba(0, 0, 0, 0.08)',
        transition: 'box-shadow 0.3s ease',
        '&:hover': {
          boxShadow: '0 8px 24px rgba(0, 0, 0, 0.12)',
        },
      },
    },
  },
  MuiTextField: {
    defaultProps: {
      variant: 'outlined',
      fullWidth: true,
      size: 'small',
    },
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: 8,
        },
      },
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        backgroundColor: '#ffffff',
        color: '#212529',
        boxShadow: '0 2px 10px rgba(0, 0, 0, 0.08)',
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        borderRight: '1px solid #e9ecef',
      },
    },
  },
  MuiAlert: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        alignItems: 'center',
      },
    },
  },
  MuiDialog: {
    styleOverrides: {
      paper: {
        borderRadius: 12,
        padding: '2px',
      },
    },
  },
  MuiTable: {
    styleOverrides: {
      root: {
        borderCollapse: 'separate',
        borderSpacing: 0,
      },
    },
  },
  MuiTableCell: {
    styleOverrides: {
      root: {
        borderBottom: '1px solid #e9ecef',
        padding: '16px',
      },
      head: {
        backgroundColor: '#f8f9fa',
        fontWeight: 600,
        color: '#495057',
      },
    },
  },
};

// Crear tema corporativo
export const corporateTheme: Theme = createTheme(
  {
    palette: corporateColors,
    typography: corporateTypography,
    components: components,
    shape: {
      borderRadius: 8,
    },
    spacing: 8,
    breakpoints: {
      values: {
        xs: 0,
        sm: 600,
        md: 960,
        lg: 1280,
        xl: 1920,
      },
    },
  },
  esES // Soporte para español
);

// Tema oscuro opcional para modo oscuro
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
      },
    },
    typography: corporateTypography,
    components: components,
    shape: {
      borderRadius: 8,
    },
  },
  esES
);

export default corporateTheme;
