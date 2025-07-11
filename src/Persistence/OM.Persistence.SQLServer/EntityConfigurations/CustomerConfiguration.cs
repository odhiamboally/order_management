using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OM.Domain.Entities;
using OM.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.EntityConfigurations;
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Segment)
            .HasConversion<int>()
            .HasDefaultValue(CustomerSegment.New);

        builder.Property(c => c.TotalOrders)
            .HasDefaultValue(0);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasIndex(c => c.Segment);
    }
}

