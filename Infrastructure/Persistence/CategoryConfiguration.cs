using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("category");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(p => p.Name)
                   .HasColumnName("nome")
                   .IsRequired();

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();
        }
    }
}
