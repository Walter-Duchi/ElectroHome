import { useTheme } from '../context/ThemeContext';

export const useThemeMode = () => {
    const { mode, setMode, toggleMode } = useTheme();

    const getThemeLabel = () => {
        switch (mode) {
            case 'light':
                return 'Modo Claro';
            case 'dark':
                return 'Modo Oscuro';
            case 'auto':
                return 'Modo Automático';
            default:
                return 'Tema';
        }
    };

    const getThemeIcon = () => {
        switch (mode) {
            case 'light':
                return '☀️';
            case 'dark':
                return '🌙';
            case 'auto':
                return '🔄';
            default:
                return '🎨';
        }
    };

    return {
        mode,
        setMode,
        toggleMode,
        getThemeLabel,
        getThemeIcon,
        isAuto: mode === 'auto',
        isLight: mode === 'light',
        isDark: mode === 'dark',
    };
};
