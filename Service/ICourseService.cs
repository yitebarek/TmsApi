using TmsApi.DTO;
namespace TmsApi.Service;

public interface ICourseService //interface for course service
{
Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);// Check if a course with the given code already exists
Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);// Check if a course with the given code already exists
Task<bool> CodeExistsAsync(string code, CancellationToken ct);// Check if a course with the given code already exists
Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);//
}