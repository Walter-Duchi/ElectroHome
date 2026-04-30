import React from 'react';
import { useParams } from 'react-router-dom';
import { Container, Typography } from '@mui/material';
import FacturaView from './FacturaView';

const DetalleFactura: React.FC = () => {
    const { ventaId } = useParams<{ ventaId: string }>();
    const id = parseInt(ventaId || '0');

    return (
        <Container maxWidth="lg" sx={{ py: 4 }}>
            <Typography variant="h4" gutterBottom>Detalle de Factura</Typography>
            <FacturaView ventaId={id} />
        </Container>
    );
};

export default DetalleFactura;