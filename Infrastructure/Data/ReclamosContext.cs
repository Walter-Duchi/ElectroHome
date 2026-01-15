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

    public virtual DbSet<ComprobanteDeReemplazo> ComprobanteDeReemplazos { get; set; }

    public virtual DbSet<ComprobanteProductoReemplazado> ComprobanteProductoReemplazados { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MarcaLoEntregoComoReemplazo> MarcaLoEntregoComoReemplazos { get; set; }

    public virtual DbSet<NumeroSerieProducto> NumeroSerieProductos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

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
        modelBuilder.Entity<ComprobanteDeReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC073721C794");

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
                .HasConstraintName("FK__Comproban__FK_Pe__10566F31");
        });

        modelBuilder.Entity<ComprobanteProductoReemplazado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC07FED65106");

            entity.ToTable("Comprobante_Producto_Reemplazado");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Comproba__0680539CBB4DC5A9").IsUnique();

            entity.HasIndex(e => e.FkProductoDeReemplazo, "UQ__Comproba__A8B53DCF0FB68B07").IsUnique();

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
                .HasConstraintName("FK__Comproban__FK_Pr__17F790F9");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ComprobanteProductoReemplazado)
                .HasForeignKey<ComprobanteProductoReemplazado>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comproban__FK_Re__17036CC0");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC07E0CA75A9");

            entity.HasIndex(e => e.Nombre, "UQ__Marcas__75E3EFCF1BEC36FA").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MarcaLoEntregoComoReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marca_Lo__3214EC075EBFE84F");

            entity.ToTable("Marca_Lo_Entrego_Como_Reemplazo");

            entity.Property(e => e.FkNumeroSerieProductos).HasColumnName("FK_Numero_Serie_Productos");

            entity.HasOne(d => d.FkNumeroSerieProductosNavigation).WithMany(p => p.MarcaLoEntregoComoReemplazos)
                .HasForeignKey(d => d.FkNumeroSerieProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Marca_Lo___FK_Nu__6B24EA82");
        });

        modelBuilder.Entity<NumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Numero_S__3214EC074A1FD25E");

            entity.ToTable("Numero_Serie_Productos");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Numero_S__F7F466E989588230").IsUnique();

            entity.Property(e => e.EstadoInventario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Estado_Inventario");
            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.NumeroSerie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Serie");

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Numero_Se__FK_Pr__6754599E");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07B8BFC258");

            entity.HasIndex(e => new { e.FkMarca, e.Modelo }, "UQ__Producto__F94693C61491D408").IsUnique();

            entity.Property(e => e.DiasGarantia).HasColumnName("Dias_Garantia");
            entity.Property(e => e.Especificacion)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Ma__619B8048");
        });

        modelBuilder.Entity<Reclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC07C3E9EAA3");

            entity.HasIndex(e => e.CodigoReclamo, "UQ__Reclamos__9ECFA1B29963A2F1").IsUnique();

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
                .HasConstraintName("FK__Reclamos__FK_Emp__7A672E12");
        });

        modelBuilder.Entity<ReclamosProductoSn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC079F930497");

            entity.ToTable("Reclamos_Producto_SN");

            entity.HasIndex(e => e.FkNumeroSerieProductos, "UQ__Reclamos__CB80933E1B9CCD4B").IsUnique();

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
                .HasDefaultValueSql("(NULL)")
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
                .HasConstraintName("FK__Reclamos___FK_Nu__7F2BE32F");

            entity.HasOne(d => d.FkReclamosNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkReclamos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Re__00200768");

            entity.HasOne(d => d.FkTecnicoAsignadoNavigation).WithMany(p => p.ReclamosProductoSns)
                .HasForeignKey(d => d.FkTecnicoAsignado)
                .HasConstraintName("FK__Reclamos___FK_Te__05D8E0BE");
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC07C58C13A6");

            entity.ToTable("Reembolso");

            entity.Property(e => e.FechaReembolso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reembolso");
            entity.Property(e => e.NumCuentaBancariaReembolso)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Num_Cuenta_Bancaria_Reembolso");
            entity.Property(e => e.NumeroComprobanteReembolso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Comprobante_Reembolso");
        });

        modelBuilder.Entity<ReembolsoPorReclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC07AF838623");

            entity.ToTable("Reembolso_Por_Reclamos");

            entity.HasIndex(e => e.FkReclamosProductoSn, "UQ__Reembols__0680539C21FEE91D").IsUnique();

            entity.Property(e => e.FkReclamosProductoSn).HasColumnName("FK_Reclamos_Producto_SN");
            entity.Property(e => e.FkReembolso).HasColumnName("FK_Reembolso");

            entity.HasOne(d => d.FkReclamosProductoSnNavigation).WithOne(p => p.ReembolsoPorReclamo)
                .HasForeignKey<ReembolsoPorReclamo>(d => d.FkReclamosProductoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__0C85DE4D");

            entity.HasOne(d => d.FkReembolsoNavigation).WithMany(p => p.ReembolsoPorReclamos)
                .HasForeignKey(d => d.FkReembolso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__0D7A0286");
        });

        modelBuilder.Entity<TokensDeAcceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokensDe__3214EC07973F6330");

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
                .HasConstraintName("FK__TokensDeA__FK_Us__5629CD9C");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC073042AA40");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19D88C2273").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Usuarios__CAF3326BCC6BC990").IsUnique();

            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena).HasMaxLength(256);
            entity.Property(e => e.Convencional)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NumCuentaBancaria)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Num_Cuenta_Bancaria");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ruc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC");
            entity.Property(e => e.TipoCuentaBancaria)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Tipo_Cuenta_Bancaria");
        });

        modelBuilder.Entity<UsuariosCertificacionMarca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07730875C3");

            entity.ToTable("Usuarios_Certificacion_Marcas");

            entity.HasIndex(e => new { e.FkMarca, e.FkTecnico }, "UQ__Usuarios__F20B19AE625EEA73").IsUnique();

            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.FkTecnico).HasColumnName("FK_Tecnico");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Ma__5CD6CB2B");

            entity.HasOne(d => d.FkTecnicoNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Te__5DCAEF64");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC0706A5228B");

            entity.HasIndex(e => e.CodigoFactura, "UQ__Ventas__BB514FC170390E8B").IsUnique();

            entity.Property(e => e.CodigoFactura)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Factura");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Compra");
            entity.Property(e => e.FkEmpresaCliente).HasColumnName("FK_Empresa_Cliente");
            entity.Property(e => e.TotalCompra)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Total_Compra");

            entity.HasOne(d => d.FkEmpresaClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.FkEmpresaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas__FK_Empre__6EF57B66");
        });

        modelBuilder.Entity<VentasPorNumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas_P__3214EC07670E9D66");

            entity.ToTable("Ventas_Por_Numero_Serie_Productos");

            entity.HasIndex(e => e.FkNumeroSerieProducto, "UQ__Ventas_P__8FE47B8B296489AF").IsUnique();

            entity.Property(e => e.FkNumeroSerieProducto).HasColumnName("FK_Numero_Serie_Producto");
            entity.Property(e => e.FkVentas).HasColumnName("FK_Ventas");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Precio_Venta");

            entity.HasOne(d => d.FkNumeroSerieProductoNavigation).WithOne(p => p.VentasPorNumeroSerieProducto)
                .HasForeignKey<VentasPorNumeroSerieProducto>(d => d.FkNumeroSerieProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas_Po__FK_Nu__75A278F5");

            entity.HasOne(d => d.FkVentasNavigation).WithMany(p => p.VentasPorNumeroSerieProductos)
                .HasForeignKey(d => d.FkVentas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ventas_Po__FK_Ve__74AE54BC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
