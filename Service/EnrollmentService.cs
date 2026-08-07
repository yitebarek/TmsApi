

using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTO;
using TmsApi.Entities;

namespace TmsApi.Service;
//--the memory implimentation ---
public class EnrollmentService
{
    private readonly Dictionary <string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;
    
    public EnrollmentService (ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }
    public Task<EnrollmentRecord> EnrollAsync(string StudentID , string courceCode)
    {
        //check duplicate enrollment 
        var existing = _store.Values.FirstOrDefault( e=> 
        e.studentID == StudentID && 
        e.courseCode == courceCode);
        if (existing is not null)
        {
            //warninig: duplicate enrollment attempt 
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",StudentID,courceCode,existing.id);
             return Task.FromResult(existing);// Return the existing enrollment record if a duplicate is found
        }
        var id = Guid.NewGuid().ToString("N") [..8];
        var record = new EnrollmentRecord(id, StudentID, courceCode, DateTime.UtcNow);
        _store[id] = record;
        _logger.LogInformation("Enrolled {studentId} in {courseCode} record {EnrollmentId}", StudentID, courceCode, record.id);
        return Task.FromResult(record);// Return the newly created enrollment record
    } 

    
    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if (record is null)
        {
            //warnings: record not found
            _logger.LogWarning("enrollment {EnrollmentId} not found", id);
        }
        return Task.FromResult(record);// Return the enrollment record if found, or null if not found
    }
    
    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);// Return a read-only list of all enrollment records
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
        {
            //information: record deleted successfully 
            _logger.LogInformation("deleted Enrollment {EnrollmentId}",id);// Log the successful deletion of the enrollment record
        }
        else
        {
            //information: record wasnot found 
            _logger.LogWarning("delated failed enrollment {EnrollmentId} not found",id);
        }
        return Task.FromResult(removed);// Return true if the record was removed, false if it was not found
    }

    public Task GetByIdAsync(int courseID, int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /*   public Task<EnrollmentResponseDto> GetByIdAsync(int courseId, int id, CancellationToken ct)
       {
           throw new NotImplementedException();
       }

       public Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
       {
           throw new NotImplementedException();
       }*/
}

public class EnrollmentServices(TmsDbContext context, ILogger<EnrollmentServices> logger) : IEnrollmentService
{
    public async Task<EnrollmentResponseDto> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.StudentId, e.CourseId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

        return enrollment ?? throw new InvalidOperationException($"Enrollment {id} for course {courseId} was not found.");
    }
     public Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Student {StudentId} enrolled in course {CourseId} (enrollment {EnrollmentId})",
            enrollment.StudentId, enrollment.CourseId, enrollment.Id);

        return await GetByIdAsync(courseId, enrollment.Id, ct);
    }

    public Task<EnrollmentRecord> EnrollAsync(string StudentID, string courceCode)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }

    Task<EnrollmentResponseDto> IEnrollmentService.GetByIdAsync(int courseID, int id, CancellationToken ct)
    {
        return GetByIdAsync(courseID, id, ct);
    }
}
// --- the data shape ---
public record EnrollmentRecord
(
     string id,
     string studentID,
     string courseCode,
     DateTime EnrolledAt
);

public class TmsDatabaseException(string message) : Exception(message);
