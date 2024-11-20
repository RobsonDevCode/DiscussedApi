﻿// <auto-generated />
using System;
using DiscussedApi.Data.Topics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiscussedApi.Migrations.TopicDB
{
    [DbContext(typeof(TopicDBContext))]
    [Migration("20241114230557_AddTopic")]
    partial class AddTopic
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("DiscussedApi.Models.Topic.Topic", b =>
                {
                    b.Property<Guid>("TopicId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("DtCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Vote")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("TopicId");

                    b.ToTable("Topics");
                });
#pragma warning restore 612, 618
        }
    }
}