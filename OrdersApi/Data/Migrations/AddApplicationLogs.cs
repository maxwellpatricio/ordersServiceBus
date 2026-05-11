using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Api.Data.Migrations;

public partial class AddApplicationLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Message = table.Column<string>(type: "text", nullable: false),
                Details = table.Column<string>(type: "text", nullable: true),
                OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ApplicationLogs", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationLogs_CreatedAt",
            table: "ApplicationLogs",
            column: "CreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApplicationLogs");
    }
}
