using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Filters;
using TmsApi.Persistence;
using TmsApi.Service;




var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions,TrainingAuthHandler>
("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<ICourseService, CourseService>();//scoped service for course service
builder.Services.AddScoped<EnrollmentService>();//scoped service for enrollment service
//builder.Services.AddScoped<TmsDbContext, TmsDbContext>();
builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());
builder.Host.UseDefaultServiceProvider(Options =>
{
    Options.ValidateScopes = true;
    Options.ValidateOnBuild = true;
});

// binding payment option
builder.Services.AddOptions<PaymentOptions>()
            .BindConfiguration("Payments")
            .ValidateDataAnnotations()
            .ValidateOnStart();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

var app = builder.Build();

//development only features 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}
                
//this is a correct order
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//seed test data at start up
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate();
    if (!context.Students.Any())
    {
        var Students = new List<Student>
        {
            new (){RegistrationNumber ="TMS-2026-0001", Name = "Alice Smith" , GPA = 3.8m , IsActive = true},
            new (){RegistrationNumber ="TMS-2026-0002", Name = "Bob jones" , GPA = 2.9m , IsActive = true},
            new (){RegistrationNumber ="TMS-2026-0003", Name = "Charlie Brown" , GPA = 3.4m , IsActive = true},
            new (){RegistrationNumber ="TMS-2026-0004", Name = "Diana prince" , GPA = 3.9m , IsActive = true},
            new (){RegistrationNumber ="", Name = "Evan Wright" , GPA = 2.5m , IsActive = true}};
            context.Students.AddRange(Students);

        var courses = new List<Course>
        {
            new (){Code ="CS-101", Title = "Introduction to Computer Science" , MaxCapacity = 30},
            new (){Code ="CS-201", Title = "Data Structures and Algorithms" , MaxCapacity = 25},
            new (){Code ="MAT-101", Title = "Calculus I" , MaxCapacity = 40}};
            context.Courses.AddRange(courses);
            context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new (){StudentId = Students[0].Id, CourseId = courses[0].Id, GPA = 4.0m},
            new (){StudentId = Students[0].Id, CourseId = courses[1].Id, GPA = 3.6m},
            new (){StudentId = Students[1].Id, CourseId = courses[0].Id, GPA = 2.8m},
            new (){StudentId = Students[3].Id, CourseId = courses[2].Id, GPA = 3.9m}};
            context.Enrollments.AddRange(enrollments);
            context.SaveChanges();    
      }
 }

app.MapGet("/api/assessments/results",() => Results.Ok (new
{
    CourseCode = "CS-101",
    StudentId = "S-001",
    LetterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke",(EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapGet("/api/error", () =>
{
   throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run();

namespace TmsApi
{
    public class PaymentOptions
    {
        public string? Provider { get; set; }
    }
}

