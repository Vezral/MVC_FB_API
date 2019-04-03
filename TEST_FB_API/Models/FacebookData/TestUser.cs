using Newtonsoft.Json;
using System.Collections.Generic;

namespace TEST_FB_API.Models.FacebookData
{
    public class TestUserQueryResult
    {
        [JsonProperty("data")]
        public List<TestUser> TestUserList { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class TestUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class TestUserProfile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("birthday")]
        public string Birthday { get; set; }

        public string BirthdayMYFormat { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("hometown")]
        public string Hometown { get; set; }
    }
}