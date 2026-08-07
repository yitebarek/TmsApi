namespace TmsApi.DTO;
public record CourseResponseDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollementCount
);
