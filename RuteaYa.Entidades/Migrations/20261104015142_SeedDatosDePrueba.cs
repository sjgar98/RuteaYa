using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RuteaYa.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class SeedDatosDePrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Envios",
                columns: new[] { "Id", "CreadoEn", "Descripcion", "DestinoDir", "DestinoLat", "DestinoLng", "Estado", "FechaLimite", "Monto", "OrigenDir", "OrigenLat", "OrigenLng", "PesoKg", "Tamano", "UsuarioId" },
                values: new object[,]
                {
                    { 13, new DateTime(2026, 10, 18, 15, 0, 0, 0, DateTimeKind.Unspecified), "Repuesto", "Palermo, CABA", -34.588900m, -58.430600m, (byte)5, new DateOnly(2026, 11, 17), 2500m, "Avellaneda, Buenos Aires", -34.662700m, -58.365300m, 1m, (byte)1, 1 },
                    { 14, new DateTime(2026, 10, 30, 15, 0, 0, 0, DateTimeKind.Unspecified), "Documentos", "Quilmes, Buenos Aires", -34.720600m, -58.254600m, (byte)2, new DateOnly(2026, 11, 29), 3500m, "Recoleta, CABA", -34.587500m, -58.397400m, 0.5m, (byte)1, 1 },
                    { 15, new DateTime(2026, 11, 1, 15, 0, 0, 0, DateTimeKind.Unspecified), "Caja de libros", "Belgrano, CABA", -34.562700m, -58.456500m, (byte)1, new DateOnly(2027, 6, 30), 3000m, "San Isidro, Buenos Aires", -34.470800m, -58.528600m, 5m, (byte)2, 1 },
                    { 16, new DateTime(2026, 9, 12, 15, 0, 0, 0, DateTimeKind.Unspecified), "Regalo", "Caballito, CABA", -34.618900m, -58.442200m, (byte)6, new DateOnly(2026, 10, 12), 2000m, "Plaza Italia, Palermo, CABA", -34.581000m, -58.420800m, 1m, (byte)1, 1 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Apellido", "CreadoEn", "Email", "Nombre", "PasswordHash" },
                values: new object[] { 2, "Gómez", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "remitente@ruteaya.com", "María", "AQAAAAIAAYagAAAAEKndklYBED8heEK5UUKWAFP0VZIhX94RvKi/4waoLwHrspyn1PcxGIq8VsiZOkk/NQ==" });

            migrationBuilder.InsertData(
                table: "Viajes",
                columns: new[] { "Id", "Activo", "DestinoDir", "DestinoLat", "DestinoLng", "DesvioMaxKm", "DistanciaKm", "DuracionMin", "HoraLlegada", "HoraSalida", "MaxPaquetes", "OrigenDir", "OrigenLat", "OrigenLng", "PesoMaxKg", "UsuarioId" },
                values: new object[,]
                {
                    { 1, true, "Lomas de Zamora, Buenos Aires", -34.760900m, -58.406300m, 5m, 18.4m, 55, new TimeOnly(8, 55, 0), new TimeOnly(8, 0, 0), 3, "Belgrano, CABA", -34.562700m, -58.456500m, 10m, 1 },
                    { 2, true, "San Isidro, Buenos Aires", -34.470800m, -58.528600m, 3m, 21.5m, 50, new TimeOnly(19, 20, 0), new TimeOnly(18, 30, 0), 2, "Palermo, CABA", -34.588900m, -58.430600m, 5m, 1 },
                    { 3, false, "Palermo, CABA", -34.588900m, -58.430600m, 4m, 12.0m, 40, new TimeOnly(9, 40, 0), new TimeOnly(9, 0, 0), 4, "Avellaneda, Buenos Aires", -34.662700m, -58.365300m, 12m, 1 }
                });

            migrationBuilder.InsertData(
                table: "EnvioEstadoHistorial",
                columns: new[] { "Id", "EnvioId", "EstadoAnterior", "EstadoNuevo", "Fecha" },
                values: new object[,]
                {
                    { 31, 13, null, (byte)1, new DateTime(2026, 10, 18, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 32, 13, (byte)1, (byte)2, new DateTime(2026, 10, 20, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 33, 13, (byte)2, (byte)3, new DateTime(2026, 10, 24, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 34, 13, (byte)3, (byte)4, new DateTime(2026, 10, 24, 12, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 35, 13, (byte)4, (byte)5, new DateTime(2026, 10, 24, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 36, 14, null, (byte)1, new DateTime(2026, 10, 30, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 37, 14, (byte)1, (byte)2, new DateTime(2026, 11, 1, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 38, 15, null, (byte)1, new DateTime(2026, 11, 1, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 39, 16, null, (byte)1, new DateTime(2026, 9, 12, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 40, 16, (byte)1, (byte)6, new DateTime(2026, 9, 14, 15, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Envios",
                columns: new[] { "Id", "CreadoEn", "Descripcion", "DestinoDir", "DestinoLat", "DestinoLng", "Estado", "FechaLimite", "Monto", "OrigenDir", "OrigenLat", "OrigenLng", "PesoKg", "Tamano", "UsuarioId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 28, 13, 0, 0, 0, DateTimeKind.Unspecified), "Caja con zapatillas", "Lomas de Zamora, Buenos Aires", -34.760900m, -58.406300m, (byte)1, new DateOnly(2027, 6, 30), 4000m, "Caballito, CABA", -34.618900m, -58.442200m, 2m, (byte)1, 2 },
                    { 2, new DateTime(2026, 10, 28, 13, 10, 0, 0, DateTimeKind.Unspecified), "Documentos y carpetas", "Lomas de Zamora, Buenos Aires", -34.760900m, -58.406300m, (byte)1, new DateOnly(2027, 6, 30), 5500m, "Palermo, CABA", -34.588900m, -58.430600m, 6m, (byte)2, 2 },
                    { 3, new DateTime(2026, 10, 29, 14, 0, 0, 0, DateTimeKind.Unspecified), "Caja de herramientas", "Caballito, CABA", -34.618900m, -58.442200m, (byte)1, new DateOnly(2027, 6, 30), 3500m, "Plaza Italia, Palermo, CABA", -34.581000m, -58.420800m, 4m, (byte)2, 2 },
                    { 4, new DateTime(2026, 10, 29, 14, 20, 0, 0, DateTimeKind.Unspecified), "Monitor", "Avellaneda, Buenos Aires", -34.662700m, -58.365300m, (byte)1, new DateOnly(2027, 6, 30), 7000m, "Palermo, CABA", -34.588900m, -58.430600m, 8m, (byte)3, 2 },
                    { 5, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Unspecified), "Libro", "Lomas de Zamora, Buenos Aires", -34.760900m, -58.406300m, (byte)1, new DateOnly(2027, 6, 30), 2500m, "Barracas, CABA", -34.645300m, -58.382600m, 1m, (byte)1, 2 },
                    { 6, new DateTime(2026, 10, 30, 12, 30, 0, 0, DateTimeKind.Unspecified), "Zapatillas", "San Isidro, Buenos Aires", -34.470800m, -58.528600m, (byte)1, new DateOnly(2027, 6, 30), 4000m, "Plaza Italia, Palermo, CABA", -34.581000m, -58.420800m, 1.5m, (byte)1, 2 },
                    { 7, new DateTime(2026, 10, 31, 15, 0, 0, 0, DateTimeKind.Unspecified), "Bolso de mano", "San Isidro, Buenos Aires", -34.470800m, -58.528600m, (byte)3, new DateOnly(2026, 11, 30), 3000m, "Belgrano, CABA", -34.562700m, -58.456500m, 2m, (byte)1, 2 },
                    { 8, new DateTime(2026, 10, 26, 15, 0, 0, 0, DateTimeKind.Unspecified), "Caja de vinos", "San Isidro, Buenos Aires", -34.470800m, -58.528600m, (byte)4, new DateOnly(2026, 11, 25), 4500m, "Plaza Italia, Palermo, CABA", -34.581000m, -58.420800m, 3m, (byte)2, 2 },
                    { 9, new DateTime(2026, 9, 3, 15, 0, 0, 0, DateTimeKind.Unspecified), "Repuestos de auto", "Palermo, CABA", -34.588900m, -58.430600m, (byte)5, new DateOnly(2026, 10, 3), 3000m, "Avellaneda, Buenos Aires", -34.662700m, -58.365300m, 3m, (byte)2, 2 },
                    { 10, new DateTime(2026, 9, 18, 15, 0, 0, 0, DateTimeKind.Unspecified), "Notebook", "Palermo, CABA", -34.588900m, -58.430600m, (byte)5, new DateOnly(2026, 10, 18), 5000m, "Barracas, CABA", -34.645300m, -58.382600m, 2m, (byte)1, 2 },
                    { 11, new DateTime(2026, 9, 30, 15, 0, 0, 0, DateTimeKind.Unspecified), "Cuadro", "Recoleta, CABA", -34.587500m, -58.397400m, (byte)5, new DateOnly(2026, 10, 30), 4500m, "Avellaneda, Buenos Aires", -34.662700m, -58.365300m, 4m, (byte)3, 2 },
                    { 12, new DateTime(2026, 10, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), "Ropa", "Morón, Buenos Aires", -34.653400m, -58.619800m, (byte)6, new DateOnly(2026, 11, 9), 2000m, "Caballito, CABA", -34.618900m, -58.442200m, 2m, (byte)1, 2 }
                });

            migrationBuilder.InsertData(
                table: "ViajeDias",
                columns: new[] { "DiaSemana", "ViajeId" },
                values: new object[,]
                {
                    { (byte)1, 1 },
                    { (byte)3, 1 },
                    { (byte)5, 1 },
                    { (byte)5, 2 },
                    { (byte)2, 3 },
                    { (byte)4, 3 }
                });

            migrationBuilder.InsertData(
                table: "Viajes",
                columns: new[] { "Id", "Activo", "DestinoDir", "DestinoLat", "DestinoLng", "DesvioMaxKm", "DistanciaKm", "DuracionMin", "HoraLlegada", "HoraSalida", "MaxPaquetes", "OrigenDir", "OrigenLat", "OrigenLng", "PesoMaxKg", "UsuarioId" },
                values: new object[] { 4, true, "Quilmes, Buenos Aires", -34.720600m, -58.254600m, 4m, 21.0m, 50, new TimeOnly(10, 50, 0), new TimeOnly(10, 0, 0), 3, "Recoleta, CABA", -34.587500m, -58.397400m, 8m, 2 });

            migrationBuilder.InsertData(
                table: "Asignaciones",
                columns: new[] { "Id", "AsignadoEn", "DesvioKm", "EnvioId", "FechaViaje", "Ganancia", "TiempoExtraMin", "ViajeId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 2, 14, 0, 0, 0, DateTimeKind.Unspecified), 1.2m, 7, new DateOnly(2026, 11, 6), 3000m, 6, 2 },
                    { 2, new DateTime(2026, 10, 28, 14, 0, 0, 0, DateTimeKind.Unspecified), 0.8m, 8, new DateOnly(2026, 10, 30), 4500m, 4, 2 },
                    { 3, new DateTime(2026, 9, 5, 14, 0, 0, 0, DateTimeKind.Unspecified), 1.0m, 9, new DateOnly(2026, 9, 8), 3000m, 5, 3 },
                    { 4, new DateTime(2026, 9, 20, 14, 0, 0, 0, DateTimeKind.Unspecified), 1.5m, 10, new DateOnly(2026, 9, 24), 5000m, 7, 3 },
                    { 5, new DateTime(2026, 10, 2, 14, 0, 0, 0, DateTimeKind.Unspecified), 2.0m, 11, new DateOnly(2026, 10, 6), 4500m, 9, 3 },
                    { 6, new DateTime(2026, 10, 20, 14, 0, 0, 0, DateTimeKind.Unspecified), 1.1m, 13, new DateOnly(2026, 10, 24), 2500m, 5, 4 },
                    { 7, new DateTime(2026, 11, 1, 14, 0, 0, 0, DateTimeKind.Unspecified), 0.7m, 14, new DateOnly(2026, 11, 7), 3500m, 3, 4 }
                });

            migrationBuilder.InsertData(
                table: "EnvioEstadoHistorial",
                columns: new[] { "Id", "EnvioId", "EstadoAnterior", "EstadoNuevo", "Fecha" },
                values: new object[,]
                {
                    { 1, 1, null, (byte)1, new DateTime(2026, 10, 28, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, null, (byte)1, new DateTime(2026, 10, 28, 13, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, null, (byte)1, new DateTime(2026, 10, 29, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 4, null, (byte)1, new DateTime(2026, 10, 29, 14, 20, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 5, null, (byte)1, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, 6, null, (byte)1, new DateTime(2026, 10, 30, 12, 30, 0, 0, DateTimeKind.Unspecified) },
                    { 7, 7, null, (byte)1, new DateTime(2026, 10, 31, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, 7, (byte)1, (byte)2, new DateTime(2026, 11, 2, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, 7, (byte)2, (byte)3, new DateTime(2026, 11, 6, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, 8, null, (byte)1, new DateTime(2026, 10, 26, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, 8, (byte)1, (byte)2, new DateTime(2026, 10, 28, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, 8, (byte)2, (byte)3, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 13, 8, (byte)3, (byte)4, new DateTime(2026, 10, 30, 12, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 14, 9, null, (byte)1, new DateTime(2026, 9, 3, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 15, 9, (byte)1, (byte)2, new DateTime(2026, 9, 5, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 16, 9, (byte)2, (byte)3, new DateTime(2026, 9, 8, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, 9, (byte)3, (byte)4, new DateTime(2026, 9, 8, 12, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 18, 9, (byte)4, (byte)5, new DateTime(2026, 9, 8, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, 10, null, (byte)1, new DateTime(2026, 9, 18, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, 10, (byte)1, (byte)2, new DateTime(2026, 9, 20, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 21, 10, (byte)2, (byte)3, new DateTime(2026, 9, 24, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 22, 10, (byte)3, (byte)4, new DateTime(2026, 9, 24, 12, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 23, 10, (byte)4, (byte)5, new DateTime(2026, 9, 24, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 24, 11, null, (byte)1, new DateTime(2026, 9, 30, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 25, 11, (byte)1, (byte)2, new DateTime(2026, 10, 2, 14, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 26, 11, (byte)2, (byte)3, new DateTime(2026, 10, 6, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 27, 11, (byte)3, (byte)4, new DateTime(2026, 10, 6, 12, 10, 0, 0, DateTimeKind.Unspecified) },
                    { 28, 11, (byte)4, (byte)5, new DateTime(2026, 10, 6, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 29, 12, null, (byte)1, new DateTime(2026, 10, 10, 15, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 30, 12, (byte)1, (byte)6, new DateTime(2026, 10, 12, 15, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "ViajeDias",
                columns: new[] { "DiaSemana", "ViajeId" },
                values: new object[] { (byte)6, 4 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Asignaciones",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "EnvioEstadoHistorial",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)1, 1 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)3, 1 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)5, 1 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)5, 2 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)2, 3 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)4, 3 });

            migrationBuilder.DeleteData(
                table: "ViajeDias",
                keyColumns: new[] { "DiaSemana", "ViajeId" },
                keyValues: new object[] { (byte)6, 4 });

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Envios",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Viajes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Viajes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Viajes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Viajes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
