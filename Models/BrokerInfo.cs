namespace MCT.Functions.Models
{
	public class BrokerInfo
	{
		public string ClientId { get { return Guid.NewGuid().ToString(); } }
		public string BrokerAddress { get; set; }
		public int BrokerPort { get; set; }
	}
}
