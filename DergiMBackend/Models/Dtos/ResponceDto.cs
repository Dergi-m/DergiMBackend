using System.Net;

namespace DergiMBackend.Models.Dtos
{
	public class ResponseDto
	{
		public object? Result { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; }
		public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
	}
}
