import { useTheme as useMuiTheme } from '@mui/material/styles';
import { themeUtils } from './index';

export const useCustomTheme = () => {
    const theme = useMuiTheme();

    return {
        ...theme,
        utils: themeUtils,
        // Funciones de utilidad específicas
        getResponsiveValue: (values: any) => {
            if (typeof window !== 'undefined') {
                const width = window.innerWidth;
                if (width < theme.breakpoints.values.sm) return values.xs || values.sm || values.md || values.lg || values.xl;
                if (width < theme.breakpoints.values.md) return values.sm || values.md || values.lg || values.xl;
                if (width < theme.breakpoints.values.lg) return values.md || values.lg || values.xl;
                if (width < theme.breakpoints.values.xl) return values.lg || values.xl;
                return values.xl || values.lg || values.md || values.sm || values.xs;
            }
            return values.md;
        },
        // Helper para márgenes/paddings consistentes
        spacing: (multiplier: number) => theme.spacing(multiplier),
        // Helper para breakpoints
        isMobile: () => {
            if (typeof window !== 'undefined') {
                return window.innerWidth < theme.breakpoints.values.md;
            }
            return false;
        },
        isTablet: () => {
            if (typeof window !== 'undefined') {
                return window.innerWidth >= theme.breakpoints.values.md &&
                    window.innerWidth < theme.breakpoints.values.lg;
            }
            return false;
        },
        isDesktop: () => {
            if (typeof window !== 'undefined') {
                return window.innerWidth >= theme.breakpoints.values.lg;
            }
            return true;
        },
    };
};

// Componentes predefinidos para consistencia
export const consistentComponents = {
    // Botón primario consistente
    PrimaryButton: {
        variant: 'contained' as const,
        color: 'primary' as const,
        size: 'medium' as const,
        sx: {
            fontWeight: 600,
            textTransform: 'none',
        },
    },
    // Card consistente
    Card: {
        elevation: 0,
        sx: {
            borderRadius: 2,
            border: '1px solid',
            borderColor: 'divider',
            overflow: 'hidden',
        },
    },
    // TextField consistente
    TextField: {
        variant: 'outlined' as const,
        size: 'medium' as const,
        fullWidth: true,
        sx: {
            '& .MuiOutlinedInput-root': {
                borderRadius: 1,
            },
        },
    },
    // Alert consistente
    Alert: {
        variant: 'standard' as const,
        sx: {
            borderRadius: 1,
            alignItems: 'center',
        },
    },
};