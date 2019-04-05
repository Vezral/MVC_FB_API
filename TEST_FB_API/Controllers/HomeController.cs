using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TEST_FB_API.Helper;
using TEST_FB_API.Models.Charts;
using TEST_FB_API.Models.FacebookData;
using TEST_FB_API.Models.HeatMap;

namespace TEST_FB_API.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private const string APP_ID = "485102995357580";
        private const string APP_SECRET = "28e8e11afc6c7bca1419b9b3a0b57fd2";
        private static List<DataPoint> DATAPOINT_LIST = new List<DataPoint>();
        public static List<string> SCHEMALIST_LABEL = new List<string>();
        public static List<int> SCHEMALIST_COUNT = new List<int>();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HeatmapPage()
        {
            return View();
        }

        public ActionResult InputPage()
        {
            DATAPOINT_LIST = new List<DataPoint>();
            SCHEMALIST_LABEL = new List<string>();
            SCHEMALIST_COUNT = new List<int>();

            return View();
        }

        #region FB Stuff
        [HttpPost]
        public ActionResult GetChartData()
        {
            DateTime today = DateTime.Today;
            JsonReturnObj jsonReturn = new JsonReturnObj();
            List<string> hometownList = new List<string>()
            {
                //"Johor",
                //"Kedah",
                //"Kelantan",
                "Kuala Lumpur",
                "Labuan",
                //"Melaka",
                //"Negeri Sembilan",
                //"Pahang",
                //"Penang",
                //"Perak",
                //"Perlis",
                "Putrajaya",
                "Sabah",
                "Sarawak",
                //"Selangor",
                //"Terengganu"
            };

            try
            {
                //Get App Token
                string appTokenURL = $"https://graph.facebook.com/oauth/access_token?client_id={APP_ID}&client_secret={APP_SECRET}&grant_type=client_credentials";
                FBAppToken appTokenObj = JsonConvert.DeserializeObject<FBAppToken>(WebRequestHelper.Get(appTokenURL));
                string access_token = appTokenObj.AccessToken;

                //Get list of test users
                string testUserURL = $"https://graph.facebook.com/v3.2/{APP_ID}/accounts/test-users?access_token={access_token}";
                string testUsersJson = WebRequestHelper.Get(testUserURL);
                TestUserQueryResult result = JsonConvert.DeserializeObject<TestUserQueryResult>(testUsersJson);
                if (result.Error != null) throw new Exception(result.Error);

                //Get list of test users profile
                Random rnd = new Random();
                List<TestUserProfile> userProfileList = new List<TestUserProfile>();
                foreach (TestUser user in result.TestUserList)
                {
                    string profileURL = $"https://graph.facebook.com/v3.2/{user.Id}/?fields=id,name,birthday,gender,hometown&access_token={user.AccessToken}";
                    TestUserProfile userProfile = JsonConvert.DeserializeObject<TestUserProfile>(WebRequestHelper.Get(profileURL));

                    char[] genderCharArray = userProfile.Gender.ToCharArray();
                    genderCharArray[0] = char.ToUpper(genderCharArray[0]);
                    userProfile.Gender = new string(genderCharArray);

                    DateTime birthday = DateTime.ParseExact(userProfile.Birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    userProfile.BirthdayMYFormat = birthday.ToString("dd/MM/yyyy");
                    userProfile.Hometown = hometownList[rnd.Next(hometownList.Count)];
                    userProfileList.Add(userProfile);
                }

                //Populate JSON data for barchart
                BarChartData barChartData = GetBarChartDataFromUserProfileList(userProfileList,today);
                RadarChartData radarChartData = GetRadarChartDataFromUserProfileList(userProfileList, hometownList);

                jsonReturn.BarChartData = barChartData;
                jsonReturn.RadarChartData = radarChartData;

                jsonReturn.IsSuccess = true;
            }
            catch (Exception ex)
            {
                jsonReturn.IsSuccess = false;
                jsonReturn.ErrorMsg = ex.Message;
            }
            return Json(jsonReturn, "application/json");
        }

        private BarChartData GetBarChartDataFromUserProfileList(List<TestUserProfile> userProfileList, DateTime today)
        {
            BarChartData barChartData = new BarChartData();
            List<BarChartUserData> userDataList = new List<BarChartUserData>();

            foreach (TestUserProfile userProfile in userProfileList)
            {
                BarChartUserData userData = new BarChartUserData();

                DateTime birthday = DateTime.ParseExact(userProfile.Birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                int age = today.Year - birthday.Year;
                if (birthday.AddYears(age) > today) age--;

                string gender = userProfile.Gender;

                // Check if gender already in list
                BarChartUserData obj = userDataList.FirstOrDefault(item => item.Gender == gender);
                if (obj == null)
                {
                    userData.Gender = gender;
                    List<TestUserProfile> innerUserProfileList = new List<TestUserProfile>();
                    innerUserProfileList.Add(userProfile);

                    userData.InnerData = new List<BarChartInnerData>
                        {
                            new BarChartInnerData
                            {
                                Count = 1,
                                Age = age,
                                ProfileList = innerUserProfileList
                            }
                        };
                    userDataList.Add(userData);
                }
                else
                {
                    // Check if age already in list
                    BarChartInnerData innerObj = obj.InnerData.FirstOrDefault(item => item.Age == age);
                    if (innerObj == null)
                    {
                        List<TestUserProfile> innerUserProfileList = new List<TestUserProfile>();
                        innerUserProfileList.Add(userProfile);

                        obj.InnerData.Add(new BarChartInnerData
                        {
                            Count = 1,
                            Age = age,
                            ProfileList = innerUserProfileList
                        });
                    }
                    else
                    {
                        innerObj.Count += 1;
                        innerObj.ProfileList.Add(userProfile);
                    }
                }
            }

            int minAge = userDataList.Min(item => item.InnerData.Min(innerData => innerData.Age));
            int maxAge = userDataList.Max(item => item.InnerData.Max(innerData => innerData.Age));
            foreach (BarChartUserData returnData in userDataList)
            {
                for (int i = minAge; i < maxAge + 1; i++)
                {
                    if (returnData.InnerData.FirstOrDefault(item => item.Age == i) == null)
                    {
                        returnData.InnerData.Add(new BarChartInnerData
                        {
                            Age = i,
                            Count = 0
                        });
                    }
                    returnData.InnerData = returnData.InnerData.OrderBy(item => item.Age).ToList();
                }
            }
            userDataList = userDataList.OrderByDescending(item => item.Gender).ToList();

            // Generate Schema
            foreach(BarChartUserData outerData in userDataList)
            {
                foreach(BarChartInnerData innerData in outerData.InnerData)
                {
                    innerData.GenerateUserProfileSchema();
                }
            }

            barChartData.MinAge = minAge;
            barChartData.MaxAge = maxAge;
            barChartData.UserList = userDataList;

            return barChartData;
        }

        private RadarChartData GetRadarChartDataFromUserProfileList(List<TestUserProfile> userProfileList, List<string> hometownList)
        {
            RadarChartData radarChartData = new RadarChartData();
            List<RadarChartUserData> userDataList = new List<RadarChartUserData>();

            foreach(string hometown in hometownList)
            {
                RadarChartUserData userData = new RadarChartUserData();

                List<TestUserProfile> residentList = userProfileList.Where(user => user.Hometown == hometown).ToList();
                userData.Hometown = hometown;
                userData.Count = residentList.Count;
                userData.ProfileList = new List<TestUserProfile>();
                foreach(TestUserProfile resident in residentList)
                {
                    userData.ProfileList.Add(resident);
                }

                userDataList.Add(userData);
            }

            // Generate Schema
            foreach (RadarChartUserData data in userDataList)
            {
                data.GenerateUserProfileSchema();
            }

            radarChartData.HometownList = hometownList;
            radarChartData.UserData = userDataList;

            return radarChartData;
        }

        private class JsonReturnObj
        {
            public BarChartData BarChartData { get; set; }

            public RadarChartData RadarChartData { get; set; }

            public bool IsSuccess { get; set; }

            public string ErrorMsg { get; set; }
        }
        #endregion

        #region HeatMap Stuff
        [HttpPost]
        public ActionResult RetrieveData()
        {
            HeatMapReturnObj jsonReturn = new HeatMapReturnObj();
            jsonReturn.IsSuccess = false;
            jsonReturn.DataPointListJSON = JsonConvert.SerializeObject(DATAPOINT_LIST);
            jsonReturn.SchemaListCount = SCHEMALIST_COUNT;
            jsonReturn.IsSuccess = true;

            return Json(jsonReturn, "application/json");
        }

        [HttpPost]
        public void UpdateDataPointList(DataPoint dataPoint)
        {
            DATAPOINT_LIST.Add(dataPoint);
        }

        private class HeatMapReturnObj
        {
            public string DataPointListJSON { get; set; }

            public List<int> SchemaListCount { get; set; }

            public bool IsSuccess { get; set; }

            public string ErrorMsg { get; set; }
        }

        public void InitializeSchemaList(List<string> schemaList)
        {
            foreach(string item in schemaList)
            {
                SCHEMALIST_LABEL.Add(item);
            }

            SCHEMALIST_COUNT = new List<int>(new int[SCHEMALIST_LABEL.Count]);
        }

        public void UpdateSchemaListCount(string name)
        {
            var index = SCHEMALIST_LABEL.IndexOf(name);
            if (index != -1)
            {
                SCHEMALIST_COUNT[index] += 1;
            }
        }

        [HttpPost]
        public ActionResult RetrieveSchemaListLabel()
        {
            return Json(SCHEMALIST_LABEL);
        }
        #endregion
    }
}