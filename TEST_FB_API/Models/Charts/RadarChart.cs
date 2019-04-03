using Schema.NET;
using System.Collections.Generic;
using TEST_FB_API.Helper;
using TEST_FB_API.Models.FacebookData;

namespace TEST_FB_API.Models.Charts
{
    public class RadarChartData
    {
        public List<string> HometownList { get; set; }

        public List<RadarChartUserData> UserData { get; set; }
    }

    public class RadarChartUserData
    {
        public string Hometown { get; set; }

        public int Count { get; set; }

        public List<TestUserProfile> ProfileList { get; set; }

        public string UserProfileSchema { get; set; }

        public void GenerateUserProfileSchema()
        {
            if (ProfileList != null && ProfileList.Count > 0)
            {
                List<ListItem> itemList = new List<ListItem>();
                int i = 1;

                foreach (TestUserProfile userProfile in ProfileList)
                {
                    itemList.Add(new ListItem()
                    {
                        Position = i++,
                        Item = SchemaHelper.GeneratePersonSchema(userProfile)
                    });
                }

                UserProfileSchema = new ItemList()
                {
                    ItemListElement = itemList
                }.ToString();
            }
        }
    }
}