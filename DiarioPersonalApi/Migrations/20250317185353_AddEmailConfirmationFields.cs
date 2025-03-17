using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiarioPersonalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Usuarios_UserId",
                table: "Entradas");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Entradas",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Entradas_UserId",
                table: "Entradas",
                newName: "IX_Entradas_UsuarioId");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationTokenExpiry",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas");

            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ConfirmationTokenExpiry",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Entradas",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Entradas_UsuarioId",
                table: "Entradas",
                newName: "IX_Entradas_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Usuarios_UserId",
                table: "Entradas",
                column: "UserId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
