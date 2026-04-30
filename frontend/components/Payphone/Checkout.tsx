import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Divider,
  CircularProgress,
  Alert,
  Button,
  Grid,
} from '@mui/material';
import { cartService } from '../../services/cartService';
import type { CartItem } from '../../src/types/ecommerce';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';

interface PayphoneInitData {
  clientTransactionId: string;
  amount: number;
  amountWithoutTax: number;
  amountWithTax: number;
  tax: number;
  token: string;
  storeId: string;
  reference: string;
  currency: string;
}

declare global {
  interface Window {
    PPaymentButtonBox: any;
  }
}

const Checkout: React.FC = () => {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [initData, setInitData] = useState<PayphoneInitData | null>(null);

  useEffect(() => {
    loadCart();
  }, []);

  useEffect(() => {
    if (initData) {
      // Cargar scripts de Payphone si no están ya
      if (!document.querySelector('#payphone-css')) {
        const link = document.createElement('link');
        link.id = 'payphone-css';
        link.rel = 'stylesheet';
        link.href = 'https://cdn.payphonetodoesposible.com/box/v1.1/payphone-payment-box.css';
        document.head.appendChild(link);
      }
      if (!document.querySelector('#payphone-js')) {
        const script = document.createElement('script');
        script.id = 'payphone-js';
        script.type = 'module';
        script.src = 'https://cdn.payphonetodoesposible.com/box/v1.1/payphone-payment-box.js';
        document.head.appendChild(script);
        script.onload = renderPayphoneBox;
      } else {
        renderPayphoneBox();
      }
    }
  }, [initData]);

  const loadCart = async () => {
    try {
      const items = await cartService.getCart();
      if (items.length === 0) {
        navigate('/cart');
        return;
      }
      setCartItems(items);
      await initializePayphone();
    } catch (err) {
      setError('Error al cargar el carrito');
    } finally {
      setLoading(false);
    }
  };

  const initializePayphone = async () => {
    try {
      const response = await api.post('/payphone/init');
      setInitData(response.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al inicializar pago');
    }
  };

  const renderPayphoneBox = () => {
    if (!window.PPaymentButtonBox || !initData) return;

    // Limpiar contenedor previo si existe
    const container = document.getElementById('pp-button');
    if (container) container.innerHTML = '';

    const ppb = new window.PPaymentButtonBox({
      token: initData.token,
      clientTransactionId: initData.clientTransactionId,
      amount: initData.amount,
      amountWithoutTax: initData.amountWithoutTax,
      amountWithTax: initData.amountWithTax,
      tax: initData.tax,
      currency: initData.currency,
      storeId: initData.storeId,
      reference: initData.reference,
      // Opcionales
      lang: 'es',
      defaultMethod: 'card',
    });
    ppb.render('pp-button');
  };

  const handleGoBack = () => {
    navigate('/cart');
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ py: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Cargando información de pago...</Typography>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
        <Button variant="contained" onClick={handleGoBack}>Volver al carrito</Button>
      </Container>
    );
  }

  const total = cartItems.reduce((sum, item) => sum + item.subtotal, 0);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        Finalizar compra
      </Typography>
      <Divider sx={{ mb: 3 }} />
      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Resumen de tu pedido
            </Typography>
            {cartItems.map((item) => (
              <Box key={item.id} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography>
                  {item.nombreProducto} x {item.cantidad}
                </Typography>
                <Typography>${item.subtotal.toFixed(2)}</Typography>
              </Box>
            ))}
            <Divider sx={{ my: 2 }} />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', fontWeight: 'bold' }}>
              <Typography variant="h6">Total</Typography>
              <Typography variant="h6">${total.toFixed(2)}</Typography>
            </Box>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12, md: 5 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Método de pago
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Paga de forma segura con Payphone (tarjeta de crédito/débito o saldo Payphone)
            </Typography>
            {initData ? (
              <div id="pp-button" style={{ minHeight: '200px' }} />
            ) : (
              <CircularProgress size={24} />
            )}
            <Button
              variant="outlined"
              fullWidth
              sx={{ mt: 2 }}
              onClick={handleGoBack}
            >
              Cancelar y volver al carrito
            </Button>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default Checkout;