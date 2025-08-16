using Exam.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json.Serialization;
using System;
using System.Text.Json;




namespace  Exam
{
    [TestFixture]
    public class StorySpoilerTests

    {
        private RestClient client;
        private static string createdStoryId;
        //your link here
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // your credentials
            string token = GetJwtToken("Lilyyy777", "123456");
            

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);

        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }


        [Order(1)]
        [Test]
        public void CreateNewStortSpoilerWithRequaredFields()
        {
            var storyData = new StoryDTO
            {
                Title = "NewStoryExam",
                Description = "Description here",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyData);

            var response = client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That(createResponse.StoryId, Is.Not.Null);
                Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
            });
            string newStoryId = createResponse.StoryId;
            createdStoryId = newStoryId;

        }

        [Order(2)]
        [Test]
        public void EditCreatedStorySpoiler()
        {

            var changes = new[]
            {
                new {path = "/name", op = "replace", value = "Editet stort title"}
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);

            var response = client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
            });


        }

        [Order(3)]
        [Test]
        public void GetAllStorySpoilers()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

        }

        [Order(4)]
        [Test]
        public void DeleteStorySpoiler() 
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);

            var response = client.Execute(request);

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse.Msg, Is.EqualTo("Successfully edited"));

        }

        [Order(5)]
        [Test]
        public void CreatedSpolerWithoutRequiredFields()
        {
            var storyData = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyData);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Order(6)]
        [Test]

        public void EditNonExistingSpoiler()
        {
            string nonExistingId = "123";

            var storyData = new StoryDTO
            {
                Title = "NewStoryExam",
                Description = "Description here",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingId}", Method.Put);
            request.AddJsonBody(storyData);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("Successfully edited"));


        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingSpoiler() 
        {
            string nonExistingId = "123";
            var request = new RestRequest($"/api/Story/Delete/{nonExistingId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));


        }


        [OneTimeTearDown]
        public void OneTimeTearDown() 
        {
            client?.Dispose();
        }
    }
}