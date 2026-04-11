import React, { useState, useEffect } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Box,
  IconButton,
  Badge,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Button,
  InputBase,
  Select,
  FormControl,
  OutlinedInput,
  useTheme,
  alpha
} from '@mui/material';
import {
  Receipt,
  ShoppingCart,
  AccountCircle,
  ExitToApp,
  Dashboard,
  PersonAdd,
  Assignment
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { cartService } from '../../services/cartService';
import { categoryService } from '../../services/categoryService';
import { Category } from '../../src/types/ecommerce';
import { ThemeSelector } from '../ThemeSelector/ThemeSelector';

interface EcommerceLayoutProps {
  children: React.ReactNode;
  onSearch?: (query: string) => void;
  onCategoryChange?: (categoryId?: number) => void;
  selectedCategory?: number;
}

const EcommerceLayout: React.FC<EcommerceLayoutProps> = ({
  children,
  onSearch,
  onCategoryChange,
  selectedCategory
}) => {
  const { auth, logout, userRole } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [cartCount, setCartCount] = useState(0);
  const [categories, setCategories] = useState<Category[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    loadCategories();
    if (auth.isAuthenticated) {
      loadCartCount();
    }
  }, [auth.isAuthenticated]);

  const loadCategories = async () => {
    try {
      const cats = await categoryService.getAllCategories();
      setCategories(cats);
    } catch (error) {
      console.error('Error loading categories', error);
    }
  };

  const loadCartCount = async () => {
    try {
      const cart = await cartService.getCart();
      const count = cart.reduce((acc, item) => acc + item.cantidad, 0);
      setCartCount(count);
    } catch (error) {
      console.error('Error loading cart', error);
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
    navigate('/');
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (onSearch) onSearch(searchQuery);
  };

  const handleCategoryChange = (categoryId?: number) => {
    if (onCategoryChange) onCategoryChange(categoryId);
  };

  const nombreCompleto = auth.user?.nombres && auth.user?.apellidos
    ? `${auth.user.nombres} ${auth.user.apellidos}`
    : auth.user?.correo?.split('@')[0] || 'Usuario';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="sticky" color="default" elevation={1}>
        <Toolbar>
          <Typography
            variant="h6"
            component={Link}
            to="/"
            sx={{ textDecoration: 'none', color: 'inherit', fontWeight: 700, flexGrow: 0, mr: 4 }}
          >
            ElectroHome
          </Typography>

          <FormControl size="small" sx={{ minWidth: 200, mr: 2 }}>
            <Select
              displayEmpty
              value={selectedCategory || ''}
              onChange={(e) => handleCategoryChange(e.target.value ? Number(e.target.value) : undefined)}
              input={<OutlinedInput />}
            >
              <MenuItem value="">Todas las categorías</MenuItem>
              {categories.map((cat) => (
                <MenuItem key={cat.id} value={cat.id}>{cat.nombre}</MenuItem>
              ))}
            </Select>
          </FormControl>

          <Box component="form" onSubmit={handleSearchSubmit} sx={{ flexGrow: 1, mr: 2 }}>
            <Box sx={{
              position: 'relative',
              borderRadius: theme.shape.borderRadius,
              backgroundColor: alpha(theme.palette.common.black, 0.05),
              '&:hover': { backgroundColor: alpha(theme.palette.common.black, 0.1) },
              width: '100%'
            }}>
              <Box sx={{ padding: theme.spacing(0, 2), height: '100%', position: 'absolute', pointerEvents: 'none', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              </Box>
              <InputBase
                placeholder="Buscar productos…"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                sx={{
                  color: 'inherit',
                  width: '100%',
                  padding: theme.spacing(1, 1, 1, 0),
                  paddingLeft: `calc(1em + ${theme.spacing(4)})`,
                }}
              />
            </Box>
          </Box>

          {auth.isAuthenticated && (
            <IconButton color="inherit" component={Link} to="/cart" sx={{ mr: 1 }}>
              <Badge badgeContent={cartCount} color="secondary">
                <ShoppingCart />
              </Badge>
            </IconButton>
          )}

          {auth.isAuthenticated ? (
            <>
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
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
                PaperProps={{ sx: { mt: 1.5, minWidth: 200 } }}
              >
                {userRole !== 'Cliente' && (
                  <MenuItem component={Link} to="/app" onClick={handleMenuClose}>
                    <ListItemIcon><Dashboard fontSize="small" /></ListItemIcon>
                    <ListItemText>Área Propia</ListItemText>
                  </MenuItem>
                )}
                <MenuItem component={Link} to="/app/reclamos" onClick={handleMenuClose}>
                  <ListItemIcon><Assignment fontSize="small" /></ListItemIcon>
                  <ListItemText>Manejar Reclamos</ListItemText>
                </MenuItem>
                <MenuItem component={Link} to="/app/perfil" onClick={handleMenuClose}>
                  <ListItemIcon><AccountCircle fontSize="small" /></ListItemIcon>
                  <ListItemText>Mi Perfil</ListItemText>
                </MenuItem>
                <MenuItem component={Link} to="/mis-facturas" onClick={handleMenuClose}>
                  <ListItemIcon><Receipt fontSize="small" /></ListItemIcon>
                  <ListItemText>Ver mis facturas</ListItemText>
                </MenuItem>
                <Divider />
                <ThemeSelector variant="menu-item" />
                <MenuItem onClick={handleLogout}>
                  <ListItemIcon><ExitToApp fontSize="small" /></ListItemIcon>
                  <ListItemText>Cerrar Sesión</ListItemText>
                </MenuItem>
              </Menu>
            </>
          ) : (
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button color="inherit" component={Link} to="/register">
                Registrarse
              </Button>
              <Button color="inherit" component={Link} to="/login">
                Iniciar sesión
              </Button>
            </Box>
          )}
        </Toolbar>
      </AppBar>

      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        {children}
      </Box>
    </Box>
  );
};

export default EcommerceLayout;