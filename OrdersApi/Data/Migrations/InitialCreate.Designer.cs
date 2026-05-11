using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Orders.Api.Data;

#nullable disable

namespace Orders.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260509000000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.4")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("Orders.Api.Models.Order", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<string>("Cliente").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<DateTime>("DataCriacao").HasColumnType("timestamp with time zone");
            b.Property<string>("Produto").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<string>("Status").IsRequired().HasColumnType("text");
            b.Property<decimal>("Valor").HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            b.HasKey("Id");
            b.ToTable("Orders");
        });

        modelBuilder.Entity("Orders.Api.Models.OrderStatusHistory", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<Guid>("OrderId").HasColumnType("uuid");
            b.Property<DateTime>("ChangedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("FromStatus").HasColumnType("text");
            b.Property<string>("ToStatus").IsRequired().HasColumnType("text");
            b.HasKey("Id");
            b.HasIndex("OrderId");
            b.ToTable("OrderStatusHistories");
        });

        modelBuilder.Entity("Orders.Api.Models.OutboxMessage", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<Guid>("OrderId").HasColumnType("uuid");
            b.Property<string>("Payload").IsRequired().HasColumnType("text");
            b.Property<string>("EventType").IsRequired().HasColumnType("text");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<DateTime?>("ProcessedAt").HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex("OrderId");
            b.HasIndex("ProcessedAt");
            b.ToTable("OutboxMessages");
        });

        modelBuilder.Entity("Orders.Api.Models.OrderStatusHistory", b =>
        {
            b.HasOne("Orders.Api.Models.Order", "Order")
                .WithMany("StatusHistory")
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Order");
        });

        modelBuilder.Entity("Orders.Api.Models.OutboxMessage", b =>
        {
            b.HasOne("Orders.Api.Models.Order", "Order")
                .WithMany("OutboxMessages")
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Order");
        });

        modelBuilder.Entity("Orders.Api.Models.Order", b =>
        {
            b.Navigation("StatusHistory");
            b.Navigation("OutboxMessages");
        });
#pragma warning restore 612, 618
    }
}
