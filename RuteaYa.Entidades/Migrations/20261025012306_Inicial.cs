using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuteaYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Envios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    OrigenDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigenLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OrigenLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DestinoDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinoLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DestinoLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Tamano = table.Column<byte>(type: "tinyint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Envios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Viajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    OrigenDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigenLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OrigenLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DestinoDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinoLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DestinoLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    HoraSalida = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraLlegada = table.Column<TimeOnly>(type: "time", nullable: false),
                    MaxPaquetes = table.Column<int>(type: "int", nullable: false),
                    PesoMaxKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DesvioMaxKm = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DistanciaKm = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DuracionMin = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Viajes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvioEstadoHistorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnterior = table.Column<byte>(type: "tinyint", nullable: true),
                    EstadoNuevo = table.Column<byte>(type: "tinyint", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvioEstadoHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvioEstadoHistorial_Envios_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "Envios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asignaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioId = table.Column<int>(type: "int", nullable: false),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    FechaViaje = table.Column<DateOnly>(type: "date", nullable: false),
                    DesvioKm = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TiempoExtraMin = table.Column<int>(type: "int", nullable: false),
                    Ganancia = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asignaciones_Envios_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "Envios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asignaciones_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Paradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paradas_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViajeDias",
                columns: table => new
                {
                    ViajeId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViajeDias", x => new { x.ViajeId, x.DiaSemana });
                    table.ForeignKey(
                        name: "FK_ViajeDias_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_EnvioId",
                table: "Asignaciones",
                column: "EnvioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_ViajeId",
                table: "Asignaciones",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvioEstadoHistorial_EnvioId",
                table: "EnvioEstadoHistorial",
                column: "EnvioId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_UsuarioId",
                table: "Envios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Paradas_ViajeId",
                table: "Paradas",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_UsuarioId",
                table: "Viajes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asignaciones");

            migrationBuilder.DropTable(
                name: "EnvioEstadoHistorial");

            migrationBuilder.DropTable(
                name: "Paradas");

            migrationBuilder.DropTable(
                name: "ViajeDias");

            migrationBuilder.DropTable(
                name: "Envios");

            migrationBuilder.DropTable(
                name: "Viajes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
