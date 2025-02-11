﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stock.Service.Models.Contexts;

#nullable disable

namespace Stock.Service.Migrations
{
    [DbContext(typeof(StockDbContext))]
    [Migration("20240405113156_editted-order-inboxes-table-id-column-to-idempotenttoken")]
    partial class edittedorderinboxestableidcolumntoidempotenttoken
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Stock.Service.Models.Entities.OrderInbox", b =>
                {
                    b.Property<Guid>("IdempotentToken")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Processed")
                        .HasColumnType("bit");

                    b.HasKey("IdempotentToken");

                    b.ToTable("OrderInboxes");
                });
#pragma warning restore 612, 618
        }
    }
}
