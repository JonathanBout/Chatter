﻿// <auto-generated />
using System;
using Chatter.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chatter.Server.Migrations
{
    [DbContext(typeof(ChatDatabaseContext))]
    [Migration("20241004142122_DeliveryTimestamp")]
    partial class DeliveryTimestamp
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Chatter.Server.Data.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTimeOffset>("DeliveredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("delivered_at");

                    b.Property<bool>("IsEncrypted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_encrypted");

                    b.Property<Guid>("ReceiverId")
                        .HasColumnType("uuid")
                        .HasColumnName("receiver_id");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("uuid")
                        .HasColumnName("sender_id");

                    b.Property<DateTimeOffset>("SentAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sent_at");

                    b.HasKey("Id")
                        .HasName("pk_messages");

                    b.HasIndex("ReceiverId")
                        .HasDatabaseName("ix_messages_receiver_id");

                    b.HasIndex("SenderId")
                        .HasDatabaseName("ix_messages_sender_id");

                    b.ToTable("messages", (string)null);
                });

            modelBuilder.Entity("Chatter.Server.Data.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("password_hash");

                    b.Property<byte[]>("PublicKey")
                        .HasColumnType("bytea")
                        .HasColumnName("public_key");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000001"),
                            PasswordHash = new byte[0],
                            Username = "Admin"
                        },
                        new
                        {
                            Id = new Guid("00000000-0000-0000-0000-000000000002"),
                            PasswordHash = new byte[0],
                            Username = "DefaultUser"
                        });
                });

            modelBuilder.Entity("Chatter.Server.Data.Message", b =>
                {
                    b.HasOne("Chatter.Server.Data.User", "Receiver")
                        .WithMany("AvailableMessages")
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_receiver_id");

                    b.HasOne("Chatter.Server.Data.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_sender_id");

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Chatter.Server.Data.User", b =>
                {
                    b.Navigation("AvailableMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
