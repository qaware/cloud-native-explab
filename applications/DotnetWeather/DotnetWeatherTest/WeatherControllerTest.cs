using DotnetWeather.Controllers;
using DotnetWeather.Models;
using Microsoft.AspNetCore.Mvc;
using static DotnetWeatherTest.Utils;

namespace DotnetWeatherTest;

[TestClass]
public class WeatherControllerTest
{
    [TestMethod]
    public async Task FindRedirectValid()
    {
        var context = GetDatabaseContext();
        var weatherController = new WeatherController(context);

        var redirect = await weatherController.Find("Munich", "2022-06-03");

        Assert.AreEqual("Index", redirect.ActionName);
        Assert.AreEqual("Weather", redirect.ControllerName);
    }

    [TestMethod]
    public async Task FindRedirectInvalid()
    {
        var context = GetDatabaseContext();
        var weatherController = new WeatherController(context);

        var redirect = await weatherController.Find("nonexistentCity", "2022-06-03");

        Assert.AreEqual("CityNotFound", redirect.ActionName);
        Assert.AreEqual("Weather", redirect.ControllerName);
    }

    [TestMethod]
    public async Task ShowWeatherTest()
    {
        var context = GetDatabaseContext();
        var weatherController = new WeatherController(context);

        City city = context.City.First();
        var result = (await weatherController.Index(city.Id, 0));
        
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult) result;
        Assert.IsNotNull(viewResult.Model);
        Assert.IsInstanceOfType(viewResult.Model, typeof(WeatherViewModel));
        var model = (WeatherViewModel) viewResult.Model;
        Assert.AreEqual(city, model.City);
        Assert.AreEqual(DateTime.Today, model.Weather.Date);
    }
}