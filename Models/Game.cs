namespace MCT.Functions.Models;

public class Game
{
    private long _TimeLimit = 0;

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

    [JsonProperty("timeLimit")]
    public long TimeLimit
    {
        get { return _TimeLimit; }

        set
        {
            List<long> timeLimits = new List<long> { 15 * 60, 60 * 60, 120 * 60, 240 * 60 };
            if (timeLimits.Contains(value)) _TimeLimit = value;
        }
    }

    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    public DateTime EndTime { get; set; }

    [JsonProperty("aantalSpelers")]
    public int AantalSpelers { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }


}