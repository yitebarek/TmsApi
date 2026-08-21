using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TmsApi.Configuration
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);//primary key
            builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(15);//registration number is required and has a max length of 15
            builder.Property(s => s.Name).IsRequired().HasMaxLength(30);//name is required and has a max length of 30
            builder.Property(s => s.GPA).HasColumnType("decimal(3, 2)");//GPA is a decimal with 3 digits and 2 decimal places
            builder.Property(s => s.IsActive).IsRequired();//is active is required
            builder.Property<DateTime>("LastUpdated");//shadow property for last updated date and time
            builder.Property<byte[]>("Version").IsRowVersion();
            builder.HasKey(s => s.Id);//primary key
            builder.Property(s => s.IsDeleted).IsRequired();//is deleted is required

            // one-to-many relationship between student and enrollment
            builder.HasMany(s => s.Enrollments).WithOne(e => e.Student).HasForeignKey(e => e.StudentId);
        }

    }
}