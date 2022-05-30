using DotnetWeather.Models;

namespace DotnetWeather.Data;

public static class DbInitializer
{
    public static void Initialize(DotnetWeatherContext context)
    {
        context.Database.EnsureCreated();

        if (context.Weather.Any())
        {
            return;
        }

        var cities = new City[]
        {
            new City {Lat = 48.137154, Lon = 11.576124, Name = "Munich"},
            new City {Lat = 50.073658, Lon = 14.418540, Name = "Prague"},
            new City {Lat = 51.507359, Lon = -0.136439, Name = "London"},
            new City {Lat = 52.531677, Lon = 13.381777, Name = "Berlin"},
            new City {Lat = -33.447487, Lon = -70.673676, Name = "Santiago"},
        };

        foreach (City c in cities)
        {
            context.City.Add(c);
        }
        context.SaveChanges();

        var weathers = new Weather[]
        {
            new Weather{CityId = 1, Date = DateTime.Parse("2022-05-30"), Temperature = 16, WeatherType = WeatherType.CLOUDY},
            new Weather{CityId = 1, Date = DateTime.Parse("2022-05-31"), Temperature = 22, WeatherType = WeatherType.RAINING},
            new Weather{CityId = 1, Date = DateTime.Parse("2022-06-01"), Temperature = 20, WeatherType = WeatherType.RAINING},
            new Weather{CityId = 5, Date = DateTime.Parse("2022-05-30"), Temperature = 19, WeatherType = WeatherType.SUNNY},
        };

        foreach (Weather w in weathers)
        {
            context.Weather.Add(w);
        }
        context.SaveChanges();
    }
}