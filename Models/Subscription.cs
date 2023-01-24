namespace MCT.Functions.Models
{
	public class Subscription
	{
		// tcp server
		//  client id
		public List<String> Topics
		{
			get
			{
				List<string> topics = new List<String>();
				topics.Add("hetJachtSeizoen/gameResults");
				topics.Add("hetJachtSeizoen/deviceAvailibility");
				return topics;
			}
		}

	}
}
