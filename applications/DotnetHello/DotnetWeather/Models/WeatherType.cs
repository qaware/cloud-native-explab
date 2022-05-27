namespace DotnetWeather.Models;

public struct WeatherType
{
    public static readonly WeatherType SUNNY = new WeatherType("Sunny", "Resources/icons/sunny.png");
    public static readonly WeatherType CLOUDY = new WeatherType("Cloudy", "Resources/icons/cloudy.png");
    public static readonly WeatherType RAINING = new WeatherType("Raining", "Resources/icons/raining.png");
    public static readonly WeatherType SNOW = new WeatherType("Snow", "Resources/icons/snow.png");
    public static readonly WeatherType NOT_AVAILABLE = new WeatherType("", "Resources/icons/error.png");

    private WeatherType(string name, string iconPath)
    {
        this.name = name;
        this.iconPath = iconPath;
    }

    public readonly string name;
    public readonly string iconPath;
}