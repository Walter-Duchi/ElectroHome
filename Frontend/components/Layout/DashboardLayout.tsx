import { AddReaction as AddReclamoIcon } from '@mui/icons-material';
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import {
  AppBar,
  Box,
  CssBaseline,
  Drawer,
  IconButton,
  Toolbar,
  Typography,
  Avatar,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Badge,
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

const drawerWidth = 280;

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

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
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

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 3, textAlign: 'center', borderBottom: 1, borderColor: 'divider' }}>
        <Typography variant="h6" sx={{ fontWeight: 700, color: 'primary.main' }}>
          CORPORATE SYSTEM
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Sistema de Gestión Empresarial
        </Typography>
      </Box>
      
      <Box sx={{ flexGrow: 1, p: 2 }}>
        <Box sx={{ mb: 3, p: 2, bgcolor: 'action.selected', borderRadius: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Avatar sx={{ bgcolor: 'primary.main', width: 40, height: 40 }}>
              {auth.user?.correo.charAt(0).toUpperCase()}
            </Avatar>
            <Box>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                {auth.user?.correo}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {auth.user?.rol}
              </Typography>
            </Box>
          </Box>
        </Box>

        <Divider sx={{ my: 2 }} />

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 2,
              p: 2,
              borderRadius: 2,
              bgcolor: 'primary.main',
              color: 'white',
            }}
          >
            <DashboardIcon />
            <Typography variant="body2" sx={{ fontWeight: 600 }}>
              Panel Principal
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, p: 2, borderRadius: 2 }}>
            <People />
            <Typography variant="body2">Gestión de Usuarios</Typography>
          </Box>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, p: 2, borderRadius: 2 }}>
            <Business />
            <Typography variant="body2">Clientes</Typography>
          </Box>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, p: 2, borderRadius: 2 }}>
            <Assessment />
            <Typography variant="body2">Reportes</Typography>
          </Box>
        </Box>
      </Box>

      <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider' }}>
        <Typography variant="caption" color="text.secondary">
          v2.1.0 • Sistema Corporativo
        </Typography>
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
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

          <IconButton color="inherit" sx={{ mr: 2 }}>
            <Badge badgeContent={3} color="error">
              <NotificationsIcon />
            </Badge>
          </IconButton>

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
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
            },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
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
