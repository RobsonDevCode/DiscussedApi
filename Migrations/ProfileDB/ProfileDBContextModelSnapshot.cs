﻿// <auto-generated />
using System;
using DiscussedApi.Data.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiscussedApi.Migrations.ProfileDB
{
    [DbContext(typeof(ProfileDBContext))]
    partial class ProfileDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("DiscussedApi.Models.Profiles.Follower", b =>
                {
                    b.Property<Guid>("UserGuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("IsFollowing")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("UserGuid");

                    b.ToTable("Follower");
                });

            modelBuilder.Entity("DiscussedApi.Models.Profiles.Following", b =>
                {
                    b.Property<long>("FollowRef")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("FollowRef"));

                    b.Property<bool?>("IsFollowing")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid?>("UserFollowing")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UserGuid")
                        .HasColumnType("char(36)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("FollowRef");

                    b.ToTable("Following");
                });

            modelBuilder.Entity("DiscussedApi.Models.Profiles.Profile", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<int>("FollerCount")
                        .HasColumnType("int");

                    b.Property<int>("FollowingCount")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Profile");
                });
#pragma warning restore 612, 618
        }
    }
}
