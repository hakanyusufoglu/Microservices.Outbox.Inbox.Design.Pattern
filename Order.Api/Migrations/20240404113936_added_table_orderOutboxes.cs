﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Api.Migrations
{
    /// <inheritdoc />
    public partial class added_table_orderOutboxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderOutboxes",
                columns: table => new
                {
                    OccuredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderOutboxes");
        }
    }
}
