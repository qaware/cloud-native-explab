using System.ComponentModel.DataAnnotations;

namespace DotnetWeather.Models;

public class City
{
    public int Id { get; set; }
    [MaxLength(30)]
    public string Name { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
}