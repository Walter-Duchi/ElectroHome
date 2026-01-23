using System;
using System.Collections.Generic;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class ReclamosContext : DbContext
{
    public ReclamosContext()
    {
    }

    public ReclamosContext(DbContextOptions<ReclamosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CarritoCompra> CarritoCompras { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<ComprobanteDeReemplazo> ComprobanteDeReemplazos { get; set; }

    public virtual DbSet<ComprobanteProductoReemplazado> ComprobanteProductoReemplazados { get; set; }

    public virtual DbSet<ConfiguracionGeneral> ConfiguracionGenerals { get; set; }

    public virtual DbSet<ConfiguracionSri> ConfiguracionSris { get; set; }

    public virtual DbSet<Envio> Envios { get; set; }

    public virtual DbSet<ImpuestoArancelario> ImpuestoArancelarios { get; set; }

    public virtual DbSet<ImpuestosConfiguracion> ImpuestosConfiguracions { get; set; }

    public virtual DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }

    public virtual DbSet<InventarioUbicacione> InventarioUbicaciones { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MarcaLoEntregoComoReemplazo> MarcaLoEntregoComoReemplazos { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<NumeroSerieProducto> NumeroSerieProductos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoImagene> ProductoImagenes { get; set; }

    public virtual DbSet<ProductoImpuesto> ProductoImpuestos { get; set; }

    public virtual DbSet<ProductoPrecioHistorial> ProductoPrecioHistorials { get; set; }

    public virtual DbSet<ProductosPopularesCache> ProductosPopularesCaches { get; set; }

    public virtual DbSet<Promocione> Promociones { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Reclamo> Reclamos { get; set; }

    public virtual DbSet<ReclamosProductoSn> ReclamosProductoSns { get; set; }

    public virtual DbSet<Reembolso> Reembolsos { get; set; }

    public virtual DbSet<ReembolsoPorReclamo> ReembolsoPorReclamos { get; set; }

    public virtual DbSet<ResenasProducto> ResenasProductos { get; set; }

    public virtual DbSet<TarifasEnvio> TarifasEnvios { get; set; }

    public virtual DbSet<TokensDeAcceso> TokensDeAccesos { get; set; }

    public virtual DbSet<UbicacionesGeografica> UbicacionesGeograficas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VentasPorNumeroSerieProducto> VentasPorNumeroSerieProductos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Reclamos;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarritoCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carrito___3214EC0761B1B6DE");

            entity.ToTable("Carrito_Compras");

            entity.HasIndex(e => new { e.FkCliente, e.FkProducto }, "UQ__Carrito___D7022F476A3428BF").IsUnique();

            entity.Property(e => e.Cantidad).HasDefaultValue(1);
            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Agregado");
            entity.Property(e => e.FkCliente).HasColumnName("FK_Cliente");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");

            entity.HasOne(d => d.FkClienteNavigation).WithMany(p => p.CarritoCompras)
                .HasForeignKey(d => d.FkCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito_C__FK_Cl__7EF6D905");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.CarritoCompras)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito_C__FK_Pr__7FEAFD3E");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07B48EC14B");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.FkCategoriaPadre).HasColumnName("FK_Categoria_Padre");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.FkCategoriaPadreNavigation).WithMany(p => p.InverseFkCategoriaPadreNavigation)
                .HasForeignKey(d => d.FkCategoriaPadre)
                .HasConstraintName("FK__Categoria__FK_Ca__5DCAEF64");
        });

        modelBuilder.Entity<ComprobanteDeReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC0768E3DB3E");

            entity.ToTable("Comprobante_De_Reemplazo");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FkPersonalEntrega).HasColumnName("FK_Personal_Entrega");
            entity.Property(e => e.PdfComprobanteEntregaCliente)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PDF_Comprobante_Entrega_Cliente");

            entity.HasOne(d => d.FkPersonalEntregaNavigation).WithMany(p => p.ComprobanteDeReemplazos)
                .HasForeignKey(d => d.FkPersonalEntrega)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comproban__FK_Pe__65370702");
        });

        modelBuilder.Entity<ComprobanteProductoReemplazado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC07706B78BF");

            entity.ToTable("Comprobante_Producto_Reemplazado");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Comproba__0680539CAFF1ADF4").IsUnique();

            entity.HasIndex(e => e.FkProductoDeReemplazo, "UQ__Comproba__A8B53DCF3EE69BD0").IsUnique();

            entity.Property(e => e.FkComprobanteDeReemplazo).HasColumnName("FK_Comprobante_De_Reemplazo");
            entity.Property(e => e.FkProductoDeReemplazo).HasColumnName("FK_Producto_De_Reemplazo");
            entity.Property(e => e.FkReclamosProductoSn).HasColumnName("FK_Reclamos_Producto_SN");

            entity.HasOne(d => d.FkComprobanteDeReemplazoNavigation).WithMany(p => p.ComprobanteProductoReemplazados)
                .HasForeignKey(d => d.FkComprobanteDeReemplazo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Comprobante_Producto_Reemplazado_Comprobante_De_Reemplazo");

            entity.HasOne(d => d.FkProductoDeReemplazoNavigation).WithOne(p => p.ComprobanteProductoReemplazado)
                .HasForeignKey<ComprobanteProductoReemplazado>(d => d.FkProductoDeReemplazo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comproban__FK_Pr__6CD828CA");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ComprobanteProductoReemplazado)
                .HasForeignKey<ComprobanteProductoReemplazado>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comproban__FK_Re__6BE40491");
        });

        modelBuilder.Entity<ConfiguracionGeneral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Configur__3214EC07AB20C815");

            entity.ToTable("Configuracion_General");

            entity.HasIndex(e => e.Clave, "UQ__Configur__E8181E116E592139").IsUnique();

            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Modificacion");
            entity.Property(e => e.ModificadoPor).HasColumnName("Modificado_Por");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Valor)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.ConfiguracionGenerals)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Configura__Modif__7A672E12");
        });

        modelBuilder.Entity<ConfiguracionSri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Configur__3214EC0716780960");

            entity.ToTable("Configuracion_SRI");

            entity.Property(e => e.Ambiente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pruebas");
            entity.Property(e => e.DireccionMatriz)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Direccion_Matriz");
            entity.Property(e => e.Establecimiento)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.FechaExpiracionToken)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Expiracion_Token");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Nombre_Comercial");
            entity.Property(e => e.ObligadoContabilidad)
                .HasDefaultValue(true)
                .HasColumnName("Obligado_Contabilidad");
            entity.Property(e => e.PuntoEmision)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("Punto_Emision");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Razon_Social");
            entity.Property(e => e.RucEmpresa)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC_Empresa");
            entity.Property(e => e.SecuencialFactura)
                .HasDefaultValue(1)
                .HasColumnName("Secuencial_Factura");
            entity.Property(e => e.SecuencialNotaCredito)
                .HasDefaultValue(1)
                .HasColumnName("Secuencial_Nota_Credito");
            entity.Property(e => e.TokenAcceso)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Token_Acceso");
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Envios__3214EC070AA7E5FB");

            entity.Property(e => e.CostoEnvio)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Costo_Envio");
            entity.Property(e => e.DimensionesTotal)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Dimensiones_Total");
            entity.Property(e => e.EstadoEnvio)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Estado_Envio");
            entity.Property(e => e.EvidenciaEntrega)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Evidencia_Entrega");
            entity.Property(e => e.FechaDespacho)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Despacho");
            entity.Property(e => e.FechaEstimadaEntrega)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Estimada_Entrega");
            entity.Property(e => e.FechaRealEntrega)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Real_Entrega");
            entity.Property(e => e.FirmadoPor)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Firmado_Por");
            entity.Property(e => e.FkVenta).HasColumnName("FK_Venta");
            entity.Property(e => e.GuiaRemision)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Guia_Remision");
            entity.Property(e => e.PesoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Peso_Total");
            entity.Property(e => e.Transportista)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.FkVentaNavigation).WithMany(p => p.Envios)
                .HasForeignKey(d => d.FkVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Envios__FK_Venta__0D44F85C");
        });

        modelBuilder.Entity<ImpuestoArancelario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Impuesto__3214EC07FA744B62");

            entity.ToTable("Impuesto_Arancelario");

            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<ImpuestosConfiguracion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Impuesto__3214EC07C41F3E97");

            entity.ToTable("Impuestos_Configuracion");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.AplicableA)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Aplicable_A");
            entity.Property(e => e.CodigoImpuesto)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Codigo_Impuesto");
            entity.Property(e => e.CreadoPor).HasColumnName("Creado_Por");
            entity.Property(e => e.FechaVigenciaFin).HasColumnName("Fecha_Vigencia_Fin");
            entity.Property(e => e.FechaVigenciaInicio).HasColumnName("Fecha_Vigencia_Inicio");
            entity.Property(e => e.ModificadoPor).HasColumnName("Modificado_Por");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.ImpuestosConfiguracionCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__Impuestos__Cread__15DA3E5D");

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.ImpuestosConfiguracionModificadoPorNavigations)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Impuestos__Modif__16CE6296");
        });

        modelBuilder.Entity<InventarioMovimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC0780B3077F");

            entity.ToTable("Inventario_Movimientos");

            entity.Property(e => e.CantidadAnterior).HasColumnName("Cantidad_Anterior");
            entity.Property(e => e.CantidadNueva).HasColumnName("Cantidad_Nueva");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Costo_Unitario");
            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Movimiento");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.FkUsuario).HasColumnName("FK_Usuario");
            entity.Property(e => e.Motivo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Tipo_Movimiento");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.InventarioMovimientos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventari__FK_Pr__1AD3FDA4");

            entity.HasOne(d => d.FkUsuarioNavigation).WithMany(p => p.InventarioMovimientos)
                .HasForeignKey(d => d.FkUsuario)
                .HasConstraintName("FK__Inventari__FK_Us__1BC821DD");
        });

        modelBuilder.Entity<InventarioUbicacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC079082BD2C");

            entity.ToTable("Inventario_Ubicaciones");

            entity.HasIndex(e => e.Codigo, "UQ__Inventar__06370DAC146F1A24").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CapacidadMaxima).HasColumnName("Capacidad_Maxima");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FkUbicacionPadre).HasColumnName("FK_Ubicacion_Padre");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.FkUbicacionPadreNavigation).WithMany(p => p.InverseFkUbicacionPadreNavigation)
                .HasForeignKey(d => d.FkUbicacionPadre)
                .HasConstraintName("FK__Inventari__FK_Ub__22751F6C");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC0740A1E372");

            entity.HasIndex(e => e.Nombre, "UQ__Marcas__75E3EFCFAF63CAF3").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MarcaLoEntregoComoReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marca_Lo__3214EC078B5388B4");

            entity.ToTable("Marca_Lo_Entrego_Como_Reemplazo");

            entity.Property(e => e.FkNumeroSerieProductos).HasColumnName("FK_Numero_Serie_Productos");

            entity.HasOne(d => d.FkNumeroSerieProductosNavigation).WithMany(p => p.MarcaLoEntregoComoReemplazos)
                .HasForeignKey(d => d.FkNumeroSerieProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Marca_Lo___FK_Nu__2DE6D218");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Metodos___3214EC0754FE6BDF");

            entity.ToTable("Metodos_Pago");

            entity.HasIndex(e => e.Tipo, "UQ__Metodos___8E762CB45CE1FD9A").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.ComisionPorcentaje)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Comision_Porcentaje");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RequiereConfirmacion)
                .HasDefaultValue(false)
                .HasColumnName("Requiere_Confirmacion");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Numero_S__3214EC076145EC9A");

            entity.ToTable("Numero_Serie_Productos");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Numero_S__F7F466E9A8E54782").IsUnique();

            entity.Property(e => e.EstadoInventario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Estado_Inventario");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Ingreso");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.FkProveedor).HasColumnName("FK_Proveedor");
            entity.Property(e => e.FkUbicacion).HasColumnName("FK_Ubicacion");
            entity.Property(e => e.NumeroSerie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Serie");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Numero_Se__FK_Pr__2739D489");

            entity.HasOne(d => d.FkProveedorNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Numero_Se__FK_Pr__2A164134");

            entity.HasOne(d => d.FkUbicacionNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkUbicacion)
                .HasConstraintName("FK__Numero_Se__FK_Ub__2B0A656D");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pagos__3214EC072C1FECDF");

            entity.Property(e => e.Cuotas).HasDefaultValue(1);
            entity.Property(e => e.DatosTransaccion)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("Datos_Transaccion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Pago");
            entity.Property(e => e.FkMetodoPago).HasColumnName("FK_Metodo_Pago");
            entity.Property(e => e.FkVenta).HasColumnName("FK_Venta");
            entity.Property(e => e.Monto).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MontoCuota)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Monto_Cuota");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TerminalPuntoVenta)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Terminal_PuntoVenta");

            entity.HasOne(d => d.FkMetodoPagoNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.FkMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__FK_Metodo__3E1D39E1");

            entity.HasOne(d => d.FkVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.FkVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__FK_Venta__3D2915A8");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC0760475489");

            entity.HasIndex(e => e.Codigo, "UQ__Producto__06370DACA9E40569").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__Producto__CA1ECF0D0CD70079").IsUnique();

            entity.HasIndex(e => new { e.FkMarca, e.Modelo }, "UQ__Producto__F94693C661EE954B").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.AltoCm)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("Alto_cm");
            entity.Property(e => e.AnchoCm)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("Ancho_cm");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreadoPor).HasColumnName("Creado_Por");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.DiasGarantia).HasColumnName("Dias_Garantia");
            entity.Property(e => e.Especificacion).IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.FkCategoria).HasColumnName("FK_Categoria");
            entity.Property(e => e.FkImpuestoArancelario).HasColumnName("FK_Impuesto_Arancelario");
            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Imagen_URL");
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModificadoPor).HasColumnName("Modificado_Por");
            entity.Property(e => e.PesoKg)
                .HasColumnType("decimal(12, 4)")
                .HasColumnName("Peso_kg");
            entity.Property(e => e.Precio).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProfundidadCm)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("Profundidad_cm");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Visibilidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Publico");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.ProductoCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__Productos__Cread__17036CC0");

            entity.HasOne(d => d.FkCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkCategoria)
                .HasConstraintName("FK__Productos__FK_Ca__114A936A");

            entity.HasOne(d => d.FkImpuestoArancelarioNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkImpuestoArancelario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Im__14270015");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Ma__10566F31");

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.ProductoModificadoPorNavigations)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Productos__Modif__17F790F9");
        });

        modelBuilder.Entity<ProductoImagene>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07AE81D59E");

            entity.ToTable("Producto_Imagenes");

            entity.Property(e => e.EsPrincipal)
                .HasDefaultValue(false)
                .HasColumnName("Es_Principal");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.UrlImagen)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("URL_Imagen");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.ProductoImagenes)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___FK_Pr__756D6ECB");
        });

        modelBuilder.Entity<ProductoImpuesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC0797FD1BB5");

            entity.ToTable("Producto_Impuestos");

            entity.HasIndex(e => new { e.FkProducto, e.FkImpuesto }, "UQ__Producto__24931C2ADA9C9311").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaVigencia).HasColumnName("Fecha_Vigencia");
            entity.Property(e => e.FkImpuesto).HasColumnName("FK_Impuesto");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.PorcentajeAplicado)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Porcentaje_Aplicado");

            entity.HasOne(d => d.FkImpuestoNavigation).WithMany(p => p.ProductoImpuestos)
                .HasForeignKey(d => d.FkImpuesto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___FK_Im__1B9317B3");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.ProductoImpuestos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___FK_Pr__1A9EF37A");
        });

        modelBuilder.Entity<ProductoPrecioHistorial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07C76E57CE");

            entity.ToTable("Producto_Precio_Historial");

            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Cambio");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.FkUsuario).HasColumnName("FK_Usuario");
            entity.Property(e => e.Motivo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PrecioAnterior)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Precio_Anterior");
            entity.Property(e => e.PrecioNuevo)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Precio_Nuevo");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.ProductoPrecioHistorials)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___FK_Pr__70A8B9AE");

            entity.HasOne(d => d.FkUsuarioNavigation).WithMany(p => p.ProductoPrecioHistorials)
                .HasForeignKey(d => d.FkUsuario)
                .HasConstraintName("FK__Producto___FK_Us__719CDDE7");
        });

        modelBuilder.Entity<ProductosPopularesCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07D58D0EF4");

            entity.ToTable("Productos_Populares_Cache");

            entity.HasIndex(e => e.Posicion, "IX_Productos_Populares_Posicion");

            entity.HasIndex(e => e.FkProducto, "UQ__Producto__311429FC3192511E").IsUnique();

            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Actualizacion");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.RatioConversion)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Ratio_Conversion");
            entity.Property(e => e.VentasUltimos30Dias)
                .HasDefaultValue(0)
                .HasColumnName("Ventas_Ultimos_30_Dias");
            entity.Property(e => e.VistasUltimos30Dias)
                .HasDefaultValue(0)
                .HasColumnName("Vistas_Ultimos_30_Dias");

            entity.HasOne(d => d.FkProductoNavigation).WithOne(p => p.ProductosPopularesCache)
                .HasForeignKey<ProductosPopularesCache>(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Pr__2057CCD0");
        });

        modelBuilder.Entity<Promocione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promocio__3214EC07F0B793D5");

            entity.HasIndex(e => e.Codigo, "UQ__Promocio__06370DACC449C02D").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Inicio");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Valor).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proveedo__3214EC07DAB9EC7F");

            entity.HasIndex(e => e.Cedula, "UQ__Proveedo__B4ADFE38B9FC4A84").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Proveedo__CAF3326B697CE7DA").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Cedula)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ContactoPrincipal)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Contacto_Principal");
            entity.Property(e => e.Direccion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PlazoEntregaDias)
                .HasDefaultValue(7)
                .HasColumnName("Plazo_Entrega_Dias");
            entity.Property(e => e.Ruc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Reclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC07A98B2A0F");

            entity.HasIndex(e => e.CodigoReclamo, "UQ__Reclamos__9ECFA1B2FF7A6A9A").IsUnique();

            entity.Property(e => e.CodigoReclamo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Reclamo");
            entity.Property(e => e.FechaCreacionReclamo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion_Reclamo");
            entity.Property(e => e.FkEmpresaCliente).HasColumnName("FK_Empresa_Cliente");

            entity.HasOne(d => d.FkEmpresaClienteNavigation).WithMany(p => p.Reclamos)
                .HasForeignKey(d => d.FkEmpresaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos__FK_Emp__4C6B5938");
        });

        modelBuilder.Entity<ReclamosProductoSn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC07AC7B9B66");

            entity.ToTable("Reclamos_Producto_SN");

            entity.HasIndex(e => e.FkNumeroSerieProductos, "UQ__Reclamos__CB80933EF494C974").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.ExplicacionRespuestaTecnico)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("Explicacion_Respuesta_Tecnico");
            entity.Property(e => e.FechaReclamoClienteFinal)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reclamo_Cliente_Final");
            entity.Property(e => e.FechaRevisionTecnico)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Revision_Tecnico");
            entity.Property(e => e.FechaVentaClienteFinal)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Venta_Cliente_Final");
            entity.Property(e => e.FkNumeroSerieProductos).HasColumnName("FK_Numero_Serie_Productos");
            entity.Property(e => e.FkReclamos).HasColumnName("FK_Reclamos");
            entity.Property(e => e.FkTecnicoAsignado).HasColumnName("FK_Tecnico_Asignado");
            entity.Property(e => e.FormaCompensacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Forma_Compensacion");
            entity.Property(e => e.PdfRevisionTecnico)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PDF_Revision_Tecnico");

            entity.HasOne(d => d.FkNumeroSerieProductosNavigation).WithOne(p => p.ReclamosProductoSn)
                .HasForeignKey<ReclamosProductoSn>(d => d.FkNumeroSerieProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Nu__51300E55");

            entity.HasOne(d => d.FkReclamosNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkReclamos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Re__5224328E");

            entity.HasOne(d => d.FkTecnicoAsignadoNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkTecnicoAsignado)
                .HasConstraintName("FK__Reclamos___FK_Te__56E8E7AB");
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC0762C453BA");

            entity.ToTable("Reembolso");

            entity.Property(e => e.ComprobantePago)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Comprobante_Pago");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Procesando");
            entity.Property(e => e.FechaAutorizacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Autorizacion");
            entity.Property(e => e.FechaReembolso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reembolso");
            entity.Property(e => e.FkMetodoPago).HasColumnName("FK_Metodo_Pago");
            entity.Property(e => e.FkUsuarioAutorizo).HasColumnName("FK_Usuario_Autorizo");
            entity.Property(e => e.NumCuentaBancariaReembolso)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Num_Cuenta_Bancaria_Reembolso");
            entity.Property(e => e.NumeroComprobanteReembolso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Comprobante_Reembolso");
            entity.Property(e => e.ReferenciaBancaria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Referencia_Bancaria");

            entity.HasOne(d => d.FkMetodoPagoNavigation).WithMany(p => p.Reembolsos)
                .HasForeignKey(d => d.FkMetodoPago)
                .HasConstraintName("FK__Reembolso__FK_Me__5AB9788F");

            entity.HasOne(d => d.FkUsuarioAutorizoNavigation).WithMany(p => p.Reembolsos)
                .HasForeignKey(d => d.FkUsuarioAutorizo)
                .HasConstraintName("FK__Reembolso__FK_Us__5D95E53A");
        });

        modelBuilder.Entity<ReembolsoPorReclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC0777D29AC8");

            entity.ToTable("Reembolso_Por_Reclamos");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Reembols__0680539C3F71D1D0").IsUnique();

            entity.Property(e => e.FkReclamosProductoSn).HasColumnName("FK_Reclamos_Producto_SN");
            entity.Property(e => e.FkReembolso).HasColumnName("FK_Reembolso");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ReembolsoPorReclamo)
                .HasForeignKey<ReembolsoPorReclamo>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__6166761E");

            entity.HasOne(d => d.FkReembolsoNavigation).WithMany(p => p.ReembolsoPorReclamos)
                .HasForeignKey(d => d.FkReembolso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__625A9A57");
        });

        modelBuilder.Entity<ResenasProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resenas___3214EC07E3B9D248");

            entity.ToTable("Resenas_Productos");

            entity.HasIndex(e => new { e.FkCliente, e.FkProducto }, "UQ__Resenas___D7022F47932CBA20").IsUnique();

            entity.Property(e => e.Comentario)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaResena)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Resena");
            entity.Property(e => e.FkCliente).HasColumnName("FK_Cliente");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");

            entity.HasOne(d => d.FkClienteNavigation).WithMany(p => p.ResenasProductos)
                .HasForeignKey(d => d.FkCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Resenas_P__FK_Cl__05A3D694");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.ResenasProductos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Resenas_P__FK_Pr__0697FACD");
        });

        modelBuilder.Entity<TarifasEnvio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tarifas___3214EC0787AACA1F");

            entity.ToTable("Tarifas_Envio");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Actualizacion");
            entity.Property(e => e.FkTransportista).HasColumnName("FK_Transportista");
            entity.Property(e => e.PesoMaximo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Peso_Maximo");
            entity.Property(e => e.PesoMinimo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Peso_Minimo");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TiempoEntregaDias).HasColumnName("Tiempo_Entrega_Dias");
            entity.Property(e => e.Zona)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TokensDeAcceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokensDe__3214EC07737A9160");

            entity.ToTable("TokensDeAcceso");

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FkUsuario).HasColumnName("FK_Usuario");
            entity.Property(e => e.TipoToken)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ResetPassword")
                .HasColumnName("Tipo_Token");
            entity.Property(e => e.Token)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Vigente).HasDefaultValue(true);

            entity.HasOne(d => d.FkUsuarioNavigation).WithMany(p => p.TokensDeAccesos)
                .HasForeignKey(d => d.FkUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TokensDeA__FK_Us__01142BA1");
        });

        modelBuilder.Entity<UbicacionesGeografica>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ubicacio__3214EC07130FEAD7");

            entity.ToTable("Ubicaciones_Geograficas");

            entity.HasIndex(e => e.Codigo, "UQ__Ubicacio__06370DAC48942E6D").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Codigo_Postal");
            entity.Property(e => e.FkPadre).HasColumnName("FK_Padre");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ZonaHoraria)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Zona_Horaria");

            entity.HasOne(d => d.FkPadreNavigation).WithMany(p => p.InverseFkPadreNavigation)
                .HasForeignKey(d => d.FkPadre)
                .HasConstraintName("FK__Ubicacion__FK_Pa__4CA06362");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC078DA0C409");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A195C65D093").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Usuarios__CAF3326B6CB35965").IsUnique();

            entity.HasIndex(e => e.Identificacion, "UQ__Usuarios__D6F931E5B42E0F31").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena).HasMaxLength(256);
            entity.Property(e => e.ContribuyenteEspecial)
                .HasDefaultValue(false)
                .HasColumnName("Contribuyente_Especial");
            entity.Property(e => e.Convencional)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.FkUbicacion).HasColumnName("FK_Ubicacion");
            entity.Property(e => e.Identificacion)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NumCuentaBancaria)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Num_Cuenta_Bancaria");
            entity.Property(e => e.ObligadoContabilidad)
                .HasDefaultValue(false)
                .HasColumnName("Obligado_Contabilidad");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Razon_Social");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ruc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC");
            entity.Property(e => e.TipoCuentaBancaria)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Tipo_Cuenta_Bancaria");
            entity.Property(e => e.TipoIdentificacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Tipo_Identificacion");

            entity.HasOne(d => d.FkUbicacionNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.FkUbicacion)
                .HasConstraintName("FK__Usuarios__FK_Ubi__59FA5E80");
        });

        modelBuilder.Entity<UsuariosCertificacionMarca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07EBB14A07");

            entity.ToTable("Usuarios_Certificacion_Marcas");

            entity.HasIndex(e => new { e.FkMarca, e.FkTecnico }, "UQ__Usuarios__F20B19AE5A004381").IsUnique();

            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.FkTecnico).HasColumnName("FK_Tecnico");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Ma__07C12930");

            entity.HasOne(d => d.FkTecnicoNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Te__08B54D69");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC07C1E65951");

            entity.HasIndex(e => e.CodigoFactura, "UQ__Ventas__BB514FC1B06041B5").IsUnique();

            entity.Property(e => e.ClaveAcceso)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Clave_Acceso");
            entity.Property(e => e.CodigoFactura)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Factura");
            entity.Property(e => e.CreadoPor).HasColumnName("Creado_Por");
            entity.Property(e => e.DireccionEntrega)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Direccion_Entrega");
            entity.Property(e => e.EstadoSri)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente")
                .HasColumnName("Estado_SRI");
            entity.Property(e => e.FechaAutorizacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Autorizacion");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Compra");
            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Modificacion");
            entity.Property(e => e.FkEmpresaCliente).HasColumnName("FK_Empresa_Cliente");
            entity.Property(e => e.FkVendedor).HasColumnName("FK_Vendedor");
            entity.Property(e => e.ModificadoPor).HasColumnName("Modificado_Por");
            entity.Property(e => e.NumeroAutorizacion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Numero_Autorizacion");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.PdfPath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("PDF_Path");
            entity.Property(e => e.TelefonoContacto)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("Telefono_Contacto");
            entity.Property(e => e.TipoVenta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Contado")
                .HasColumnName("Tipo_Venta");
            entity.Property(e => e.TotalCompra)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Total_Compra");
            entity.Property(e => e.XmlPath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("XML_Path");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.VentaCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__Ventas__Creado_P__395884C4");

            entity.HasOne(d => d.FkEmpresaClienteNavigation).WithMany(p => p.VentaFkEmpresaClienteNavigations)
                .HasForeignKey(d => d.FkEmpresaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas__FK_Empre__31B762FC");

            entity.HasOne(d => d.FkVendedorNavigation).WithMany(p => p.VentaFkVendedorNavigations)
                .HasForeignKey(d => d.FkVendedor)
                .HasConstraintName("FK__Ventas__FK_Vende__32AB8735");

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.VentaModificadoPorNavigations)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Ventas__Modifica__3A4CA8FD");
        });

        modelBuilder.Entity<VentasPorNumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas_P__3214EC07871F5C2B");

            entity.ToTable("Ventas_Por_Numero_Serie_Productos");

            entity.HasIndex(e => e.FkNumeroSerieProducto, "UQ__Ventas_P__8FE47B8BB51CF326").IsUnique();

            entity.Property(e => e.Descuento)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.FkNumeroSerieProducto).HasColumnName("FK_Numero_Serie_Producto");
            entity.Property(e => e.FkVentas).HasColumnName("FK_Ventas");
            entity.Property(e => e.Iva)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("IVA");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Precio_Venta");

            entity.HasOne(d => d.FkNumeroSerieProductoNavigation).WithOne(p => p.VentasPorNumeroSerieProducto)
                .HasForeignKey<VentasPorNumeroSerieProducto>(d => d.FkNumeroSerieProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas_Po__FK_Nu__46B27FE2");

            entity.HasOne(d => d.FkVentasNavigation).WithMany(p => p.VentasPorNumeroSerieProductos)
                .HasForeignKey(d => d.FkVentas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas_Po__FK_Ve__45BE5BA9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
