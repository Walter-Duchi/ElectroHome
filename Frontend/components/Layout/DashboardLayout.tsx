import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
  Notifications as NotificationsIcon,
  AccountCircle,
  Settings,
  ExitToApp,
  People,
  Add,
  Engineering,
  LocalShipping,
  Assignment,
  Receipt,
  Analytics,
  Inventory,
  ShoppingCart,
  Store,
  ShowChart,
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import CreateUserModal from '../Navbar/CreateUserModal';
import { ThemeSelector } from '../ThemeSelector/ThemeSelector';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

function DashboardLayout({ children }: DashboardLayoutProps) {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const { auth, logout, userRole } = useAuth();

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
      case 'Vendedor':
        return 'Panel de Ventas';
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

          {/* Botón para ir a la tienda */}
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

          {userRole === 'Tecnico' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<Engineering />}
              onClick={() => navigate('/app/tecnico')}
              sx={{ mr: 2 }}
            >
              Mis Revisiones
            </Button>
          )}

          {userRole === 'Revisor' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<Receipt />}
              onClick={() => navigate('/app/reclamo')}
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
              onClick={() => navigate('/app/mis-reclamos')}
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
              onClick={() => navigate('/app/entrega')}
              sx={{ mr: 2 }}
            >
              Procesar Entregas
            </Button>
          )}

          {userRole === 'Vendedor' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<ShoppingCart />}
              onClick={() => navigate('/app/ventas')}
              sx={{ mr: 2 }}
            >
              Ventas
            </Button>
          )}

          {userRole === 'Analista_Datos' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<Analytics />}
              onClick={() => navigate('/app/analisis')}
              sx={{ mr: 2 }}
            >
              Análisis
            </Button>
          )}

          {userRole === 'Encargado_Inventario' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<Inventory />}
              onClick={() => navigate('/app/inventario')}
              sx={{ mr: 2 }}
            >
              Inventario
            </Button>
          )}

          {userRole === 'Analista_Datos' && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<ShowChart />}
              onClick={() => navigate('/app/analista')}
              sx={{ mr: 2 }}
            >
              Dashboard
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
