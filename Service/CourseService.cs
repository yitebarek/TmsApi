using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using TmsApi.Data;
using TmsApi.DTO;

namespace TmsApi.Service;


public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);// Get a course by its ID and return it as a CourseResponseDto, or null if not found

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);// Add the new course to the database context
        await context.SaveChangesAsync(ct);// Save the new course to the database

        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);// Log the creation of the course with its ID and code

        return (await GetByIdAsync(course.Id, ct))!;
    }
        //insted of trying toinsert and hoping it works, we ask the database first if the code exists, and if it does we return false, otherwise we insert the new course and return true
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct); // Check if a course with the given code already exists

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        //step 1 : start with no-tracking querey
        var query = context.Courses.AsNoTracking();
        //step 2: apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }
        //step 3 : count before paging
        var totalCount = await query.CountAsync(ct);
        // step 4 : Apply ordering
         query = request.OrderBy switch
        {
            "Code" => request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title)
        };
         // Step 5: Page, project, and materialize
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);
             // Step 6: Wrap in PagedResponse
           return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
        
    
