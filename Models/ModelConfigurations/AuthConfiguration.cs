using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TodoApi.Models.ModelConfigurations
{
    public class AuthConfiguration : IEntityTypeConfiguration<Users>
    {
        public AuthConfiguration()
        {
        }

        public void Configure(EntityTypeBuilder<Users> builder){
            builder.HasKey(prop => prop.Id);
            
            builder.Property(prop => prop.Login)
                .IsRequired();

            builder.Property(prop => prop.Password)
                .IsRequired();

            builder.Property(prop => prop.Role)
                .IsRequired();
        }
        
    }
}