using DotnetWeather.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetWeatherTest;

public static class Utils
{
    public static DotnetWeatherContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<DotnetWeatherContext>()
            .UseInMemoryDatabase(databaseName: "MockDatabase")
            .Options;

        var context = new DotnetWeatherContext(options);
        DbSeed.Seed(context);

        return context;
    }
}