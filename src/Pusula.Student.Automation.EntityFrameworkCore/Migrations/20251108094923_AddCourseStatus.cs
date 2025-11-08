using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pusula.Student.Automation.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCourses_TenantId_Code",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppCourses");

            migrationBuilder.AlterColumn<int>(
                name: "Credit",
                table: "AppCourses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AppCourses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AppCourses_Code",
                table: "AppCourses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCourses_Code",
                table: "AppCourses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppCourses");

            migrationBuilder.AlterColumn<int>(
                name: "Credit",
                table: "AppCourses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppCourses",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppCourses",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppCourses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppCourses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppCourses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppCourses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppCourses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCourses_TenantId_Code",
                table: "AppCourses",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }
    }
}
