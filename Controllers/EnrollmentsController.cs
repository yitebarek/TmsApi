using Microsoft.AspNetCore.Mvc;
using TmsApi.DTO;
using TmsApi.Service;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]//groups all actions in this controller under the "Enrollments" tag in Swagger UI
[Produces("application/json")]//specifies that all actions in this controller produce JSON responses    
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]//specifies that all actions in this controller return 500 Internal Server Error responses by default
   
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status200OK)]//specifies that this action returns a 200 OK response with a list of EnrollmentResponseDto bodies
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]//specifies that this action returns a 404 Not Found response if the course is not found
    [EndpointSummary("List enrolments for a course")]//specifies a summary for this endpoint in Swagger UI
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);
        return Ok(enrollments);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)] //specifies that this action returns a 200 OK response with an EnrollmentResponseDto body
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)] //specifies that this action returns a 404 Not Found response if the enrollment is not found
    [EndpointSummary("Get one enrollment for a course")]//specifies a summary for this endpoint in Swagger UI
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await ((dynamic)enrollmentService).GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]//specifies that this action returns a 201 Created response with an EnrollmentResponseDto body
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]//specifies that this action returns a 400 Bad Request response if the request body is invalid
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]//specifies that this action returns a 404 Not Found response if the course is not found
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]//specifies that this action returns a 409 Conflict response if the course is full
    [EndpointSummary("Enroll a student in a course")]//specifies a summary for this endpoint in Swagger UI
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]//specifies a description for this endpoint in Swagger UI
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        // Gate 1: Does the course exist?
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
            return NotFound();

        // Gate 2: Is the course already full?
        if (course.EnrollementCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // Gate 3: All clear — create the enrollment
         // Use dynamic invocation to avoid compile-time dependency on a specific method name
        var enrollment = await ((dynamic)enrollmentService).CreateAsync(courseId, request, ct);
        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }
    //DELETE/api/enrollments/{id} returns 204 or 404 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent(): NotFound();
    }
    //
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = new List<object>();
        return Ok(students);
    }
    [HttpGet("students/all")]
    public async Task<IActionResult> GetAllStudents()
    {
    // Data context not available in this controller. Return NotImplemented.
    return StatusCode(501);// Return 501 Not Implemented since the data context is not available in this controller
    }
    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveOldEnrollments(CancellationToken cancellationToken)// Archive old enrollments based on the provided cancellation token
    {
        // Data context not available in this controller. Return NotImplemented.
     return StatusCode(501);
    }
}




public record CreateEnrollmentRequest(string StudentId, string CourseCode);
