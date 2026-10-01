using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppForSEII.API.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Surname",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApellidosUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<int>(type: "int", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DatosPago = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClienteId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_AspNetUsers_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pistas",
                columns: table => new
                {
                    IdPista = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePista = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NPersonas = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pistas", x => x.IdPista);
                });

            migrationBuilder.CreateTable(
                name: "TipoDeporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDeporte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaseDeportiva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Monitor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlazasDisponibles = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    TipoDeporteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaseDeportiva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaseDeportiva_TipoDeporte_TipoDeporteId",
                        column: x => x.TipoDeporteId,
                        principalTable: "TipoDeporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Competicion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Plazas = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TipoDeporteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competicion_TipoDeporte_TipoDeporteId",
                        column: x => x.TipoDeporteId,
                        principalTable: "TipoDeporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaseInscrita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaseDeportivaId = table.Column<int>(type: "int", nullable: false),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PlazasReservadas = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaseInscrita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaseInscrita_ClaseDeportiva_ClaseDeportivaId",
                        column: x => x.ClaseDeportivaId,
                        principalTable: "ClaseDeportiva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaseInscrita_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompeticionInscrita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompeticionId = table.Column<int>(type: "int", nullable: false),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    ProblemasFisicos = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompeticionInscrita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompeticionInscrita_Competicion_CompeticionId",
                        column: x => x.CompeticionId,
                        principalTable: "Competicion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompeticionInscrita_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaseDeportiva_TipoDeporteId",
                table: "ClaseDeportiva",
                column: "TipoDeporteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaseInscrita_ClaseDeportivaId",
                table: "ClaseInscrita",
                column: "ClaseDeportivaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaseInscrita_InscripcionId",
                table: "ClaseInscrita",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Competicion_TipoDeporteId",
                table: "Competicion",
                column: "TipoDeporteId");

            migrationBuilder.CreateIndex(
                name: "IX_CompeticionInscrita_CompeticionId",
                table: "CompeticionInscrita",
                column: "CompeticionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompeticionInscrita_InscripcionId",
                table: "CompeticionInscrita",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ClienteId",
                table: "Inscripciones",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaseInscrita");

            migrationBuilder.DropTable(
                name: "CompeticionInscrita");

            migrationBuilder.DropTable(
                name: "Pistas");

            migrationBuilder.DropTable(
                name: "ClaseDeportiva");

            migrationBuilder.DropTable(
                name: "Competicion");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "TipoDeporte");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Surname",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
