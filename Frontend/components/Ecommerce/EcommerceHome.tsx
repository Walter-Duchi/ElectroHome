import React, { useState, useEffect } from 'react';
import { Container, Typography, Box, Divider, Tabs, Tab } from '@mui/material';
import EcommerceLayout from './EcommerceLayout';
import ProductList from './ProductList';
import { productService } from '../../services/productService';
import { Product, ProductFilter } from '../../src/types/ecommerce';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`home-tabpanel-${index}`}
      aria-labelledby={`home-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const EcommerceHome: React.FC = () => {
  const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
  const [popularProducts, setPopularProducts] = useState<Product[]>([]);
  const [newArrivals, setNewArrivals] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [tabValue, setTabValue] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<number | undefined>(undefined);

  // Cargar productos iniciales (todos, populares y novedades)
  useEffect(() => {
    loadInitialData();
  }, []);

  // Cargar productos filtrados cuando cambien búsqueda o categoría
  useEffect(() => {
    loadFilteredProducts();
  }, [searchQuery, selectedCategory]);

  const loadInitialData = async () => {
    setLoading(true);
    try {
      const [popular, newArr] = await Promise.all([
        productService.getPopularProducts(),
        productService.getNewArrivals()
      ]);
      setPopularProducts(popular);
      setNewArrivals(newArr);
      // También cargamos los productos sin filtro para la pestaña inicial
      await loadFilteredProducts();
    } catch (error) {
      console.error('Error loading products', error);
    } finally {
      setLoading(false);
    }
  };

  const loadFilteredProducts = async () => {
    setLoading(true);
    try {
      const filter: ProductFilter = {};
      if (searchQuery) filter.busqueda = searchQuery;
      if (selectedCategory) filter.categoriaId = selectedCategory;
      const result = await productService.getProducts(filter);
      setFilteredProducts(result);
    } catch (error) {
      console.error('Error loading filtered products', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (query: string) => {
    setSearchQuery(query);
  };

  const handleCategoryChange = (categoryId?: number) => {
    setSelectedCategory(categoryId);
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleAddToCart = () => {
    // Opcional: mostrar notificación o simplemente actualizar contador (ya se hace en layout)
  };

  return (
    <EcommerceLayout
      onSearch={handleSearch}
      onCategoryChange={handleCategoryChange}
      selectedCategory={selectedCategory}
    >
      <Container maxWidth="xl">
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
          <Tabs value={tabValue} onChange={handleTabChange} aria-label="categorías de productos">
            <Tab label="Todos los productos" />
            <Tab label="Más populares" />
            <Tab label="Novedades" />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          {loading ? (
            <Typography>Cargando productos...</Typography>
          ) : (
            <ProductList products={filteredProducts} onAddToCart={handleAddToCart} />
          )}
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          {loading ? (
            <Typography>Cargando productos populares...</Typography>
          ) : (
            <ProductList products={popularProducts} onAddToCart={handleAddToCart} />
          )}
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          {loading ? (
            <Typography>Cargando novedades...</Typography>
          ) : (
            <ProductList products={newArrivals} onAddToCart={handleAddToCart} />
          )}
        </TabPanel>
      </Container>
    </EcommerceLayout>
  );
};

export default EcommerceHome;
