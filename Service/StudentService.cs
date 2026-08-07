
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Service
{
    public class StudentService
    {
        private readonly DbContext _context;

        public StudentService(DbContext context)
        {
            _context = context;
        }

        public async Task UpdateStudentAsync(Student student, UpdateStudentRequest request)
        {
            student.Name = request.Name;

            _context.Entry(student)
                .Property("LastUpdated")
                .CurrentValue = DateTime.UtcNow;
            await _context.SaveChangesAsync();// Save the changes to the database
        }
        
    }

    public class UpdateStudentRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}

