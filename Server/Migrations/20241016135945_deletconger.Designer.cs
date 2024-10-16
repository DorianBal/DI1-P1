﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Server.Persistence;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(WssDbContext))]
    [Migration("20241016135945_deletconger")]
    partial class deletconger
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Server.Models.Company", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("PlayerId")
                        .HasColumnType("integer");

                    b.Property<int>("Treasury")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(85000);

                    b.HasKey("Id");

                    b.HasIndex("PlayerId")
                        .IsUnique();

                    b.ToTable("companies", (string)null);
                });

            modelBuilder.Entity("Server.Models.Consultant", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("SalaryRequirement")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("consultants", (string)null);
                });

            modelBuilder.Entity("Server.Models.Employee", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Salary")
                        .HasColumnType("integer");

                    b.Property<int>("dureeformation")
                        .HasColumnType("integer");

                    b.Property<bool>("enformation")
                        .HasColumnType("boolean");

                    b.Property<bool>("enprojet")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("GameId");

                    b.ToTable("employees", (string)null);
                });

            modelBuilder.Entity("Server.Models.Game", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Rounds")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)")
                        .HasDefaultValue("Waiting");

                    b.HasKey("Id");

                    b.ToTable("games", (string)null);
                });

            modelBuilder.Entity("Server.Models.Player", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("Server.Models.Project", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("ProjectDuration")
                        .HasColumnType("integer");

                    b.Property<int>("Revenu")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("GameId");

                    b.ToTable("projects", (string)null);
                });

            modelBuilder.Entity("Server.Models.Round", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("rounds", (string)null);
                });

            modelBuilder.Entity("Server.Models.Skill", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.ToTable("skills", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Communication"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Programmation"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Reseau"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Cybersecurite"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Management"
                        });
                });

            modelBuilder.Entity("Server.Models.Company", b =>
                {
                    b.HasOne("Server.Models.Player", "Player")
                        .WithOne("Company")
                        .HasForeignKey("Server.Models.Company", "PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Server.Models.Consultant", b =>
                {
                    b.HasOne("Server.Models.Game", "Game")
                        .WithMany("Consultants")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Server.Models.LeveledSkill", "Skills", b1 =>
                        {
                            b1.Property<int>("ConsultantId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Level")
                                .HasColumnType("integer");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("ConsultantId", "Id");

                            b1.ToTable("consultants");

                            b1.ToJson("Skills");

                            b1.WithOwner()
                                .HasForeignKey("ConsultantId");
                        });

                    b.Navigation("Game");

                    b.Navigation("Skills");
                });

            modelBuilder.Entity("Server.Models.Employee", b =>
                {
                    b.HasOne("Server.Models.Company", "Company")
                        .WithMany("Employees")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Server.Models.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Server.Models.LeveledSkill", "Skills", b1 =>
                        {
                            b1.Property<int>("EmployeeId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Level")
                                .HasColumnType("integer");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("EmployeeId", "Id");

                            b1.ToTable("employees");

                            b1.ToJson("Skills");

                            b1.WithOwner()
                                .HasForeignKey("EmployeeId");
                        });

                    b.Navigation("Company");

                    b.Navigation("Game");

                    b.Navigation("Skills");
                });

            modelBuilder.Entity("Server.Models.Player", b =>
                {
                    b.HasOne("Server.Models.Game", "Game")
                        .WithMany("Players")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("Server.Models.Project", b =>
                {
                    b.HasOne("Server.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId");

                    b.HasOne("Server.Models.Game", "Game")
                        .WithMany("Projects")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Server.Models.LeveledSkill", "Skills", b1 =>
                        {
                            b1.Property<int>("ProjectId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Level")
                                .HasColumnType("integer");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("ProjectId", "Id");

                            b1.ToTable("projects");

                            b1.ToJson("Skills");

                            b1.WithOwner()
                                .HasForeignKey("ProjectId");
                        });

                    b.Navigation("Company");

                    b.Navigation("Game");

                    b.Navigation("Skills");
                });

            modelBuilder.Entity("Server.Models.Round", b =>
                {
                    b.HasOne("Server.Models.Game", "Game")
                        .WithMany("RoundsCollection")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Server.Models.RoundAction", "Actions", b1 =>
                        {
                            b1.Property<int>("RoundId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("ActionType")
                                .HasColumnType("text");

                            b1.Property<string>("Payload")
                                .HasColumnType("text");

                            b1.Property<int?>("PlayerId")
                                .HasColumnType("integer");

                            b1.HasKey("RoundId", "Id");

                            b1.ToTable("rounds");

                            b1.ToJson("Actions");

                            b1.WithOwner()
                                .HasForeignKey("RoundId");
                        });

                    b.Navigation("Actions");

                    b.Navigation("Game");
                });

            modelBuilder.Entity("Server.Models.Company", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Server.Models.Game", b =>
                {
                    b.Navigation("Consultants");

                    b.Navigation("Players");

                    b.Navigation("Projects");

                    b.Navigation("RoundsCollection");
                });

            modelBuilder.Entity("Server.Models.Player", b =>
                {
                    b.Navigation("Company")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
