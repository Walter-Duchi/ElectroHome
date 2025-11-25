using System;
using System.Collections.Generic;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Data;

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

    public virtual DbSet<DetallesCompra> DetallesCompras { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<NumeroSerieProducto> NumeroSerieProductos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Reclamo> Reclamos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=.;Database=Reclamos;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compras__3214EC0768D4B209");

            entity.HasIndex(e => e.FkCliente, "IX_Compras_Cliente");

            entity.HasIndex(e => e.FechaCompra, "IX_Compras_Fecha");

            entity.HasIndex(e => e.CodigoFactura, "UQ__Compras__BB514FC192BF4785").IsUnique();

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

        modelBuilder.Entity<DetallesCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Detalles__3214EC0716CBF997");

            entity.ToTable("Detalles_Compras");

            entity.HasIndex(e => e.FkCompra, "IX_Detalles_Compra");

            entity.HasIndex(e => e.FkNumeroSerie, "UQ__Detalles__7C523D3538666F3C").IsUnique();

            entity.Property(e => e.FkCompra).HasColumnName("FK_Compra");
            entity.Property(e => e.FkNumeroSerie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FK_Numero_Serie");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Precio_Venta");

            entity.HasOne(d => d.FkCompraNavigation).WithMany(p => p.DetallesCompras)
                .HasForeignKey(d => d.FkCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Detalles___FK_Co__5629CD9C");

            entity.HasOne(d => d.FkNumeroSerieNavigation).WithOne(p => p.DetallesCompra)
                .HasPrincipalKey<NumeroSerieProducto>(p => p.NumeroSerie)
                .HasForeignKey<DetallesCompra>(d => d.FkNumeroSerie)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Detalles___FK_Nu__571DF1D5");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC078CEE5478");

            entity.HasIndex(e => e.Nombre, "UQ__Marcas__75E3EFCF61E2C3E6").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NumeroSerieProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Numero_S__3214EC077E2E083B");

            entity.ToTable("Numero_Serie_Productos");

            entity.HasIndex(e => e.NumeroSerie, "UQ__Numero_S__F7F466E9E7735EE2").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07F537638E");

            entity.HasIndex(e => new { e.FkMarca, e.Modelo }, "UQ__Producto__F94693C65EE58F85").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Reclamos__3214EC07694CA75D");

            entity.HasIndex(e => e.FkClienteFinal, "IX_Reclamos_Cliente");

            entity.HasIndex(e => e.Estado, "IX_Reclamos_Estado");

            entity.HasIndex(e => e.FechaCreacionReclamo, "IX_Reclamos_Fecha");

            entity.HasIndex(e => e.FkTecnicoAsignado, "IX_Reclamos_Tecnico");

            entity.HasIndex(e => e.FkDetalleCompra, "UQ__Reclamos__6DCF170A1D9273E4").IsUnique();

            entity.HasIndex(e => e.CodigoReclamo, "UQ__Reclamos__9ECFA1B2E601F373").IsUnique();

            entity.Property(e => e.CodigoReclamo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Codigo_Reclamo");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.ExplicacionRespuesta)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("Explicacion_Respuesta");
            entity.Property(e => e.FechaCreacionReclamo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion_Reclamo");
            entity.Property(e => e.FechaNotificacionLeida)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Notificacion_Leida");
            entity.Property(e => e.FechaReclamoClienteFinal)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reclamo_Cliente_Final");
            entity.Property(e => e.FechaReembolso)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Reembolso");
            entity.Property(e => e.FechaRespuesta)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Respuesta");
            entity.Property(e => e.FechaVentaClienteFinal)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Venta_Cliente_Final");
            entity.Property(e => e.FkClienteFinal).HasColumnName("FK_Cliente_Final");
            entity.Property(e => e.FkDetalleCompra).HasColumnName("FK_Detalle_Compra");
            entity.Property(e => e.FkTecnicoAsignado).HasColumnName("FK_Tecnico_Asignado");
            entity.Property(e => e.FormaCompensacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Forma_Compensacion");
            entity.Property(e => e.NumeroComprobanteReembolso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Numero_Comprobante_Reembolso");
            entity.Property(e => e.PdfComprobanteEntregaCliente)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PDF_Comprobante_Entrega_Cliente");
            entity.Property(e => e.PdfEvidenciaRevision)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PDF_Evidencia_Revision");

            entity.HasOne(d => d.FkClienteFinalNavigation).WithMany(p => p.ReclamoFkClienteFinalNavigations)
                .HasForeignKey(d => d.FkClienteFinal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos__FK_Cli__5DCAEF64");

            entity.HasOne(d => d.FkDetalleCompraNavigation).WithOne(p => p.Reclamo)
                .HasForeignKey<Reclamo>(d => d.FkDetalleCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reclamos__FK_Det__5CD6CB2B");

            entity.HasOne(d => d.FkTecnicoAsignadoNavigation).WithMany(p => p.ReclamoFkTecnicoAsignadoNavigations)
                .HasForeignKey(d => d.FkTecnicoAsignado)
                .HasConstraintName("FK__Reclamos__FK_Tec__619B8048");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC0751CEDFD2");

            entity.HasIndex(e => e.Correo, "IX_Usuarios_Correo");

            entity.HasIndex(e => e.Rol, "IX_Usuarios_Rol");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A1905568995").IsUnique();

            entity.HasIndex(e => e.Ruc, "UQ__Usuarios__CAF3326BE59ED9CC").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC074BC0CDD6");

            entity.ToTable("Usuarios_Certificacion_Marcas");

            entity.HasIndex(e => new { e.FkMarca, e.FkTecnico }, "UQ__Usuarios__F20B19AE6109054C").IsUnique();

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
