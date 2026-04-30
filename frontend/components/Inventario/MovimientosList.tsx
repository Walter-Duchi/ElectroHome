import React, { useState, useEffect } from 'react';
import {
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, TextField, MenuItem, Box, Typography, Chip, FormControl, InputLabel, Select
} from '@mui/material';
import { inventarioService } from '../../services/inventarioService';
import type { MovimientoInventario } from '../../src/types/inventario';
import type { ProductoManagement } from '../../src/types/producto';

interface MovimientosListProps {
  productos: ProductoManagement[];
}

const MovimientosList: React.FC<MovimientosListProps> = ({ productos }) => {
  const [movimientos, setMovimientos] = useState<MovimientoInventario[]>([]);
  const [filtroProducto, setFiltroProducto] = useState<number | ''>('');
  const [filtroDesde, setFiltroDesde] = useState('');
  const [filtroHasta, setFiltroHasta] = useState('');

  useEffect(() => {
    cargarMovimientos();
  }, [filtroProducto, filtroDesde, filtroHasta]);

  const cargarMovimientos = async () => {
    try {
      const data = await inventarioService.getMovimientos(
        filtroProducto || undefined,
        filtroDesde || undefined,
        filtroHasta || undefined
      );
      setMovimientos(data);
    } catch (error) {
      console.error(error);
    }
  };

  const getTipoColor = (tipo: string) => {
    switch (tipo) {
      case 'Entrada': return 'success';
      case 'Salida': return 'error';
      case 'Ajuste': return 'warning';
      case 'Devolucion': return 'info';
      default: return 'default';
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Producto</InputLabel>
          <Select
            value={filtroProducto}
            label="Producto"
            onChange={(e) => setFiltroProducto(e.target.value as number | '')}
          >
            <MenuItem value="">Todos</MenuItem>
            {productos.map(p => (
              <MenuItem key={p.id} value={p.id}>{p.nombreCompleto}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <TextField
          label="Desde"
          type="date"
          value={filtroDesde}
          onChange={(e) => setFiltroDesde(e.target.value)}
          InputLabelProps={{ shrink: true }}
          onKeyDown={(e) => e.preventDefault()}
          onPaste={(e) => e.preventDefault()}
        />
        <TextField
          label="Hasta"
          type="date"
          value={filtroHasta}
          onChange={(e) => setFiltroHasta(e.target.value)}
          InputLabelProps={{ shrink: true }}
          onKeyDown={(e) => e.preventDefault()}
          onPaste={(e) => e.preventDefault()}
        />
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Fecha</TableCell>
              <TableCell>Producto</TableCell>
              <TableCell>Tipo</TableCell>
              <TableCell align="right">Cantidad</TableCell>
              <TableCell align="right">Anterior</TableCell>
              <TableCell align="right">Nueva</TableCell>
              <TableCell>Motivo/Referencia</TableCell>
              <TableCell>Usuario</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {movimientos.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography sx={{ py: 2 }}>No hay movimientos</Typography>
                </TableCell>
              </TableRow>
            ) : (
              movimientos.map(m => (
                <TableRow key={m.id}>
                  <TableCell>{new Date(m.fechaMovimiento).toLocaleString()}</TableCell>
                  <TableCell>
                    <Typography variant="body2">{m.productoNombre}</Typography>
                    <Typography variant="caption" color="text.secondary">{m.productoSku}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={m.tipoMovimiento} color={getTipoColor(m.tipoMovimiento)} size="small" />
                  </TableCell>
                  <TableCell align="right">{m.cantidad}</TableCell>
                  <TableCell align="right">{m.cantidadAnterior}</TableCell>
                  <TableCell align="right">{m.cantidadNueva}</TableCell>
                  <TableCell>
                    {m.motivo && <div>{m.motivo}</div>}
                    {m.referencia && <div><small>Ref: {m.referencia}</small></div>}
                  </TableCell>
                  <TableCell>{m.usuarioNombre}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default MovimientosList;