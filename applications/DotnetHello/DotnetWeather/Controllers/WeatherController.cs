using System.Text.Json;
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
    
    public async Task<RedirectToActionResult> Find(string city, string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime dateT))
        {
            dateT = DateTime.Today;
        }
        int day = (dateT - DateTime.Today).Days;

        var foundCities = await _context.City.Where(c => c.Name.ToLower() == city).ToListAsync();
        if (foundCities.Count == 0)
        {
            List<City> alternatives = new List<City>();            //TODO: find similar names in database
            return new RedirectToActionResult("CityNotFound", "Weather", alternatives);
        }

        else if (foundCities.Count == 1)
        {
            return new RedirectToActionResult("Index", "Weather", new {cityId = foundCities[0].Id, day});
        }

        else
        {
            return new RedirectToActionResult("CityNotFound", "Weather", foundCities);
        }
    }

    public async Task<IActionResult> Index(int cityId = 3, int day = 0)
    {
        City? city = await _context.City.FindAsync(cityId);
        DateTime date = DateTime.Today + TimeSpan.FromDays(day);
        string dateString = date.ToString("dd. MM.");
        Weather? weather = await _context.Weather.FindAsync(city.Id, date);

        return View(new WeatherModel(city.Name, weather.WeatherType, weather.Temperature??0, dateString));
    }

    public string CityNotFound(List<City> alternatives)
    {
        return "city not found";
    }
}