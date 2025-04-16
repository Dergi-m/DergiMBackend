namespace DergiMBackend.Models.Dtos
{
	public class AccessTokenDto
	{
		public string AccessToken { get; set; }
		public DateTime Expiration { get; set; }
	}
}
