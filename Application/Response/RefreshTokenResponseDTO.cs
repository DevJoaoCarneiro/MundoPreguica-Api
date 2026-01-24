using System.Text.Json.Serialization;

namespace Application.Response
{
    public class RefreshTokenResponseDTO
    {

        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DataToken Data { get; set; } = new DataToken();
    }

    public class DataToken
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

}
