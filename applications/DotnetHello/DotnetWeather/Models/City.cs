using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetWeather.Models;

public class City
{
    public int Id { get; set; }
    [MaxLength(30)]
    public string Name { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    [NotMapped]
    public List<City> SimilarCities { get; set; }
}