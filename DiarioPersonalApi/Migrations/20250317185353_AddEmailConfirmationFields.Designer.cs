﻿// <auto-generated />
using System;
using DiarioPersonalApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiarioPersonalApi.Migrations
{
    [DbContext(typeof(DiarioDbContext))]
    [Migration("20250317185353_AddEmailConfirmationFields")]
    partial class AddEmailConfirmationFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DiarioPersonalApi.Models.Entrada", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Contenido")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Fecha")
                        .HasColumnType("datetime2");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Entradas");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.EntradaEtiqueta", b =>
                {
                    b.Property<int>("EntradaId")
                        .HasColumnType("int");

                    b.Property<int>("EtiquetaId")
                        .HasColumnType("int");

                    b.Property<int>("PosicionInicio")
                        .HasColumnType("int");

                    b.HasKey("EntradaId", "EtiquetaId");

                    b.HasIndex("EtiquetaId");

                    b.ToTable("EntradasEtiquetas");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Etiqueta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Etiquetas");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ConfirmationToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ConfirmationTokenExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("ContraseñaHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("NombreUsuario")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Entrada", b =>
                {
                    b.HasOne("DiarioPersonalApi.Models.Usuario", "Usuario")
                        .WithMany("Entradas")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.EntradaEtiqueta", b =>
                {
                    b.HasOne("DiarioPersonalApi.Models.Entrada", "Entrada")
                        .WithMany("EntradasEtiquetas")
                        .HasForeignKey("EntradaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DiarioPersonalApi.Models.Etiqueta", "Etiqueta")
                        .WithMany("EntradasEtiquetas")
                        .HasForeignKey("EtiquetaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entrada");

                    b.Navigation("Etiqueta");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Entrada", b =>
                {
                    b.Navigation("EntradasEtiquetas");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Etiqueta", b =>
                {
                    b.Navigation("EntradasEtiquetas");
                });

            modelBuilder.Entity("DiarioPersonalApi.Models.Usuario", b =>
                {
                    b.Navigation("Entradas");
                });
#pragma warning restore 612, 618
        }
    }
}
