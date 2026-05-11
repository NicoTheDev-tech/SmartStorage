using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToStorageUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageUnits_UnitNumber",
                table: "StorageUnits");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IdNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OccupancyStatus",
                table: "StorageUnits");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "UnitNumber",
                table: "StorageUnits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "StorageUnits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "StorageUnits",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClimateControl",
                table: "StorageUnits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StorageUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_StorageUnits_UnitNumber",
                table: "StorageUnits",
                column: "UnitNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageUnits_UnitNumber",
                table: "StorageUnits");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StorageUnits");

            migrationBuilder.AlterColumn<string>(
                name: "UnitNumber",
                table: "StorageUnits",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "StorageUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "StorageUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClimateControl",
                table: "StorageUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyStatus",
                table: "StorageUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "Clients",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 1,
                column: "OccupancyStatus",
                value: 0);

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 2,
                column: "OccupancyStatus",
                value: 0);

            migrationBuilder.UpdateData(
                table: "StorageUnits",
                keyColumn: "Id",
                keyValue: 3,
                column: "OccupancyStatus",
                value: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StorageUnits_UnitNumber",
                table: "StorageUnits",
                column: "UnitNumber",
                unique: true,
                filter: "[UnitNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IdNumber",
                table: "Clients",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");
        }
    }
}
