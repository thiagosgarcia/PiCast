using System.Collections.Generic;

namespace PiCast.Model;

public class Wind
{
    public double speed { get; set; }
    public int deg { get; set; }
    public double gust { get; set; }
}

public class EmailTemplateData
{
    public string subject { get; set; }
    public EmailData current { get; set; }
    public List<EmailData> forecast { get; set; }
}

public class EmailData
{
    public string stringDate { get; set; }
    public string stringDateTime { get; set; }
    public string city { get; set; }
    public string weatherDescription { get; set; }
    public string weatherIcon { get; set; }
    public string temperature { get; set; }
    public string feelsLike { get; set; }
    public string tempMin { get; set; }
    public string tempMax { get; set; }
    public string pressure { get; set; }
    public string humidity { get; set; }
    public string windSpeed { get; set; }
    public string windDirection { get; set; }
    public string stringWindDirection { get; set; }
    public string sunrise { get; set; }
    public string sunset { get; set; }
    
// "stringDate": "22 de junho de 2022",
// "stringDateTime": "22/06/2022 06:00",
// "city": "Saquarema",
// "weatherDescription": "céu limpo",
// "weatherIcon": "01n",
// "temperature": 20,
// "feelsLike": 21,
// "tempMin": 20.09,
// "tempMax": 20.09,
// "pressure": 1016,
// "humidity": 75,
// "windSpeed": 3.77,
// "windDirection": 50,
// "stringWindDirection": ""
}