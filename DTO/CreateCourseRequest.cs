using System.ComponentModel.DataAnnotations;
namespace TmsApi.DTO;

public record CreateCourseRequest{
    [Required, RegularExpression(@"^[A-Z]{3}-\d{3}$", 
    ErrorMessage = "Course code must be 3 uppercase letters followed by a hyphen and 3 digits.")]
    public required string Code {get; init;}
    [Required, MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public required string Title {get; init;}
   
    [Range(1, 200, ErrorMessage = "Max capacity must be between 1 and 200.")]
    public required int MaxCapacity {get; init;}
};