import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Container, Typography, Box, CircularProgress, Alert, Button } from '@mui/material';
import api from '../../services/api';
import FacturaView from '../Factura/FacturaView';

const PayphoneResponse: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');
  const [ventaId, setVentaId] = useState<number | null>(null);
  const [facturaError, setFacturaError] = useState(false);

  useEffect(() => {
    const id = searchParams.get('id');
    const clientTxId = searchParams.get('clientTransactionId');

    if (!id || !clientTxId) {
      setStatus('error');
      setMessage('Parámetros de respuesta inválidos');
      return;
    }

    const confirmPayment = async () => {
      try {
        const response = await api.post('/payphone/confirm', { id: Number(id), clientTransactionId: clientTxId });
        setStatus('success');
        setMessage('¡Pago exitoso! Tu compra ha sido procesada.');
        if (response.data.ventaId) {
          setVentaId(response.data.ventaId);
          // Verificar si la factura está autorizada (podría fallar si hubo error en facturación)
          try {
            await api.get(`/factura/html/${response.data.ventaId}`, { responseType: 'text' });
          } catch (err) {
            setFacturaError(true);
            setMessage('El pago fue exitoso pero hubo un error al generar la factura. Contacta al administrador.');
          }
        }
      } catch (err: any) {
        setStatus('error');
        setMessage(err.response?.data?.error || 'Error al confirmar el pago');
      }
    };

    confirmPayment();
  }, [searchParams]);

  return (
    <Container maxWidth="md" sx={{ py: 8, textAlign: 'center' }}>
      {status === 'loading' && (
        <Box>
          <CircularProgress size={60} sx={{ mb: 2 }} />
          <Typography>Confirmando tu pago, por favor espera...</Typography>
        </Box>
      )}
      {status === 'success' && ventaId && !facturaError && (
        <Box>
          <Alert severity="success" sx={{ mb: 3 }}>{message}</Alert>
          <FacturaView ventaId={ventaId} />
          <Button variant="outlined" onClick={() => navigate('/')} sx={{ mt: 2 }}>
            Volver a la tienda
          </Button>
        </Box>
      )}
      {status === 'success' && facturaError && (
        <Box>
          <Alert severity="warning" sx={{ mb: 3 }}>{message}</Alert>
          <Button variant="contained" onClick={() => window.location.reload()} sx={{ mr: 2 }}>
            Reintentar
          </Button>
          <Button variant="outlined" onClick={() => navigate('/')}>
            Ir a la tienda
          </Button>
        </Box>
      )}
      {status === 'error' && (
        <Box>
          <Alert severity="error" sx={{ mb: 3 }}>{message}</Alert>
          <Button variant="contained" onClick={() => navigate('/cart')}>
            Volver al carrito
          </Button>
        </Box>
      )}
    </Container>
  );
};

export default PayphoneResponse;