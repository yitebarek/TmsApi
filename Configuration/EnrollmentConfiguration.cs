using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TmsApi.Configuration
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);//primary key
            builder.Property(e => e.GPA).HasPrecision(3, 2);//GPA is a decimal with 3 digits and 2 decimal places
            builder.Property(e => e.StudentId).IsRequired();//student id is required
            builder.Property(e => e.CourseId).IsRequired();//course id is required
            builder.Property(e => e.EnrolledAt).IsRequired();//enrolled at is required
            builder.Property(e => e.Year).IsRequired();//year is required
            //student relationship - one student can have many enrollments, but each enrollment belongs to one student
            builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);//priventing deletion of student if there are enrollments associated with it
            //course relationship - one course can have many enrollments, but each enrollment belongs to one course
            builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);//preventing deletion of course if there are enrollments associated with it
        }
    }
}       
        