import React from 'react';
import {
  IconButton,
  Tooltip,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Box,
  Typography,
} from '@mui/material';
import {
  BrightnessAuto,
  LightMode,
  DarkMode,
  Palette,
} from '@mui/icons-material';
import { useTheme } from '../../src/context/ThemeContext';

interface ThemeSelectorProps {
  variant?: 'icon' | 'menu-item';
  showLabel?: boolean;
}

export const ThemeSelector: React.FC<ThemeSelectorProps> = ({
  variant = 'icon',
  showLabel = false
}) => {
  const { mode, setMode } = useTheme();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleSelect = (selectedMode: 'light' | 'dark' | 'auto') => {
    setMode(selectedMode);
    handleClose();
  };

  const getCurrentIcon = () => {
    switch (mode) {
      case 'light':
        return <LightMode />;
      case 'dark':
        return <DarkMode />;
      case 'auto':
        return <BrightnessAuto />;
      default:
        return <Palette />;
    }
  };

  const getCurrentLabel = () => {
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

  if (variant === 'menu-item') {
    return (
      <>
        <MenuItem onClick={handleClick}>
          <ListItemIcon>
            <Palette fontSize="small" />
          </ListItemIcon>
          <ListItemText>Tema</ListItemText>
          <Typography variant="body2" color="text.secondary" sx={{ ml: 2 }}>
            {getCurrentLabel()}
          </Typography>
        </MenuItem>
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleClose}
          anchorOrigin={{
            vertical: 'top',
            horizontal: 'right',
          }}
          transformOrigin={{
            vertical: 'top',
            horizontal: 'left',
          }}
        >
          <MenuItem
            onClick={() => handleSelect('auto')}
            selected={mode === 'auto'}
          >
            <ListItemIcon>
              <BrightnessAuto fontSize="small" />
            </ListItemIcon>
            <ListItemText>Automático</ListItemText>
          </MenuItem>
          <MenuItem
            onClick={() => handleSelect('light')}
            selected={mode === 'light'}
          >
            <ListItemIcon>
              <LightMode fontSize="small" />
            </ListItemIcon>
            <ListItemText>Claro</ListItemText>
          </MenuItem>
          <MenuItem
            onClick={() => handleSelect('dark')}
            selected={mode === 'dark'}
          >
            <ListItemIcon>
              <DarkMode fontSize="small" />
            </ListItemIcon>
            <ListItemText>Oscuro</ListItemText>
          </MenuItem>
        </Menu>
      </>
    );
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Tooltip title="Cambiar tema">
        <IconButton
          onClick={handleClick}
          color="inherit"
          size="small"
          sx={{
            width: 40,
            height: 40,
            '&:hover': {
              backgroundColor: 'action.hover',
            },
          }}
        >
          {getCurrentIcon()}
        </IconButton>
      </Tooltip>

      {showLabel && (
        <Typography variant="body2" color="text.secondary">
          {getCurrentLabel()}
        </Typography>
      )}

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
      >
        <MenuItem
          onClick={() => handleSelect('auto')}
          selected={mode === 'auto'}
        >
          <ListItemIcon>
            <BrightnessAuto fontSize="small" />
          </ListItemIcon>
          <ListItemText>Automático</ListItemText>

        </MenuItem>
        <MenuItem
          onClick={() => handleSelect('light')}
          selected={mode === 'light'}
        >
          <ListItemIcon>
            <LightMode fontSize="small" />
          </ListItemIcon>
          <ListItemText>Claro</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={() => handleSelect('dark')}
          selected={mode === 'dark'}
        >
          <ListItemIcon>
            <DarkMode fontSize="small" />
          </ListItemIcon>
          <ListItemText>Oscuro</ListItemText>
        </MenuItem>
      </Menu>
    </Box>
  );
};

export default ThemeSelector;
