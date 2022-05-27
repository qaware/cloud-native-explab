using DotnetWeather.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetWeather.Controllers;

public class WeatherController : Controller
{
    public RedirectToActionResult Find(string city = "London", string date = "")
    {
        DateTime dateT = DateTime.Today;
        DateTime.TryParse(date, out dateT);

        int cityID = 123456;                                       //TODO: find id in database
        int day = 0;                                               //TODO: count day from dateT

        return new RedirectToActionResult("Index", "Weather", new {cityID, day});
    }

    public IActionResult Index(int cityID = 123456, int day = 0)
    {
        string city = "London";                                     //TODO: get city from database
        DateTime date = DateTime.Today + TimeSpan.FromDays(day);
        string dateString = date.ToString("dd. MM.");
        WeatherType weather = WeatherType.RAINING;                  //TODO: get weather from database for given date
        int temperature = 19;                                       //TODO: get temperature from database for given date

        return View(new WeatherModel(city, weather, temperature, dateString));
    }
}