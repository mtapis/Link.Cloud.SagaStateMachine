using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Link.Cloud.SagaStateMachine.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStateInstance",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStateInstance", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleName",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventStateInstanceCorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleName_EventStateInstance_EventStateInstanceCorrelationId",
                        column: x => x.EventStateInstanceCorrelationId,
                        principalTable: "EventStateInstance",
                        principalColumn: "CorrelationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleName_EventStateInstanceCorrelationId",
                table: "UserRoleName",
                column: "EventStateInstanceCorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoleName");

            migrationBuilder.DropTable(
                name: "EventStateInstance");
        }
    }
}
