
using Microsoft.AspNetCore.Mvc;
using TmsApi.DTO;
using TmsApi.Service;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")] //base route for all actions in this controller
[Tags("Courses")] //groups all actions in this controller under the "Courses" tag in Swagger UI
[Produces("application/json")] //specifies that all actions in this controller produce JSON responses
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)] //specifies that all actions in this controller return 200 OK responses by default
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    // GET /api/courses/{id}
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]//specifies that this action returns a 200 OK response with a CourseDetailDto body
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]//specifies that this action returns a 404 Not Found response if the course is not found
    [EndpointSummary("Get course by ID")]//specifies a summary for this endpoint in Swagger UI
    [EndpointDescription("Returns the details of a specific TMS course by its ID, including links to related resources.")]//specifies a description for this endpoint in Swagger UI
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        if (course is null)
            return NotFound();          // 404 not found

       // return Ok(course);              // 200 ok with course data
        // Build links using LinkGenerator (derived from routing metadata)
        var selfPath = linkGenerator.GetPathByName(
            HttpContext, nameof(GetCourseById), new { id })!;

        var enrollmentsPath = linkGenerator.GetPathByAction(
            HttpContext,
           "listCourseEnrollments",
            "Enrollments",
            new { courseId = id })!;

        var links = new List<LinkDto>
        {
            new(selfPath, "self", "GET"),
            new(selfPath, "update", "PUT"),
            new(selfPath, "delete", "DELETE"),
            new(enrollmentsPath, "enrollments", "GET")
        };

        // Conditional link: only show "enroll" if course has space
        if (course.EnrollementCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsPath, "enroll", "POST"));
        }

        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollementCount,
            Links = links
        };

        return Ok(detailDto);
    }
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]//specifies that this action returns a 200 OK response with a PagedResponse<CourseResponseDto> body
    [EndpointSummary("List courses with pagination")]//specifies a summary for this endpoint in Swagger UI
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]//specifies a description for this endpoint in Swagger UI

    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    // POST /api/courses
    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]//specifies that this action returns a 201 Created response with a CourseResponseDto body
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]//specifies that this action returns a 400 Bad Request response if the request body is invalid
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]//specifies that this action returns a 409 Conflict response if the course code already exists
    [EndpointSummary("Create a new course")]//specifies a summary for this endpoint in Swagger UI
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]//specifies a description for this endpoint in Swagger UI
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with the code '{request.Code}' already exists or registered.",
                Status = StatusCodes.Status409Conflict
            }); // 409 Conflict
        }
            
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(
            nameof(GetCourseById),      // Name of the GET action
            new { id = result.Id },      // Route values for Location header
            result                       // Response body
        );                              // 201 Created
    }
}