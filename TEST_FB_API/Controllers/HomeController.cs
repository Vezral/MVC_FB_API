using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TEST_FB_API.Controllers
{
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
        public ActionResult GetChartData()
        {
            DateTime today = DateTime.Today;
            JsonReturnObj jsonReturn = new JsonReturnObj();
            List<string> hometownList = new List<string>()
            {
                "Johor",
                "Kedah",
                "Kelantan",
                "Kuala Lumpur",
                "Labuan",
                "Melaka",
                "Negeri Sembilan",
                "Pahang",
                "Penang",
                "Perak",
                "Perlis",
                "Putrajaya",
                "Sabah",
                "Sarawak",
                "Selangor",
                "Terengganu"
            };

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
                if (result.error != null) throw new Exception(result.error);

                //Get list of test users profile
                Random rnd = new Random();
                List<TestUserProfile> userProfileList = new List<TestUserProfile>();
                foreach (TestUser user in result.testUserList)
                {
                    string profileURL = $"https://graph.facebook.com/v3.2/{user.id}/?fields=id,name,birthday,gender,hometown&access_token={user.access_token}";
                    TestUserProfile userProfile = JsonConvert.DeserializeObject<TestUserProfile>(Get(profileURL));
                    userProfile.hometown = hometownList[rnd.Next(hometownList.Count)];
                    userProfileList.Add(userProfile);
                }

                //Populate JSON data for barchart
                BarChartData barChartData = getBarChartDataFromUserProfileList(userProfileList,today);

                //List<string> userHometownList = new List<string>();
                //foreach(TestUserProfile userProfile in userProfileList)
                //{
                //    userHometownList.Add(userProfile.hometown);
                //}

                jsonReturn.barChartData = barChartData;
                jsonReturn.isSuccess = true;
            }
            catch (Exception ex)
            {
                jsonReturn.isSuccess = false;
                jsonReturn.errorMsg = ex.Message;
            }
            return Json(jsonReturn, "application/json");
        }

        private BarChartData getBarChartDataFromUserProfileList(List<TestUserProfile> userProfileList, DateTime today)
        {
            BarChartData barChartData = new BarChartData();
            List<BarChartUserData> userDataList = new List<BarChartUserData>();

            foreach (TestUserProfile userProfile in userProfileList)
            {
                BarChartUserData userData = new BarChartUserData();

                char[] genderCharArray = userProfile.gender.ToCharArray();
                genderCharArray[0] = char.ToUpper(genderCharArray[0]);
                string gender = new string(genderCharArray);

                DateTime birthday = DateTime.ParseExact(userProfile.birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                int age = today.Year - birthday.Year;
                if (birthday.AddYears(age) > today) age--;

                // Check if gender already in list
                BarChartUserData obj = userDataList.FirstOrDefault(item => item.gender == gender);
                if (obj == null)
                {
                    userData.gender = gender;
                    userData.innerData = new List<InnerData>
                        {
                            new InnerData
                            {
                                count = 1,
                                age = age
                            }
                        };
                    userDataList.Add(userData);
                }
                else
                {
                    // Check if age already in list
                    InnerData innerObj = obj.innerData.FirstOrDefault(item => item.age == age);
                    if (innerObj == null)
                    {
                        obj.innerData.Add(new InnerData
                        {
                            count = 1,
                            age = age
                        });
                    }
                    else
                    {
                        innerObj.count += 1;
                    }
                }
            }

            int minAge = userDataList.Min(item => item.innerData.Min(innerData => innerData.age));
            int maxAge = userDataList.Max(item => item.innerData.Max(innerData => innerData.age));
            foreach (BarChartUserData returnData in userDataList)
            {
                for (int i = minAge; i < maxAge + 1; i++)
                {
                    if (returnData.innerData.FirstOrDefault(item => item.age == i) == null)
                    {
                        returnData.innerData.Add(new InnerData
                        {
                            age = i,
                            count = 0
                        });
                    }
                    returnData.innerData = returnData.innerData.OrderBy(item => item.age).ToList();
                }
            }
            userDataList = userDataList.OrderByDescending(item => item.gender).ToList();

            barChartData.minAge = minAge;
            barChartData.maxAge = maxAge;
            barChartData.userList = userDataList;

            return barChartData;
        }
        
        private class JsonReturnObj
        {
            public BarChartData barChartData { get; set; }

            public bool isSuccess { get; set; }

            public string errorMsg { get; set; }
        }

        private class BarChartData
        {
            public int minAge { get; set; }

            public int maxAge { get; set; }

            public List<BarChartUserData> userList { get; set; }

        }

        private class BarChartUserData
        {
            public string gender { get; set; }

            public List<InnerData> innerData { get; set; }
        }

        private class InnerData
        {
            public int age { get; set; }

            public int count { get; set; }
        }

        private class FBAppToken
        {
            [JsonProperty("access_token")]
            public string access_token { get; set; }

            [JsonProperty("token_type")]
            public string token_type { get; set; }
        }

        private class TestUserQueryResult
        {
            [JsonProperty("data")]
            public List<TestUser> testUserList { get; set; }

            [JsonProperty("error")]
            public string error { get; set; }
        }

        private class TestUser
        {
            [JsonProperty("id")]
            public string id { get; set; }

            [JsonProperty("access_token")]
            public string access_token { get; set; }
        }

        private class TestUserProfile
        {
            [JsonProperty("id")]
            public string id { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("birthday")]
            public string birthday { get; set; }

            [JsonProperty("gender")]
            public string gender { get; set; }

            [JsonProperty("hometown")]
            public string hometown { get; set; }
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