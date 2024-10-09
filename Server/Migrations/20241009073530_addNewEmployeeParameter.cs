using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class addNewEmployeeParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.AddColumn<bool>(
                name: "enformation",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enprojet",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Treasury",
                table: "companies",
                type: "integer",
                nullable: false,
                defaultValue: 85000,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1000000);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Communication");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Programmation");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Reseau");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Cybersecurite");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Management");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enformation",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "enprojet",
                table: "employees");

            migrationBuilder.AlterColumn<int>(
                name: "Treasury",
                table: "companies",
                type: "integer",
                nullable: false,
                defaultValue: 1000000,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 85000);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "HTML");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "CSS");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "JavaScript");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "TypeScript");

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "React");

            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 6, "Angular" },
                    { 7, "Vue.js" },
                    { 8, "Node.js" },
                    { 9, "Express.js" },
                    { 10, "ASP.NET Core" },
                    { 11, "Ruby on Rails" },
                    { 12, "Django" },
                    { 13, "Flask" },
                    { 14, "PHP" },
                    { 15, "Laravel" },
                    { 16, "Spring Boot" },
                    { 17, "SQL" },
                    { 18, "NoSQL" },
                    { 19, "GraphQL" },
                    { 20, "REST APIs" }
                });
        }
    }
}
