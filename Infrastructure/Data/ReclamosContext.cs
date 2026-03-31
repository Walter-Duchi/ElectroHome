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

    public virtual DbSet<ConfiguracionSri> ConfiguracionSris { get; set; }

    public virtual DbSet<DatosEmpresa> DatosEmpresas { get; set; }

    public virtual DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }

    public virtual DbSet<InventarioUbicacione> InventarioUbicaciones { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MarcaLoEntregoComoReemplazo> MarcaLoEntregoComoReemplazos { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<NumeroSerieProducto> NumeroSerieProductos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<PayphoneTransaction> PayphoneTransactions { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoImagene> ProductoImagenes { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Reclamo> Reclamos { get; set; }

    public virtual DbSet<ReclamosProductoSn> ReclamosProductoSns { get; set; }

    public virtual DbSet<Reembolso> Reembolsos { get; set; }

    public virtual DbSet<ReembolsoPorReclamo> ReembolsoPorReclamos { get; set; }

    public virtual DbSet<TokensDeAcceso> TokensDeAccesos { get; set; }

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
            entity.HasKey(e => e.Id).HasName("PK__Carrito___3214EC0761084E0A");

            entity.ToTable("Carrito_Compras");

            entity.HasIndex(e => new { e.FkCliente, e.FkProducto }, "UQ__Carrito___D7022F4704F52D7E").IsUnique();

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
                .HasConstraintName("FK__Carrito_C__FK_Cl__662B2B3B");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.CarritoCompras)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito_C__FK_Pr__671F4F74");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07F7883CF0");

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
                .HasConstraintName("FK__Categoria__FK_Ca__59063A47");
        });

        modelBuilder.Entity<ComprobanteDeReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC07528FF8F3");

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
                .HasConstraintName("FK__Comproban__FK_Pe__55F4C372");
        });

        modelBuilder.Entity<ComprobanteProductoReemplazado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC07A67986CA");

            entity.ToTable("Comprobante_Producto_Reemplazado");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Comproba__0680539CEBB85C09").IsUnique();

            entity.HasIndex(e => e.FkProductoDeReemplazo, "UQ__Comproba__A8B53DCF6BE5AFB8").IsUnique();

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
                .HasConstraintName("FK__Comproban__FK_Pr__5D95E53A");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ComprobanteProductoReemplazado)
                .HasForeignKey<ComprobanteProductoReemplazado>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comproban__FK_Re__5CA1C101");
        });

        modelBuilder.Entity<ConfiguracionSri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Configur__3214EC0748931DFC");

            entity.ToTable("Configuracion_SRI");

            entity.Property(e => e.Ambiente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pruebas");
            entity.Property(e => e.FechaExpiracionToken)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Expiracion_Token");
            entity.Property(e => e.TokenAcceso)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Token_Acceso");
        });

        modelBuilder.Entity<DatosEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Datos_Em__3214EC0700AFCA54");

            entity.ToTable("Datos_Empresa");

            entity.Property(e => e.DireccionMatriz)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Direccion_Matriz");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Nombre_Comercial");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Razon_Social");
            entity.Property(e => e.RucEmpresa)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC_Empresa");
        });

        modelBuilder.Entity<InventarioMovimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC07988997BA");

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
                .HasConstraintName("FK__Inventari__FK_Pr__0C85DE4D");

            entity.HasOne(d => d.FkUsuarioNavigation).WithMany(p => p.InventarioMovimientos)
                .HasForeignKey(d => d.FkUsuario)
                .HasConstraintName("FK__Inventari__FK_Us__0D7A0286");
        });

        modelBuilder.Entity<InventarioUbicacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC07A66132A0");

            entity.ToTable("Inventario_Ubicaciones");

            entity.HasIndex(e => e.Codigo, "UQ__Inventar__06370DAC17E762A9").IsUnique();

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
                .HasConstraintName("FK__Inventari__FK_Ub__14270015");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC077522D56B");

            entity.HasIndex(e => e.Nombre, "UQ__Marcas__75E3EFCF856CF278").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MarcaLoEntregoComoReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marca_Lo__3214EC07434F0702");

            entity.ToTable("Marca_Lo_Entrego_Como_Reemplazo");

            entity.Property(e => e.FkNumeroSerieProductos).HasColumnName("FK_Numero_Serie_Productos");

            entity.HasOne(d => d.FkNumeroSerieProductosNavigation).WithMany(p => p.MarcaLoEntregoComoReemplazos)
                .HasForeignKey(d => d.FkNumeroSerieProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Marca_Lo___FK_Nu__1F98B2C1");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Metodos___3214EC0711B09CED");

            entity.ToTable("Metodos_Pago");

            entity.HasIndex(e => e.Tipo, "UQ__Metodos___8E762CB40B8EDAA2").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Numero_S__3214EC077A7A1FA1");

            entity.ToTable("Numero_Serie_Productos");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Numero_S__F7F466E92FCE6C70").IsUnique();

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
                .HasConstraintName("FK__Numero_Se__FK_Pr__18EBB532");

            entity.HasOne(d => d.FkProveedorNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Numero_Se__FK_Pr__1BC821DD");

            entity.HasOne(d => d.FkUbicacionNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkUbicacion)
                .HasConstraintName("FK__Numero_Se__FK_Ub__1CBC4616");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pagos__3214EC07A79E1BB1");

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
                .HasConstraintName("FK__Pagos__FK_Metodo__2FCF1A8A");

            entity.HasOne(d => d.FkVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.FkVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__FK_Venta__2EDAF651");
        });

        modelBuilder.Entity<PayphoneTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payphone__3214EC07924435DA");

            entity.HasIndex(e => e.ClientTransactionId, "UQ__Payphone__93829C425171869F").IsUnique();

            entity.Property(e => e.ClientTransactionId)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.FkUsuarioNavigation).WithMany(p => p.PayphoneTransactions)
                .HasForeignKey(d => d.FkUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayphoneTransactions_Usuario");

            entity.HasOne(d => d.Venta).WithMany(p => p.PayphoneTransactions)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("FK_PayphoneTransactions_Venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC070DCE5BAA");

            entity.HasIndex(e => e.Codigo, "UQ__Producto__06370DAC15E9F113").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__Producto__CA1ECF0D2CA87BE1").IsUnique();

            entity.HasIndex(e => new { e.FkMarca, e.Modelo }, "UQ__Producto__F94693C681883829").IsUnique();

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
                .HasConstraintName("FK__Productos__Cread__08B54D69");

            entity.HasOne(d => d.FkCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkCategoria)
                .HasConstraintName("FK__Productos__FK_Ca__03F0984C");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Ma__02FC7413");

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.ProductoModificadoPorNavigations)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Productos__Modif__09A971A2");
        });

        modelBuilder.Entity<ProductoImagene>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07B0F30B77");

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
                .HasConstraintName("FK__Producto___FK_Pr__6166761E");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proveedo__3214EC07CF7D07FF");

            entity.HasIndex(e => e.Cedula, "UQ__Proveedo__B4ADFE38CF1E316E").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Proveedo__CAF3326BB0433609").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC07281C664C");

            entity.HasIndex(e => e.CodigoReclamo, "UQ__Reclamos__9ECFA1B20ECC050F").IsUnique();

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
                .HasConstraintName("FK__Reclamos__FK_Emp__3E1D39E1");
        });

        modelBuilder.Entity<ReclamosProductoSn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC0790698F46");

            entity.ToTable("Reclamos_Producto_SN");

            entity.HasIndex(e => e.FkNumeroSerieProductos, "UQ__Reclamos__CB80933EE8AB39A4").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.ExplicacionRespuestaTecnico)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("Explicacion_Respuesta_Tecnico");
            entity.Property(e => e.FechaReclamoClienteFinal)
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
                .HasConstraintName("FK__Reclamos___FK_Nu__42E1EEFE");

            entity.HasOne(d => d.FkReclamosNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkReclamos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Re__43D61337");

            entity.HasOne(d => d.FkTecnicoAsignadoNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkTecnicoAsignado)
                .HasConstraintName("FK__Reclamos___FK_Te__47A6A41B");
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC077BBE8DC5");

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
                .HasConstraintName("FK__Reembolso__FK_Me__4B7734FF");

            entity.HasOne(d => d.FkUsuarioAutorizoNavigation).WithMany(p => p.Reembolsos)
                .HasForeignKey(d => d.FkUsuarioAutorizo)
                .HasConstraintName("FK__Reembolso__FK_Us__4E53A1AA");
        });

        modelBuilder.Entity<ReembolsoPorReclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC071CB639A7");

            entity.ToTable("Reembolso_Por_Reclamos");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Reembols__0680539C7FBE3B74").IsUnique();

            entity.Property(e => e.FkReclamosProductoSn).HasColumnName("FK_Reclamos_Producto_SN");
            entity.Property(e => e.FkReembolso).HasColumnName("FK_Reembolso");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ReembolsoPorReclamo)
                .HasForeignKey<ReembolsoPorReclamo>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__5224328E");

            entity.HasOne(d => d.FkReembolsoNavigation).WithMany(p => p.ReembolsoPorReclamos)
                .HasForeignKey(d => d.FkReembolso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__531856C7");
        });

        modelBuilder.Entity<TokensDeAcceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokensDe__3214EC0733E9B901");

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
                .HasConstraintName("FK__TokensDeA__FK_Us__73BA3083");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC079D5B755E");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19727E137F").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Usuarios__CAF3326BA8615BE4").IsUnique();

            entity.HasIndex(e => e.Identificacion, "UQ__Usuarios__D6F931E50E566BE9").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Ciudad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Codigo_Postal");
            entity.Property(e => e.Contrasena).HasMaxLength(256);
            entity.Property(e => e.ContribuyenteEspecial).HasColumnName("Contribuyente_Especial");
            entity.Property(e => e.Convencional)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreadoPor).HasColumnName("Creado_Por");
            entity.Property(e => e.Direccion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DivisionAdministrativa)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Division_administrativa");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
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
            entity.Property(e => e.ObligadoContabilidad).HasColumnName("Obligado_Contabilidad");
            entity.Property(e => e.Pais)
                .HasMaxLength(50)
                .IsUnicode(false);
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

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.InverseCreadoPorNavigation)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__Usuarios__Creado__5535A963");
        });

        modelBuilder.Entity<UsuariosCertificacionMarca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC0733249E08");

            entity.ToTable("Usuarios_Certificacion_Marcas");

            entity.HasIndex(e => new { e.FkMarca, e.FkTecnico }, "UQ__Usuarios__F20B19AE21F543D3").IsUnique();

            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.FkTecnico).HasColumnName("FK_Tecnico");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Ma__7A672E12");

            entity.HasOne(d => d.FkTecnicoNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Te__7B5B524B");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC07E1FC93FE");

            entity.HasIndex(e => e.CodigoFactura, "UQ__Ventas__BB514FC1FFF94B80").IsUnique();

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
            entity.Property(e => e.SriAutorizacion).HasColumnName("SRI_Autorizacion");
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

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.VentaCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__Ventas__Creado_P__2B0A656D");

            entity.HasOne(d => d.FkEmpresaClienteNavigation).WithMany(p => p.VentaFkEmpresaClienteNavigations)
                .HasForeignKey(d => d.FkEmpresaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas__FK_Empre__236943A5");

            entity.HasOne(d => d.FkVendedorNavigation).WithMany(p => p.VentaFkVendedorNavigations)
                .HasForeignKey(d => d.FkVendedor)
                .HasConstraintName("FK__Ventas__FK_Vende__245D67DE");

            entity.HasOne(d => d.ModificadoPorNavigation).WithMany(p => p.VentaModificadoPorNavigations)
                .HasForeignKey(d => d.ModificadoPor)
                .HasConstraintName("FK__Ventas__Modifica__2BFE89A6");
        });

        modelBuilder.Entity<VentasPorNumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas_P__3214EC07645CD9E1");

            entity.ToTable("Ventas_Por_Numero_Serie_Productos");

            entity.HasIndex(e => e.FkNumeroSerieProducto, "UQ__Ventas_P__8FE47B8B1D09ECB6").IsUnique();

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
                .HasConstraintName("FK__Ventas_Po__FK_Nu__3864608B");

            entity.HasOne(d => d.FkVentasNavigation).WithMany(p => p.VentasPorNumeroSerieProductos)
                .HasForeignKey(d => d.FkVentas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas_Po__FK_Ve__37703C52");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
