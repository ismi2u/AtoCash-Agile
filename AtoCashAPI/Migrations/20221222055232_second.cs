using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtoCashAPI.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessTypeId",
                table: "PettyCashRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusinessTypeId",
                table: "EmployeeExtendedInfos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashRequests_BusinessTypeId",
                table: "PettyCashRequests",
                column: "BusinessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeExtendedInfos_BusinessTypeId",
                table: "EmployeeExtendedInfos",
                column: "BusinessTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeExtendedInfos_BusinessTypes_BusinessTypeId",
                table: "EmployeeExtendedInfos",
                column: "BusinessTypeId",
                principalTable: "BusinessTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PettyCashRequests_BusinessTypes_BusinessTypeId",
                table: "PettyCashRequests",
                column: "BusinessTypeId",
                principalTable: "BusinessTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeExtendedInfos_BusinessTypes_BusinessTypeId",
                table: "EmployeeExtendedInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_PettyCashRequests_BusinessTypes_BusinessTypeId",
                table: "PettyCashRequests");

            migrationBuilder.DropIndex(
                name: "IX_PettyCashRequests_BusinessTypeId",
                table: "PettyCashRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeExtendedInfos_BusinessTypeId",
                table: "EmployeeExtendedInfos");

            migrationBuilder.DropColumn(
                name: "BusinessTypeId",
                table: "PettyCashRequests");

            migrationBuilder.DropColumn(
                name: "BusinessTypeId",
                table: "EmployeeExtendedInfos");
        }
    }
}
