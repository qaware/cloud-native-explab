using System.ComponentModel.DataAnnotations;

namespace DotnetWeather.Models;

public class Weather
{
    public int CityId { get; set; }
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    public WeatherType WeatherType { get; set; }
    public int? Temperature { get; set; }

    public static Weather GetNotAvailable(int cityId, DateTime date)
    {
        return new Weather{CityId = cityId, Date = date, WeatherType = WeatherType.NOT_AVAILABLE, Temperature = null};
    }
}