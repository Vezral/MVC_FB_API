using Schema.NET;
using System.Collections.Generic;
using TEST_FB_API.Helper;
using TEST_FB_API.Models.FacebookData;

namespace TEST_FB_API.Models.Charts
{
    public class BarChartData
    {
        public int MinAge { get; set; }

        public int MaxAge { get; set; }

        public List<BarChartUserData> UserList { get; set; }

    }

    public class BarChartUserData
    {
        public string Gender { get; set; }

        public List<BarChartInnerData> InnerData { get; set; }
    }

    public class BarChartInnerData
    {
        public int Age { get; set; }

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