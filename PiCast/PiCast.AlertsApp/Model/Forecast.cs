using System.Collections.Generic;

namespace PiCast.Model;

public class Forecast
{
    public string cod { get; set; }
    public int message { get; set; }
    public int cnt { get; set; }
    public List<WeatherList> list { get; set; }
    public City city { get; set; }
}