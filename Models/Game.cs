namespace MCT.Functions.Models;

public class Game
{
    [JsonProperty("groep")]
    public string Groep { get; set; }

    [JsonProperty("BoefLatitude")]
    public float Latitude { get; set; }

    [JsonProperty("BoefLongtitude")]
    public float Longtitude { get; set; }

    [JsonProperty("spelcode")]
    public string SpelCode { get; set; }

    [JsonProperty("aantalSpelers")]
    public int AantalSpelers { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}