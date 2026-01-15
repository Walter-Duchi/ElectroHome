import { AddReaction as AddReclamoIcon } from '@mui/icons-material';
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Engineering } from '@mui/icons-material';
import { LocalShipping } from '@mui/icons-material';
import { Assignment } from '@mui/icons-material';

import {
  AppBar,
  Box,
  CssBaseline,
  IconButton,
  Toolbar,
  Typography,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Button,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Notifications as NotificationsIcon,
  AccountCircle,
  Settings,
  ExitToApp,
  Dashboard as DashboardIcon,
  People,
  Business,
  Assessment,
  Add,
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import { userService } from '../../services/userService';
import CreateUserModal from '../Navbar/CreateUserModal';
import { ThemeSelector } from '../ThemeSelector/ThemeSelector';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

function DashboardLayout({ children }: DashboardLayoutProps) {
  const navigate = useNavigate();

  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [allowedRoles, setAllowedRoles] = useState<string[]>([]);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [loadingRoles, setLoadingRoles] = useState(false);
  const { auth, logout, userRole } = useAuth();

  useEffect(() => {
    if (auth.isAuthenticated && userRole && userRole !== 'Cliente') {
      loadAllowedRoles();
    }
  }, [auth.isAuthenticated, userRole]);

  const loadAllowedRoles = async () => {
    try {
      setLoadingRoles(true);
      const roles = await userService.getAllowedRoles();
      setAllowedRoles(roles);
    } catch (error) {
      console.error('Error loading allowed roles:', error);
    } finally {
      setLoadingRoles(false);
    }
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    handleMenuClose();
  };

  const canCreateUsers = () => {
    return userRole && userRole !== 'Cliente' && allowedRoles.length > 0;
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            Sistema de Gestión
          </Typography>

          {canCreateUsers() && (
            <Button
              variant="contained"
              color="secondary"
              startIcon={<Add />}
              onClick={() => setShowCreateModal(true)}
              disabled={loadingRoles}
              sx={{ mr: 2 }}
            >
              Crear Usuario
            </Button>
          )}

          {userRole === 'Tecnico' && (
          <Button
              variant="contained"
              color="primary"
              startIcon={<Engineering />}
              onClick={() => navigate('/tecnico')}
              sx={{ mr: 2 }}
          >
              Mis Revisiones
          </Button>
          )}

          {userRole === 'Revisor' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddReclamoIcon />}
              onClick={() => navigate('/crear-reclamo')}
              sx={{ mr: 2 }}
            >
              Crear Reclamo
            </Button>
          )}

          {userRole === 'Cliente' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<Assignment />}
              onClick={() => navigate('/mis-reclamos')}
              sx={{ mr: 2 }}
            >
              Mis Reclamos
            </Button>
          )}


          {userRole === 'Personal de Entrega' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<LocalShipping />}
              onClick={() => navigate('/entrega')}
              sx={{ mr: 2 }}
            >
              Procesar Entregas
            </Button>
          )}

          <IconButton onClick={handleMenuOpen} color="inherit">
            <AccountCircle />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
        PaperProps={{
          sx: {
            mt: 1.5,
            minWidth: 200,
            borderRadius: 2,
            boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
          },
        }}
      >
        <MenuItem onClick={handleMenuClose}>
          <ListItemIcon>
            <AccountCircle fontSize="small" />
          </ListItemIcon>
          <ListItemText>Mi Perfil</ListItemText>
        </MenuItem>
        
        {/* Selector de tema como ítem de menú */}
        <ThemeSelector variant="menu-item" />
        
        <MenuItem onClick={handleMenuClose}>
          <ListItemIcon>
            <Settings fontSize="small" />
          </ListItemIcon>
          <ListItemText>Configuración</ListItemText>
        </MenuItem>
        
        <Divider />
        
        <MenuItem onClick={handleLogout}>
          <ListItemIcon>
            <ExitToApp fontSize="small" />
          </ListItemIcon>
          <ListItemText>Cerrar Sesión</ListItemText>
        </MenuItem>
      </Menu>

      <Box
        component="nav"
        sx={{ flexShrink: { sm: 0 } }}
      >
       
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          mt: 8,
          backgroundColor: 'background.default',
          minHeight: '100vh',
        }}
      >
        {children}
      </Box>

      {showCreateModal && (
        <CreateUserModal
          allowedRoles={allowedRoles}
          onClose={() => setShowCreateModal(false)}
          currentUserRole={userRole || ''}
        />
      )}
    </Box>
  );
};

export default DashboardLayout;
