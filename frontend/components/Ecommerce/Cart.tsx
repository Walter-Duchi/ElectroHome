import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Button,
  TextField,
  Divider,
  Alert,
  CircularProgress
} from '@mui/material';
import { Delete, Add, Remove, ShoppingCart as CartIcon } from '@mui/icons-material';
import { useAuth } from '../../services/authContext';
import { cartService } from '../../services/cartService';
import type { CartItem } from '../../src/types/ecommerce';
import { useNavigate } from 'react-router-dom';

const Cart: React.FC = () => {
  const { auth } = useAuth();
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!auth.isAuthenticated) {
      navigate('/login');
    } else {
      loadCart();
    }
  }, [auth.isAuthenticated]);

  const loadCart = async () => {
    setLoading(true);
    try {
      const items = await cartService.getCart();
      setCartItems(items);
    } catch (err) {
      setError('Error al cargar el carrito');
    } finally {
      setLoading(false);
    }
  };

  const handleQuantityChange = async (productoId: number, newQuantity: number) => {
    if (newQuantity < 1) return;
    try {
      await cartService.updateQuantity(productoId, newQuantity);
      await loadCart();
    } catch (err) {
      setError('Error al actualizar cantidad');
    }
  };

  const handleRemoveItem = async (productoId: number) => {
    try {
      await cartService.removeItem(productoId);
      await loadCart();
    } catch (err) {
      setError('Error al eliminar producto');
    }
  };

  const handleClearCart = async () => {
    try {
      await cartService.clearCart();
      await loadCart();
    } catch (err) {
      setError('Error al vaciar el carrito');
    }
  };

  const total = cartItems.reduce((sum, item) => sum + item.subtotal, 0);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        <CartIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
        Mi Carrito
      </Typography>
      <Divider sx={{ mb: 3 }} />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {cartItems.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Tu carrito está vacío
          </Typography>
          <Button variant="contained" onClick={() => navigate('/')}>
            Ir a la tienda
          </Button>
        </Paper>
      ) : (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Producto</TableCell>
                  <TableCell align="right">Precio Unit.</TableCell>
                  <TableCell align="center">Cantidad</TableCell>
                  <TableCell align="right">Subtotal</TableCell>
                  <TableCell align="center">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {cartItems.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        {/* Contenedor cuadrado para la imagen */}
                        <Box
                          sx={{
                            width: 60,
                            height: 60,
                            flexShrink: 0,
                            mr: 2,
                            overflow: 'hidden',
                            borderRadius: 1,
                          }}
                        >
                          <img
                            src={item.imagenUrl || '/placeholder.jpg'}
                            alt={item.nombreProducto}
                            style={{
                              width: '100%',
                              height: '100%',
                              objectFit: 'cover',
                            }}
                          />
                        </Box>
                        <Typography variant="body1">{item.nombreProducto}</Typography>
                      </Box>
                    </TableCell>
                    <TableCell align="right">${item.precioUnitario.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                        <IconButton
                          size="small"
                          onClick={() => handleQuantityChange(item.productoId, item.cantidad - 1)}
                        >
                          <Remove />
                        </IconButton>
                        <TextField
                          value={item.cantidad}
                          size="small"
                          type="number"
                          inputProps={{ min: 1, style: { textAlign: 'center', width: 60 } }}
                          onChange={(e) => {
                            const val = parseInt(e.target.value);
                            if (!isNaN(val) && val >= 1) {
                              handleQuantityChange(item.productoId, val);
                            }
                          }}
                        />
                        <IconButton
                          size="small"
                          onClick={() => handleQuantityChange(item.productoId, item.cantidad + 1)}
                        >
                          <Add />
                        </IconButton>
                      </Box>
                    </TableCell>
                    <TableCell align="right">${item.subtotal.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <IconButton color="error" onClick={() => handleRemoveItem(item.productoId)}>
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Button variant="outlined" color="error" onClick={handleClearCart}>
              Vaciar carrito
            </Button>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="h5" gutterBottom>
                Total: ${total.toFixed(2)}
              </Typography>
              <Button
                variant="contained"
                size="large"
                onClick={() => navigate('/checkout')}
              >
                Proceder al pago
              </Button>
            </Box>
          </Box>
        </>
      )}
    </Container>
  );
};

export default Cart;