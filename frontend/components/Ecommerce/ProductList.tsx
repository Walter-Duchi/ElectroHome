import React from 'react';
import { Grid, Typography, Box } from '@mui/material';
import ProductCard from './ProductCard';
import type { Product } from '../../src/types/ecommerce';

interface ProductListProps {
  products: Product[];
  onAddToCart?: () => void;
}

const ProductList: React.FC<ProductListProps> = ({ products, onAddToCart }) => {
  if (products.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary">
          No se encontraron productos
        </Typography>
      </Box>
    );
  }

  return (
    <Grid container spacing={3}>
      {products.map((product) => (
        <Grid key={product.id} size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
          <ProductCard product={product} onAddToCart={onAddToCart} />
        </Grid>
      ))}
    </Grid>
  );
};

export default ProductList;