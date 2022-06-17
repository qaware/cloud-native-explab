namespace DotnetWeather.Models;

public class WeatherType
{
    private static Dictionary<string, WeatherType> serializeDict = new ();

    public static readonly WeatherType SUNNY = new WeatherType("Sunny", "icons/sunny.png");
    public static readonly WeatherType CLOUDY = new WeatherType("Cloudy", "icons/cloudy.png");
    public static readonly WeatherType RAINING = new WeatherType("Raining", "icons/raining.png");
    public static readonly WeatherType SNOW = new WeatherType("Snow", "icons/snow.png");
    public static readonly WeatherType STORM = new WeatherType("Thunderstorm", "icons/storm.png");
    public static readonly WeatherType NOT_AVAILABLE = new WeatherType("(not available)", "icons/error.png");
    // icons by https://www.iconfinder.com/iconsets/the-weather-is-nice-today  (CC BY-NC 3.0)

    private WeatherType(string name, string iconPath)
    {
        this.Name = name;
        this.IconPath = iconPath;

        if (serializeDict.ContainsKey(name))
        {
            throw new Exception($"Weather with this name already exists: {name}");
        }
        serializeDict[name] = this;
    }

    public string Name { get; }
    public string IconPath { get; }

    public static string Serialize(WeatherType weatherType)
    {
        return weatherType.Name;
    }

    public static WeatherType Deserialize(string s)
    {
        if (!serializeDict.ContainsKey(s))
        {
            throw new Exception($"Unknown weather name: {s}");
        }
        return serializeDict[s];
    }

    public static List<WeatherType> GetAllWeatherTypes()
    {
        List<WeatherType> allTypes = serializeDict.Values.ToList();
        allTypes.Remove(WeatherType.NOT_AVAILABLE);
        return allTypes;
    }
}