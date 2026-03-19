/* eslint-disable react-hooks/set-state-in-effect */
import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Container, Typography, Box, CircularProgress, Alert, Button } from '@mui/material';
import api from '../../services/api';

const PayphoneResponse: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);

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
        if (response.data.pdfUrl) {
          setPdfUrl(response.data.pdfUrl);
          // Descargar automáticamente después de 1 segundo
          setTimeout(() => {
            window.open(response.data.pdfUrl, '_blank');
          }, 1000);
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
      {status === 'success' && (
        <Box>
          <Alert severity="success" sx={{ mb: 3 }}>{message}</Alert>
          {pdfUrl && (
            <Button
              variant="contained"
              href={pdfUrl}
              target="_blank"
              sx={{ mr: 2 }}
            >
              Descargar factura
            </Button>
          )}
          <Button variant="outlined" onClick={() => navigate('/')}>
            Volver a la tienda
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
