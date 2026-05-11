using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Api.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Produto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Orders", x => x.Id));

        migrationBuilder.CreateTable(
            name: "OrderStatusHistories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                FromStatus = table.Column<string>(type: "text", nullable: true),
                ToStatus = table.Column<string>(type: "text", nullable: false),
                ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderStatusHistories_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false),
                EventType = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                table.ForeignKey(
                    name: "FK_OutboxMessages_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistories_OrderId",
            table: "OrderStatusHistories",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_OrderId",
            table: "OutboxMessages",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_ProcessedAt",
            table: "OutboxMessages",
            column: "ProcessedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OutboxMessages");
        migrationBuilder.DropTable(name: "OrderStatusHistories");
        migrationBuilder.DropTable(name: "Orders");
    }
}
