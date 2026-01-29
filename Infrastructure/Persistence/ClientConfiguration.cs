using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Client");

            builder.HasKey(c => c.clientId);

            builder.Property(c => c.clientName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.clientPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(c => c.clientPhone)
                .IsUnique(false);

            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Client)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}