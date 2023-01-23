namespace MCT.Functions.Models;

public class Game
{
	[JsonProperty("timeLimit")]
	public DateTime TimeLimit { get; private set; }

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

	public long Duration
	{
		set
		{
			List<long> timeLimits = new List<long> { 15 * 60, 60 * 60, 120 * 60, 240 * 60 };
			DateTime end = DateTime.Now.AddSeconds(value);
			if (timeLimits.Contains(value)) TimeLimit = end;
		}
	}

	[JsonProperty("startTime")]
	public DateTime StartTime { get { return DateTime.Now; } }

	[JsonProperty("endTime")]
	public DateTime EndTime { get; set; }

	[JsonProperty("aantalSpelers")]
	public int AantalSpelers { get; set; }

	[JsonProperty("winner")]
	public bool Winner { get; set; }



	[JsonProperty("id")]
	public string Id { get; set; }


}