namespace TmsApi.Entities;

public class Enrollment
{
    public int Id{get; set;}
    public int StudentId{get; set;}
    public int CourseId{get; set;}
    public decimal? GPA{get; set;}
    public DateTime EnrolledAt {get; set;} = DateTime.UtcNow;
    public int Year {get; set;}
    public bool IsArchived {get; set;} = false;//soft delete flag
    //navigation properties back to entities
    public Student Student {get; set;} = null!;
    public Course Course{get; set;} = null!;


}