using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiarioPersonalApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Etiquetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etiquetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContraseñaHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entradas_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntradasEtiquetas",
                columns: table => new
                {
                    EntradaId = table.Column<int>(type: "int", nullable: false),
                    EtiquetaId = table.Column<int>(type: "int", nullable: false),
                    PosicionInicio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasEtiquetas", x => new { x.EntradaId, x.EtiquetaId });
                    table.ForeignKey(
                        name: "FK_EntradasEtiquetas_Entradas_EntradaId",
                        column: x => x.EntradaId,
                        principalTable: "Entradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntradasEtiquetas_Etiquetas_EtiquetaId",
                        column: x => x.EtiquetaId,
                        principalTable: "Etiquetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_UserId",
                table: "Entradas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasEtiquetas_EtiquetaId",
                table: "EntradasEtiquetas",
                column: "EtiquetaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntradasEtiquetas");

            migrationBuilder.DropTable(
                name: "Entradas");

            migrationBuilder.DropTable(
                name: "Etiquetas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
