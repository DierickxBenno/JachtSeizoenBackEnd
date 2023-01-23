namespace MCT.Functions.Models
{
	public class GameResults
	{
		//private
		private string _GameId = "0000-0000";


		//public
		[JsonProperty("spelcode")]
		public string GameId { get; set; }

		[JsonProperty("winner")]
		public bool Winner { get; set; }

	}
}
