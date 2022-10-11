using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace FirstHomework
{
    [TestClass]
    public class HttpClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<Pets> cleanUpList = new List<Pets>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{PetEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data

            var categoryName = "Guard Dog";
            var petName = "Blacky";
            var tagName = "Rottweiler";
            var newStatus = "Available";

            Category newCategory = new Category()
            {
                Id = 283746,
                Name = categoryName
            };

            Category tempTag = new Category()
            {
                Id = 993298,
                Name = tagName
            };

            Category[] newTag = { tempTag };

            // Create Json Object
            Pets petData = new Pets()
            {
                Id = 00392345,
                Category = newCategory,
                Name = petName,
                PhotoUrls = new string[] { "https://buypets.com/" },
                Tags = newTag,
                Status = newStatus
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(PetEndpoint), postRequest);

            #endregion

            #region send put request to update data

            // Update value of userData
            petData = new Pets()
            {
                Id = 5435566, //update
                Category = newCategory,
                Name = petName,
                PhotoUrls = new string[] { "https://pets4sale.com/" }, //update
                Tags = newTag,
                Status = "Not Available" //update
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{PetEndpoint}"), postRequest);

            // Get Status Code
            var putStatusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));
            var getStatusCode = getResponse.StatusCode;

            // Deserialize Content
            var listPetData = JsonConvert.DeserializeObject<Pets>(getResponse.Content.ReadAsStringAsync().Result);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, putStatusCode, "Error: Put Status code is not equal to 200");
            Assert.AreEqual(HttpStatusCode.OK, getStatusCode, "Error: Get Status code is not equal to 200");
            Assert.AreEqual(petData.Id, listPetData.Id, "Id did not match with updated value");
            Assert.AreEqual(petData.Category.Id, listPetData.Category.Id, "Category Id did not match with updated value");
            Assert.AreEqual(petData.Category.Name, listPetData.Category.Name, "Category Name did not match with updated value");
            Assert.AreEqual(petData.Name, listPetData.Name, "Name did not match with updated value");
            Assert.AreEqual(petData.PhotoUrls[0], listPetData.PhotoUrls[0], "PhotoUrls did not match with updated value");
            Assert.AreEqual(petData.Tags[0].Id, listPetData.Tags[0].Id, "TagsId did not match with updated value");
            Assert.AreEqual(petData.Tags[0].Name, listPetData.Tags[0].Name, "TagsName did not match with updated value");
            Assert.AreEqual(petData.Status, listPetData.Status, "Status did not match to with updated value");

            #endregion

            #region cleanupdata

            // Add data to cleanup list
            cleanUpList.Add(listPetData);

            #endregion
        }

    }
}
