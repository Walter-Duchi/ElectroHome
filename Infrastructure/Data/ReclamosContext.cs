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

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<ComprasProducto> ComprasProductos { get; set; }

    public virtual DbSet<EvidenciaReemplazo> EvidenciaReemplazos { get; set; }

    public virtual DbSet<EvidenciaReemplazoReclamosProducto> EvidenciaReemplazoReclamosProductos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<NumeroSerieProducto> NumeroSerieProductos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Reclamo> Reclamos { get; set; }

    public virtual DbSet<ReclamosProducto> ReclamosProductos { get; set; }

    public virtual DbSet<Reembolso> Reembolsos { get; set; }

    public virtual DbSet<ReembolsoReclamosProducto> ReembolsoReclamosProductos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=Reclamos;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compras__3214EC07373B512B");

            entity.HasIndex(e => e.CodigoFactura, "UQ__Compras__BB514FC1D5DDB6F5").IsUnique();

            entity.Property(e => e.CodigoFactura)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Factura");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Compra");
            entity.Property(e => e.FkCliente).HasColumnName("FK_Cliente");
            entity.Property(e => e.TotalCompra)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Total_Compra");

            entity.HasOne(d => d.FkClienteNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.FkCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compras__FK_Clie__5070F446");
        });

        modelBuilder.Entity<ComprasProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compras___3214EC079796A79A");

            entity.ToTable("Compras_Productos");

            entity.HasIndex(e => e.FkNumeroSerie, "UQ__Compras___7C523D35EEE4E72B").IsUnique();

            entity.Property(e => e.FkCompra).HasColumnName("FK_Compra");
            entity.Property(e => e.FkNumeroSerie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FK_Numero_Serie");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Precio_Venta");

            entity.HasOne(d => d.FkCompraNavigation).WithMany(p => p.ComprasProductos)
                .HasForeignKey(d => d.FkCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compras_P__FK_Co__5629CD9C");

            entity.HasOne(d => d.FkNumeroSerieNavigation).WithOne(p => p.ComprasProducto)
                .HasPrincipalKey<NumeroSerieProducto>(p => p.NumeroSerie)
                .HasForeignKey<ComprasProducto>(d => d.FkNumeroSerie)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compras_P__FK_Nu__571DF1D5");
        });

        modelBuilder.Entity<EvidenciaReemplazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evidenci__3214EC07B13AAB10");

            entity.ToTable("Evidencia_Reemplazo");

            entity.Property(e => e.PdfComprobanteEntregaCliente)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PDF_Comprobante_Entrega_Cliente");
        });

        modelBuilder.Entity<EvidenciaReemplazoReclamosProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evidenci__3214EC07616B21EF");

            entity.ToTable("Evidencia_Reemplazo_Reclamos_Productos");

            entity.HasIndex(e => e.FkReclamosProductos, "UQ__Evidenci__E9B8404D96308DD7").IsUnique();

            entity.Property(e => e.FkEvidenciaReemplazo).HasColumnName("FK_Evidencia_Reemplazo");
            entity.Property(e => e.FkReclamosProductos).HasColumnName("FK_Reclamos_Productos");

            entity.HasOne(d => d.FkEvidenciaReemplazoNavigation).WithMany(p => p.EvidenciaReemplazoReclamosProductos)
                .HasForeignKey(d => d.FkEvidenciaReemplazo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evidencia__FK_Ev__74AE54BC");

            entity.HasOne(d => d.FkReclamosProductosNavigation).WithOne(p => p.EvidenciaReemplazoReclamosProducto)
                .HasForeignKey<EvidenciaReemplazoReclamosProducto>(d => d.FkReclamosProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evidencia__FK_Re__73BA3083");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC07A4D4F04C");

            entity.HasIndex(e => e.Nombre, "UQ__Marcas__75E3EFCFE6AF12BA").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Numero_S__3214EC07551536A5");

            entity.ToTable("Numero_Serie_Productos");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Numero_S__F7F466E962736F9B").IsUnique();

            entity.Property(e => e.FkProducto).HasColumnName("FK_Producto");
            entity.Property(e => e.NumeroSerie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Serie");
            entity.Property(e => e.Vendido).HasDefaultValue(false);

            entity.HasOne(d => d.FkProductoNavigation).WithMany(p => p.NumeroSerieProductos)
                .HasForeignKey(d => d.FkProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Numero_Se__FK_Pr__4BAC3F29");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07953FDB5C");

            entity.HasIndex(e => new { e.FkMarca, e.Modelo }, "UQ__Producto__F94693C6973742A8").IsUnique();

            entity.Property(e => e.DiasGarantia).HasColumnName("Dias_Garantia");
            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__FK_Ma__45F365D3");
        });

        modelBuilder.Entity<Reclamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC078CEC54E6");

            entity.HasIndex(e => e.CodigoReclamo, "UQ__Reclamos__9ECFA1B2377DBF4B").IsUnique();

            entity.Property(e => e.CodigoReclamo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Reclamo");
            entity.Property(e => e.EmpresaCliente).HasColumnName("Empresa_Cliente");
            entity.Property(e => e.FechaCreacionReclamo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion_Reclamo");

            entity.HasOne(d => d.EmpresaClienteNavigation).WithMany(p => p.Reclamos)
                .HasForeignKey(d => d.EmpresaCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos__Empres__5BE2A6F2");
        });

        modelBuilder.Entity<ReclamosProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC079185219F");

            entity.ToTable("Reclamos_Productos");

            entity.HasIndex(e => e.FkComprasProductos, "UQ__Reclamos__CD22BE477E823F7E").IsUnique();

            entity.HasIndex(e => e.FkReclamos, "UQ__Reclamos__FC9A0828DC40AD0A").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.ExplicacionRespuestaTecnico)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("Explicacion_Respuesta_Tecnico");
            entity.Property(e => e.FechaNotificacionLeida)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Notificacion_Leida");
            entity.Property(e => e.FechaRevisionTecnico)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Revision_Tecnico");
            entity.Property(e => e.FechaVentaClienteFinal)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Venta_Cliente_Final");
            entity.Property(e => e.FkComprasProductos).HasColumnName("FK_Compras_Productos");
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

            entity.HasOne(d => d.FkComprasProductosNavigation).WithOne(p => p.ReclamosProducto)
                .HasForeignKey<ReclamosProducto>(d => d.FkComprasProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Co__619B8048");

            entity.HasOne(d => d.FkReclamosNavigation).WithOne(p => p.ReclamosProducto)
                .HasForeignKey<ReclamosProducto>(d => d.FkReclamos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos___FK_Re__628FA481");

            entity.HasOne(d => d.FkTecnicoAsignadoNavigation).WithMany(p => p.ReclamosProductos)
                .HasForeignKey(d => d.FkTecnicoAsignado)
                .HasConstraintName("FK__Reclamos___FK_Te__656C112C");
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC07DFD3E5CD");

            entity.ToTable("Reembolso");

            entity.Property(e => e.FechaReembolso)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reembolso");
            entity.Property(e => e.NumeroComprobanteReembolso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Comprobante_Reembolso");
        });

        modelBuilder.Entity<ReembolsoReclamosProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reembols__3214EC07B76E2098");

            entity.ToTable("Reembolso_Reclamos_Productos");

            entity.HasIndex(e => new { e.FkReclamosProductos, e.FkReembolso }, "UQ__Reembols__85C90861B4F58557").IsUnique();

            entity.HasIndex(e => e.FkReclamosProductos, "UQ__Reembols__E9B8404D61D09FB0").IsUnique();

            entity.Property(e => e.FkReclamosProductos).HasColumnName("FK_Reclamos_Productos");
            entity.Property(e => e.FkReembolso).HasColumnName("FK_Reembolso");

            entity.HasOne(d => d.FkReclamosProductosNavigation).WithOne(p => p.ReembolsoReclamosProducto)
                .HasForeignKey<ReembolsoReclamosProducto>(d => d.FkReclamosProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__6D0D32F4");

            entity.HasOne(d => d.FkReembolsoNavigation).WithMany(p => p.ReembolsoReclamosProductos)
                .HasForeignKey(d => d.FkReembolso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reembolso__FK_Re__6E01572D");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07A74DCD8B");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19BAFA74D0").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Usuarios__CAF3326B31259340").IsUnique();

            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena).HasMaxLength(64);
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
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Num_Cuenta_Bancaria");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ruc)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("RUC");
        });

        modelBuilder.Entity<UsuariosCertificacionMarca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07C94A3713");

            entity.ToTable("Usuarios_Certificacion_Marcas");

            entity.HasIndex(e => new { e.FkMarca, e.FkTecnico }, "UQ__Usuarios__F20B19AEF3460180").IsUnique();

            entity.Property(e => e.FkMarca).HasColumnName("FK_Marca");
            entity.Property(e => e.FkTecnico).HasColumnName("FK_Tecnico");

            entity.HasOne(d => d.FkMarcaNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Ma__412EB0B6");

            entity.HasOne(d => d.FkTecnicoNavigation).WithMany(p => p.UsuariosCertificacionMarcas)
                .HasForeignKey(d => d.FkTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios___FK_Te__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
