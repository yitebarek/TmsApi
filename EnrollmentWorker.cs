using TmsApi.Service;

public class EnrollmentWorker(IServiceScopeFactory ScopeFactory)
{
    public void ProcessBatch()
    {
     using var scope = ScopeFactory.CreateScope();
     var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
     svc.DoWork();
    }
}

public static class EnrollmentServiceExtensions
{
    public static void DoWork(this IEnrollmentService service)
    {
        throw new System.NotImplementedException();
    }
}