import React from 'react';
import {
  Card,
  CardMedia,
  CardContent,
  CardActions,
  Typography,
  Button,
  Box,
  IconButton,
  Tooltip
} from '@mui/material';
import { AddShoppingCart } from '@mui/icons-material';
import type { Product } from '../../src/types/ecommerce';
import { useAuth } from '../../services/authContext';
import { useNavigate } from 'react-router-dom';
import { cartService } from '../../services/cartService';

interface ProductCardProps {
  product: Product;
  onAddToCart?: () => void;
}

const ProductCard: React.FC<ProductCardProps> = ({ product, onAddToCart }) => {
  const { auth } = useAuth();
  const navigate = useNavigate();

  const handleAddToCart = async () => {
    if (!auth.isAuthenticated) {
      navigate('/login');
      return;
    }
    try {
      await cartService.addToCart({ productoId: product.id, cantidad: 1 });
      if (onAddToCart) onAddToCart();
    } catch (error) {
      console.error('Error adding to cart', error);
    }
  };

  const hasStock = product.stockDisponible > 0;

  return (
    <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Contenedor cuadrado para la imagen */}
      <Box sx={{ position: 'relative', paddingTop: '100%' /* 1:1 Aspect Ratio */ }}>
        <CardMedia
          component="img"
          image={product.imagenPrincipal || '/placeholder.jpg'}
          alt={product.nombre}
          sx={{
            position: 'absolute',
            top: 0,
            left: 0,
            width: '100%',
            height: '100%',
            objectFit: 'cover', // Recorta la imagen para llenar el cuadrado sin deformar
          }}
        />
      </Box>
      <CardContent sx={{ flexGrow: 1 }}>
        <Typography gutterBottom variant="h6" component="h2" noWrap>
          {product.nombre}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {product.categoria}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          display: '-webkit-box',
          WebkitLineClamp: 3,
          WebkitBoxOrient: 'vertical',
        }}>
          {product.descripcion}
        </Typography>
        <Box sx={{ mt: 2 }}>
          <Typography variant="h6" color="primary" fontWeight={600}>
            ${product.precio.toFixed(2)}
          </Typography>
        </Box>
      </CardContent>
      <CardActions sx={{ justifyContent: 'space-between', px: 2, pb: 2 }}>
        <Tooltip title={hasStock ? "Añadir al carrito" : "Producto agotado"}>
          <span>
            <IconButton
              color="primary"
              onClick={handleAddToCart}
              disabled={!hasStock}
            >
              <AddShoppingCart />
            </IconButton>
          </span>
        </Tooltip>
        <Button
          size="small"
          variant="outlined"
          onClick={() => navigate(`/producto/${product.id}`)}
        >
          Ver detalles
        </Button>
      </CardActions>
    </Card>
  );
};

export default ProductCard;