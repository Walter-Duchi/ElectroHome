/* eslint-disable react-hooks/immutability */
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
  Brightness4,
  Brightness7,
  SettingsBrightness,
  Search as SearchIcon
} from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import { useTheme as useCustomTheme } from '../../src/context/ThemeContext';
import { Link, useNavigate } from 'react-router-dom';
import { cartService } from '../../services/cartService';
import { categoryService } from '../../services/categoryService';
import { Category } from '../../src/types/ecommerce';

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
  const { auth, logout } = useAuth();
  const { mode, toggleMode } = useCustomTheme();
  const navigate = useNavigate();
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

  const getThemeIcon = () => {
    if (mode === 'auto') return <SettingsBrightness />;
    return mode === 'light' ? <Brightness7 /> : <Brightness4 />;
  };

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

          {/* Category Selector */}
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

          {/* Search Bar */}
          <Box component="form" onSubmit={handleSearchSubmit} sx={{ flexGrow: 1, mr: 2 }}>
            <Box sx={{
              position: 'relative',
              borderRadius: theme.shape.borderRadius,
              backgroundColor: alpha(theme.palette.common.black, 0.05),
              '&:hover': { backgroundColor: alpha(theme.palette.common.black, 0.1) },
              width: '100%'
            }}>
              <Box sx={{ padding: theme.spacing(0, 2), height: '100%', position: 'absolute', pointerEvents: 'none', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <SearchIcon />
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

          {/* Cart Icon */}
          <IconButton color="inherit" component={Link} to="/cart" sx={{ mr: 1 }}>
            <Badge badgeContent={cartCount} color="secondary">
              <ShoppingCart />
            </Badge>
          </IconButton>

          {/* Theme Toggle */}
          <IconButton color="inherit" onClick={toggleMode} sx={{ mr: 1 }}>
            {getThemeIcon()}
          </IconButton>

          {/* User Menu */}
          {auth.isAuthenticated ? (
            <>
              <IconButton color="inherit" onClick={handleMenuOpen}>
                <AccountCircle />
              </IconButton>
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
                PaperProps={{ sx: { mt: 1.5, minWidth: 200 } }}
              >
                <MenuItem component={Link} to="/cart" onClick={handleMenuClose}>
                  <ListItemIcon><ShoppingCart fontSize="small" /></ListItemIcon>
                  <ListItemText>Mi Carrito</ListItemText>
                </MenuItem>
                {/* Siempre mostrar acceso al dashboard para usuarios autenticados */}
                <MenuItem component={Link} to="/app" onClick={handleMenuClose}>
                  <ListItemIcon><Dashboard fontSize="small" /></ListItemIcon>
                  <ListItemText>
                    {auth.user?.rol === 'Cliente' ? 'Mi Panel de Cliente' : 'Ir al Dashboard'}
                  </ListItemText>
                </MenuItem>
                <MenuItem component={Link} to="/mis-facturas" onClick={handleMenuClose}>
                  <ListItemIcon><Receipt fontSize="small" /></ListItemIcon>
                  <ListItemText>Ver mis facturas</ListItemText>
                </MenuItem>
                <Divider />
                <MenuItem onClick={handleLogout}>
                  <ListItemIcon><ExitToApp fontSize="small" /></ListItemIcon>
                  <ListItemText>Cerrar Sesión</ListItemText>
                </MenuItem>
              </Menu>
            </>
          ) : (
            <Button color="inherit" component={Link} to="/login">
              Iniciar sesión
            </Button>
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
