using System.ComponentModel.DataAnnotations;

namespace TmsApi.DTO;

public record EnrollmentResponseDto(
    int Id,
    int StudentId,
    int CourseId,
    DateTime EnrolledAt
);
