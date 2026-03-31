import React, { useEffect, useState } from 'react';
import {
    Container, Typography, Paper, Table, TableBody, TableCell,
    TableContainer, TableHead, TableRow, IconButton, CircularProgress, Alert
} from '@mui/material';
import { Visibility } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';

interface FacturaResumen {
    id: number;
    codigoFactura: string;
    fechaCompra: string;
    totalCompra: number;
    claveAcceso: string;
    numeroAutorizacion: string;
    fechaAutorizacion: string;
}

const MisFacturas: React.FC = () => {
    const [facturas, setFacturas] = useState<FacturaResumen[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        const loadFacturas = async () => {
            try {
                const response = await api.get('/factura/mis-facturas');
                setFacturas(response.data);
            } catch (err) {
                setError('No se pudieron cargar sus facturas');
            } finally {
                setLoading(false);
            }
        };
        loadFacturas();
    }, []);

    const handleVerFactura = (id: number) => {
        navigate(`/mis-facturas/${id}`);
    };

    if (loading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error}</Alert>;

    return (
        <Container maxWidth="lg" sx={{ py: 4 }}>
            <Typography variant="h4" gutterBottom>Mis Facturas</Typography>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Código</TableCell>
                            <TableCell>Fecha</TableCell>
                            <TableCell>Total</TableCell>
                            <TableCell>Clave Acceso</TableCell>
                            <TableCell>Fecha Autorización</TableCell>
                            <TableCell align="center">Ver</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {facturas.map((f) => (
                            <TableRow key={f.id}>
                                <TableCell>{f.codigoFactura}</TableCell>
                                <TableCell>{new Date(f.fechaCompra).toLocaleDateString()}</TableCell>
                                <TableCell>${f.totalCompra.toFixed(2)}</TableCell>
                                <TableCell>{f.claveAcceso?.substring(0, 20)}...</TableCell>
                                <TableCell>{f.fechaAutorizacion ? new Date(f.fechaAutorizacion).toLocaleDateString() : '-'}</TableCell>
                                <TableCell align="center">
                                    <IconButton onClick={() => handleVerFactura(f.id)} color="primary">
                                        <Visibility />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Container>
    );
};

export default MisFacturas;