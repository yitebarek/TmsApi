using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;




namespace TmsApi.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly DbContext _context;

        public ReportsController(DbContext context)
        {
            _context = context;
        }

        public async Task<int> GetActiveHighGpaStudentCount(DbContext context)
        {
            // how many students are active and have a GPA of 3.0 or higher?
            var count = await context.Set<Student>().Where(s => s.IsActive && s.GPA >= 3.0m).CountAsync();
            return count;
        }
        public async Task<string> GetCourseWithMostEnrollments(DbContext context)
        {
            // which course has the most enrollments?
            var list = await context.Set<Course>().Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .FirstOrDefaultAsync();
            return list?.Title ?? string.Empty;
        }
        public async Task<decimal> GetAverageGpaOfActiveStudents(DbContext context)
        {
            // what is the average GPA of all active students?
            var list = await context.Set<Enrollment>()
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGpa = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();
            var averageGpa = list.Average(c => c.AverageGpa);
            return averageGpa;
        }
        public async Task<string> GetStudentWithZeroEnrollments(DbContext context)
        {
            //using subquery to find a student with zero enrollments
            var list = await context.Set<Student>()
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .FirstOrDefaultAsync();
            return list ?? string.Empty;
            }
        public async Task<string> GetStudentWithZeroEnrollmentsUsingLeftJoin(DbContext context)
        {
            //using left join to find a student with zero enrollments
            var list2 = await context.Set<Student>()
                 .LeftJoin(context.Set<Enrollment>(), s => s.Id,
                  e => e.StudentId, 
                  (s, e) => new { Student = s, Enrollment = e })
                  .Where(se => se.Enrollment == null)
                  .Select(se => se.Student.Name)
                  .ToListAsync();
            return list2.FirstOrDefault() ?? string.Empty;
            
        }

        //pagination for students and courses
        [HttpGet("Students")]
        public async Task<IActionResult> GetStudents(int page =1, CancellationToken cancellationToken = default)
        {
            int pageSize = 20;
            var students = await _context.Set<Student>()
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

               return Ok(students); 
        }
        [HttpGet("Courses")]
        public async Task<IActionResult> GetCourses(int page = 1, CancellationToken cancellationToken = default)
        {
            var courses = await _context.Set<Enrollment>()
                .GroupBy(c => c.Course.Title)
                .Select(g => new
                {
                    Title = g.Key,
                    EnrollmentCount = g.Count()
                })
                .OrderByDescending(c => c.EnrollmentCount)
                .Take(5)
                .ToListAsync(cancellationToken);

            return Ok(courses);
        }

        //projection concept M5 exercise 7 Part B fix with shaping
        [HttpGet("optimized")]
        public async Task<IActionResult> Optimized(CancellationToken cancellationToken)
        {
            var report = await _context.Set<Student>()
                .AsNoTracking()
                .Select(s => new
                {
                    s.Name,
                    EnrollmentCount = s.Enrollments.Count
                })
                .ToListAsync(cancellationToken);

            return Ok(report);
        }
        
        
    }
}