using TmsApi.DTO;

namespace TmsApi.Service;

public interface IEnrollmentService
{
        Task<EnrollmentRecord> EnrollAsync(string StudentID , string courceCode); 
        Task<EnrollmentRecord?> GetByIdAsync(string id);
        Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
        Task<bool> DeleteAsync(string id);

        Task<EnrollmentResponseDto> GetByIdAsync(int courseId, int id, CancellationToken ct);
        Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
        Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
}