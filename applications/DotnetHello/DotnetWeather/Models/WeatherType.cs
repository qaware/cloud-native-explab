namespace DotnetWeather.Models;

public class WeatherType
{
    private static Dictionary<string, WeatherType> serializeDict = new ();

    public static readonly WeatherType SUNNY = new WeatherType("Sunny", "Resources/icons/sunny.png");
    public static readonly WeatherType CLOUDY = new WeatherType("Cloudy", "Resources/icons/cloudy.png");
    public static readonly WeatherType RAINING = new WeatherType("Raining", "Resources/icons/raining.png");
    public static readonly WeatherType SNOW = new WeatherType("Snow", "Resources/icons/snow.png");
    public static readonly WeatherType NOT_AVAILABLE = new WeatherType("(not available)", "Resources/icons/error.png");

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