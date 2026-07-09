using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STLMS.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddReligionCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrayerCalculationMethod",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailyQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReligionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuotes_Religions_ReligionId",
                        column: x => x.ReligionId,
                        principalTable: "Religions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FestivalCalendarEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReligionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalCalendarEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FestivalCalendarEntries_Religions_ReligionId",
                        column: x => x.ReligionId,
                        principalTable: "Religions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPrayerLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PrayerName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrayerLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPrayerLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuotes_ReligionId",
                table: "DailyQuotes",
                column: "ReligionId");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalCalendarEntries_ReligionId_Date",
                table: "FestivalCalendarEntries",
                columns: new[] { "ReligionId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPrayerLogs_UserId_Date_PrayerName",
                table: "UserPrayerLogs",
                columns: new[] { "UserId", "Date", "PrayerName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuotes");

            migrationBuilder.DropTable(
                name: "FestivalCalendarEntries");

            migrationBuilder.DropTable(
                name: "UserPrayerLogs");

            migrationBuilder.DropColumn(
                name: "PrayerCalculationMethod",
                table: "Users");
        }
    }
}
