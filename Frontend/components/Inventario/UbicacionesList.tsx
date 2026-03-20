import React from 'react';
import {
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, IconButton, Chip, Box, Typography
} from '@mui/material';
import { Edit, Delete } from '@mui/icons-material';
import type { Ubicacion } from '../../src/types/inventario';

interface UbicacionesListProps {
  ubicaciones: Ubicacion[];
  onEdit: (ubicacion: Ubicacion) => void;
  onDelete: (id: number) => void;
}

const UbicacionesList: React.FC<UbicacionesListProps> = ({ ubicaciones, onEdit, onDelete }) => {
  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Código</TableCell>
            <TableCell>Nombre</TableCell>
            <TableCell>Tipo</TableCell>
            <TableCell>Ubicación Padre</TableCell>
            <TableCell>Capacidad</TableCell>
            <TableCell>Estado</TableCell>
            <TableCell align="right">Acciones</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {ubicaciones.length === 0 ? (
            <TableRow>
              <TableCell colSpan={7} align="center">
                <Typography sx={{ py: 2 }}>No hay ubicaciones registradas</Typography>
              </TableCell>
            </TableRow>
          ) : (
            ubicaciones.map((u) => (
              <TableRow key={u.id}>
                <TableCell><strong>{u.codigo}</strong></TableCell>
                <TableCell>{u.nombre}</TableCell>
                <TableCell>{u.tipo || '-'}</TableCell>
                <TableCell>{u.ubicacionPadreNombre || '-'}</TableCell>
                <TableCell>{u.capacidadMaxima ?? '-'}</TableCell>
                <TableCell>
                  <Chip
                    label={u.activo ? 'Activo' : 'Inactivo'}
                    color={u.activo ? 'success' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell align="right">
                  <IconButton size="small" onClick={() => onEdit(u)} color="primary">
                    <Edit />
                  </IconButton>
                  <IconButton size="small" onClick={() => onDelete(u.id)} color="error">
                    <Delete />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default UbicacionesList;
