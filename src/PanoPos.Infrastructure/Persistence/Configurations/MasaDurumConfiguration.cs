using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PanoPos.Domain.Entities;

namespace PanoPos.Infrastructure.Persistence.Configurations;

public sealed class MasaDurumConfiguration : IEntityTypeConfiguration<MasaDurum>
{
    public void Configure(EntityTypeBuilder<MasaDurum> builder)
    {
        builder.ToTable("MasaDurum");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Ad).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AktifMi).IsRequired();
    }
}
