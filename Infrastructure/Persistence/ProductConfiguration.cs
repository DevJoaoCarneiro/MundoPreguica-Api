using Domain.entities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .HasColumnName("id");

            builder.Property(p => p.Name)
                   .HasColumnName("nome")
                   .IsRequired();

            builder.Property(p => p.Category)
                   .HasColumnName("categoria")
                   .IsRequired();

            builder.Property(p => p.Price)
                   .HasColumnName("preco")
                   .HasPrecision(10, 2)
                   .IsRequired();

            builder.Property(p => p.ImageUrL)
                   .HasColumnName("image_url")
                   .IsRequired();

            builder.Property(p => p.Status)
                   .HasColumnName("status")
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("data_criacao")
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .HasColumnName("data_update");
        }
    }
}
