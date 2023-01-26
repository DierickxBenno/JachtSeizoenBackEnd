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
    public string GameId { get; set; }

    [JsonProperty("inProgress")]
    public bool GameInProgress { get; set; }

    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    public DateTime EndTime { get; set; }

    [JsonProperty("startTimeJs")]
    public long StartTimeJs { get; set; }

    [JsonProperty("endTimeJs")]
    public long EndTimeJs { get; set; }

    [JsonProperty("aantalSpelers")]
    public int AantalSpelers { get; set; }

    [JsonProperty("jagersWinnen")]
    public bool Winner { get; set; }

    [JsonProperty("startSpelkeuze")]
    public bool startSpelkeuze { get; set; }

    [JsonProperty("startSpel")]
    public bool startSpel { get; set; }

    [JsonProperty("durationGame")]
    public int durationGame { get; set; }

    [JsonProperty("durationLocation")]
    public int durationLocation { get; set; }

    [JsonProperty("beginJager")]
    public bool beginJager { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}