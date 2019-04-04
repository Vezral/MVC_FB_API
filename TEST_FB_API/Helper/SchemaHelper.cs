using Schema.NET;
using System;
using System.Globalization;
using System.Web;
using TEST_FB_API.Models.FacebookData;

namespace TEST_FB_API.Helper
{
    public class SchemaHelper
    {
        public static Person GeneratePersonSchema(TestUserProfile userProfile)
        {
            string region;
            switch (userProfile.Hometown)
            {
                case "Perlis":
                case "Kedah":
                case "Penang":
                case "Perak":
                    region = "Northern Region";
                    break;
                case "Kuala Lumpur":
                case "Selangor":
                case "Negeri Sembilan":
                case "Putrajaya":
                    region = "Central Region";
                    break;
                case "Melaka":
                case "Johor":
                    region = "Southern Region";
                    break;
                case "Pahang":
                case "Kelantan":
                case "Terengganu":
                    region = "East Coast";
                    break;
                case "Labuan":
                case "Sabah":
                case "Sarawak":
                    region = "East Malaysia";
                    break;
                default:
                    region = "Unknown";
                    break;
            }

            var request = HttpContext.Current.Request;
            var userPageURL = string.Format("{0}://{1}/User?id={2}", request.Url.Scheme, request.Url.Authority,userProfile.Id);

            return new Person()
            {
                Name = userProfile.Name,
                Address = new PostalAddress()
                {
                    AddressCountry = "Malaysia",
                    AddressLocality = userProfile.Hometown,
                    AddressRegion = region
                },
                BirthDate = DateTimeOffset.ParseExact(userProfile.BirthdayMYFormat, "dd/MM/yyyy", new CultureInfo("en-MY")),
                Url = new Uri(userPageURL)
            };
        }
    }
}