using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic.CompilerServices;

namespace DotnetWeather.Models;

public class Weather
{
    public int CityId { get; set; }
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    public WeatherType WeatherType { get; set; }
    public int? Temperature { get; set; }
}