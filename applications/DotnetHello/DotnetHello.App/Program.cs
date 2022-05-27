namespace DotnetHello.App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouter(routeBuilder =>
        {
            routeBuilder.MapGet("", context =>
            {
                context.Response.Redirect("index.html", permanent: false);
                return Task.FromResult(0);
            });
        });

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/sum", (int? n1, int? n2) => n1 + n2);

        app.Run();
    }
}