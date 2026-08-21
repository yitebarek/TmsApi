using Microsoft.AspNetCore.Mvc;
using TmsApi.Data;

namespace TmsApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDefferred()
    {
        Console.WriteLine("\n>>> step 1: building the query object (no database content)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> step 2: Appending a sorting clause ...");
        var orderedQuery = query.OrderBy(s => s.Name);
        Console.WriteLine(">>> step 3: Materializing query into C# list...");
        var result = orderedQuery.ToList();
        Console.WriteLine(">>> step 4: Materialization finished. list populated .\n");
        return Ok(result);
    }



private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }
    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: running non-translatable query...");
        try
        {
            var Students = context.Students.Where(s => IsHonorRoll(s.GPA)).ToList();
            return Ok(Students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: (ex.message)\n");
            return BadRequest(new {Message = ex.Message});
        }
    }
}