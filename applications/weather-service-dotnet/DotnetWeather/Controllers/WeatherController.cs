using DotnetWeather.Data;
using DotnetWeather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetWeather.Controllers;

public class WeatherController : Controller
{
    private readonly DotnetWeatherContext _context;

    public WeatherController(DotnetWeatherContext context)
    {
        _context = context;
    }
    
    [ServiceFilter(typeof(NeedsDatabase))]
    public async Task<RedirectToActionResult> Find(string city, string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime dateT))
        {
            dateT = DateTime.Today;
        }
        int day = (dateT - DateTime.Today).Days;

        if (city == null)
        {
            city = "Munich";
        }

        var foundCities = await _context.City.Where(c => c.Name.ToLower().Equals(city.ToLower())).ToListAsync();
        if (foundCities.Count == 1)
        {
            return new RedirectToActionResult("Index", "Weather", new {cityId = foundCities[0].Id, day});
        }
        else
        {
            return new RedirectToActionResult("CityNotFound", "Weather", new {city});
        }
    }

    [ServiceFilter(typeof(NeedsDatabase))]
    public async Task<IActionResult> Index(int cityId = 1, int day = 0)
    {
        City? city = await _context.City.FindAsync(cityId);

        DateTime date = DateTime.Today + TimeSpan.FromDays(day);
        Weather weather = await _context.Weather.FindAsync(cityId, date) ?? Weather.GetNotAvailable(cityId, date);

        return View(new WeatherViewModel {City = city, Weather = weather});
    }

    [ServiceFilter(typeof(NeedsDatabase))]
    public async Task<IActionResult> CityNotFound(string city)
    {
        List<City> alternatives = await _context.City.Take(5).ToListAsync();
        City fake = new City {Name = city, SimilarCities = alternatives};
        return View(fake);
    }
}