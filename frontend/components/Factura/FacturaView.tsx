import React, { useEffect, useState } from 'react';
import { Box, CircularProgress, Alert, Button } from '@mui/material';
import { GetApp } from '@mui/icons-material';
import api from '../../services/api';

interface FacturaViewProps {
    ventaId: number;
    onDownload?: () => void;
}

const FacturaView: React.FC<FacturaViewProps> = ({ ventaId, onDownload }) => {
    const [html, setHtml] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const loadFactura = async () => {
            try {
                const response = await api.get(`/factura/html/${ventaId}`, {
                    responseType: 'text',
                    headers: { Accept: 'text/html' }
                });
                setHtml(response.data);
            } catch (err) {
                setError('No se pudo cargar la factura');
            } finally {
                setLoading(false);
            }
        };
        loadFactura();
    }, [ventaId]);

    const handleDownload = async () => {
        try {
            const response = await api.get(`/factura/pdf/${ventaId}`, {
                responseType: 'blob'
            });
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `factura_${ventaId}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
            if (onDownload) onDownload();
        } catch (err) {
            console.error('Error descargando PDF', err);
        }
    };

    if (loading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error}</Alert>;
    if (!html) return null;

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                <Button variant="contained" startIcon={<GetApp />} onClick={handleDownload}>
                    Descargar PDF
                </Button>
            </Box>
            <div dangerouslySetInnerHTML={{ __html: html }} />
        </Box>
    );
};

export default FacturaView;