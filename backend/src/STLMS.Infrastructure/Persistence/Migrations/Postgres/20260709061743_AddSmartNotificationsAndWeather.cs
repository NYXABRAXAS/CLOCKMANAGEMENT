using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STLMS.Infrastructure.Persistence.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddSmartNotificationsAndWeather : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushNotificationsEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "WeatherLatitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeatherLongitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicineReminderLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineReminderLogs_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FcmToken = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineReminderLogs_MedicineId_Date_Hour_Minute",
                table: "MedicineReminderLogs",
                columns: new[] { "MedicineId", "Date", "Hour", "Minute" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId_FcmToken",
                table: "UserDevices",
                columns: new[] { "UserId", "FcmToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicineReminderLogs");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PushNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeatherLatitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeatherLongitude",
                table: "Users");
        }
    }
}
