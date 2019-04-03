using Newtonsoft.Json;

namespace TEST_FB_API.Models.FacebookData
{
    public class FBAppToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}