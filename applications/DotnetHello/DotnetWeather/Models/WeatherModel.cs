namespace DotnetWeather.Models;

public class WeatherModel
{
    public readonly string city;
    public readonly WeatherType weather;
    public readonly int temperature;
    public readonly string date;

    public WeatherModel(string city, WeatherType weather, int temperature, string date)
    {
        if (weather.Equals(WeatherType.NOT_AVAILABLE))
        {
            weather = WeatherType.SUNNY;
        }
        this.city = city;
        this.weather = weather;
        this.temperature = temperature;
        this.date = date;
    }
}