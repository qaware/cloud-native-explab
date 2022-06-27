using DotnetWeather.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetWeather.Data;

public class DotnetWeatherContext : DbContext
{
    public DotnetWeatherContext(DbContextOptions<DotnetWeatherContext> options) : base(options) {}
    
    public DbSet<City> City { get; set; }
    
    public DbSet<Weather> Weather { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Weather>().HasKey(nameof(DotnetWeather.Models.Weather.CityId), nameof(DotnetWeather.Models.Weather.Date));
        modelBuilder.Entity<Weather>().Property(e => e.WeatherType).HasConversion(
            v => WeatherType.Serialize(v), 
            v => WeatherType.Deserialize(v));
    }
}