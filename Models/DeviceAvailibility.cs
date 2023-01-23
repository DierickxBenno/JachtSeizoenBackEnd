namespace MCT.Functions.Models
{
	public class DeviceAvailibility
	{
		[JsonProperty("type")]
		private string type;

		public string Type
		{
			get { return type; }

			set
			{
				if (type == "request" || type == "response") type = value;
			}
		}

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("available")]
		public bool Available { get; set; }
	}
}
