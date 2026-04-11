import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
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
  Tooltip,
} from '@mui/material';
import {
  Menu as MenuIcon,
  AccountCircle,
  ExitToApp,
  Add,
  ShoppingCart,
  Store,
  Assignment,
  Receipt,
  Dashboard
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import CreateUserModal from '../Navbar/CreateUserModal';
import { ThemeSelector } from '../ThemeSelector/ThemeSelector';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

function DashboardLayout({ children }: DashboardLayoutProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const { auth, logout, userRole } = useAuth();

  const nombreCompleto = auth.user?.nombres && auth.user?.apellidos
    ? `${auth.user.nombres} ${auth.user.apellidos}`
    : auth.user?.correo?.split('@')[0] || 'Usuario';

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
    return userRole === 'Administrador';
  };

  const getDashboardTitle = () => {
    switch (userRole) {
      case 'Administrador':
        return 'Panel de Administración';
      case 'Revisor':
        return 'Panel de Revisión';
      case 'Tecnico':
        return 'Panel Técnico';
      case 'Personal de Entrega':
        return 'Panel de Entregas';
      case 'Analista_Datos':
        return 'Panel de Análisis';
      case 'Encargado_Inventario':
        return 'Panel de Inventario';
      case 'Gestor_Productos':
        return 'Panel de Productos';
      case 'Cliente':
        return 'Mi Panel de Cliente';
      default:
        return 'Sistema de Gestión';
    }
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <CssBaseline />
      <AppBar position="fixed">
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {getDashboardTitle()}
          </Typography>

          <Tooltip title="Ir a la tienda">
            <Button
              color="inherit"
              startIcon={<Store />}
              onClick={() => navigate('/')}
              sx={{ mr: 2 }}
            >
              Tienda
            </Button>
          </Tooltip>

          {canCreateUsers() && (
            <Tooltip title="Crear nuevo usuario">
              <Button
                variant="contained"
                color="secondary"
                startIcon={<Add />}
                onClick={() => setShowCreateModal(true)}
                sx={{ mr: 2 }}
              >
                Crear Usuario
              </Button>
            </Tooltip>
          )}

          <Box sx={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }} onClick={handleMenuOpen}>
            <Box sx={{ textAlign: 'right', mr: 1 }}>
              <Typography variant="body2" sx={{ fontWeight: 500, lineHeight: 1.2 }}>
                {nombreCompleto}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {userRole}
              </Typography>
            </Box>
            <IconButton color="inherit" edge="end">
              <AccountCircle />
            </IconButton>
          </Box>
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
        {userRole !== 'Cliente' && (
          <MenuItem onClick={() => { handleMenuClose(); navigate('/app'); }}>
            <ListItemIcon>
              <Dashboard fontSize="small" />
            </ListItemIcon>
            <ListItemText>Área Propia</ListItemText>
          </MenuItem>
        )}
        <MenuItem onClick={() => { handleMenuClose(); navigate('/app/perfil'); }}>
          <ListItemIcon>
            <AccountCircle fontSize="small" />
          </ListItemIcon>
          <ListItemText>Mi Perfil</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => { handleMenuClose(); navigate('/app/reclamos'); }}>
          <ListItemIcon><Assignment fontSize="small" /></ListItemIcon>
          <ListItemText>Manejar Reclamos</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => { handleMenuClose(); navigate('/mis-facturas'); }}>
          <ListItemIcon><Receipt fontSize="small" /></ListItemIcon>
          <ListItemText>Mis Facturas</ListItemText>
        </MenuItem>
        <Divider />
        <ThemeSelector variant="menu-item" />
        <MenuItem onClick={handleLogout}>
          <ListItemIcon>
            <ExitToApp fontSize="small" />
          </ListItemIcon>
          <ListItemText>Cerrar Sesión</ListItemText>
        </MenuItem>
      </Menu>

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
          onClose={() => setShowCreateModal(false)}
        />
      )}
    </Box>
  );
};

export default DashboardLayout;