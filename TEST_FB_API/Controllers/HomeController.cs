using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TEST_FB_API.Controllers
{
    internal class JsonReturnObj
    {
        public string[] data { get; set; }
        public bool isSuccess { get; set; }
        public string errorMsg { get; set; }
    }

    internal class TestUserQueryResult
    {
        [JsonProperty("data")]
        public List<TestUser> testUserList { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }
    }

    internal class TestUser
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("access_token")]
        public string access_token { get; set; }
    }

    internal class TestUserProfile
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("birthday")]
        public string birthday { get; set; }
    }

    internal class FBAppToken
    {
        [JsonProperty("access_token")]
        public string access_token { get; set; }
        [JsonProperty("token_type")]
        public string token_type { get; set; }
    }

    [RequireHttps]
    public class HomeController : Controller
    {
        private const string APP_ID = "485102995357580";
        private const string APP_SECRET = "28e8e11afc6c7bca1419b9b3a0b57fd2";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult GetTestUsersBirthdays()
        {
            JsonReturnObj jsonReturn = new JsonReturnObj();

            try
            {
                //Get App Token
                string appTokenURL = $"https://graph.facebook.com/oauth/access_token?client_id={APP_ID}&client_secret={APP_SECRET}&grant_type=client_credentials";
                FBAppToken appTokenObj = JsonConvert.DeserializeObject<FBAppToken>(Get(appTokenURL));
                string access_token = appTokenObj.access_token;

                //Get list of test users
                string testUserURL = $"https://graph.facebook.com/v3.2/{APP_ID}/accounts/test-users?access_token={access_token}";
                string testUsersJson = Get(testUserURL);
                TestUserQueryResult result = JsonConvert.DeserializeObject<TestUserQueryResult>(testUsersJson);
                if (result.error != null) throw new Exception("Can't get list of test users");

                //Get birthday of each user
                string[] birthday_list = new string[result.testUserList.Count];
                int i = 0;
                foreach (TestUser user in result.testUserList)
                {
                    string birthdayURL = $"https://graph.facebook.com/v3.2/{user.id}/?fields=id,name,birthday&access_token={user.access_token}";
                    TestUserProfile userProfile = JsonConvert.DeserializeObject<TestUserProfile>(Get(birthdayURL));
                    birthday_list[i++] = userProfile.birthday;
                }
                jsonReturn.data = birthday_list;
                jsonReturn.isSuccess = true;
            }
            catch (Exception ex)
            {
                jsonReturn.isSuccess = false;
                jsonReturn.errorMsg = ex.Message;
            }

            return Json(jsonReturn, "application/json");
        }

        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public string Post(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;

            using (Stream requestBody = request.GetRequestStream())
            {
                requestBody.Write(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<string> PostAsync(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;

            using (Stream requestBody = request.GetRequestStream())
            {
                await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}