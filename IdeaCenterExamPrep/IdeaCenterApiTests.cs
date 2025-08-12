using System;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using IdeaCenterExamPrep.Models;
using System.ComponentModel.Design;


namespace IdeaCenterExamPrep
{
    [TestFixture]
    public class IdeaCenterApiTests
    {
        private RestClient client;
        private static string lastCreatedIdeaId;
        private object email;
        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";

        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJjZjNkZjZjMi04OWIzLTQ3NGMtOTk1Ny1lN2E1YWQ3YjJmNGEiLCJpYXQiOiIwOC8xMi8yMDI1IDE2OjA3OjA1IiwiVXNlcklkIjoiNTYwODYzZTAtNDIzMC00NTA2LWQyODMtMDhkZGQ0ZTA4YmQ4IiwiRW1haWwiOiJ1c2VyMTAxMEBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiVXNlcjEwMTAiLCJleHAiOjE3NTUwMzY0MjUsImlzcyI6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.kImX06ke8wLTTip7UECa8pY_ga4U1H2UcyzFlmvMJ_E";

        private const string LoginEmai = "user1010@example.com";
        private const string LoginPassword = "123456";

        
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmai,  LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this .client = new RestClient(options);

        }

        private string GetJwtToken(string loginEmai, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email,password});

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (!string.IsNullOrWhiteSpace(token))
                { 

                    throw new InvalidOperationException("Failed to retrieve JWT token from the response.");
                }
                return token;
            }
        else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode},Content: {response.Content}");
            }
        }
        //All test here
        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFields_ShouldReturnSuccess()
        {
            var ideaRequest = new IdeaDTO
            {
                Title = "Test idea",
                Description = "My Description",
                Url = ""
            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this.client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }
        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnListOfIdeas()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

            lastCreatedIdeaId = responseItems.LastOrDefault()?.Id;

        }

        [Order(3)]
        [Test]

        public void EditExistingIdea_ShouldIdReurnSucess()
        {
            var editRequest = new IdeaDTO
            {
                Title = "Test new idea",
                Description = "My new Description",
                Url = ""
            };

            var request = new RestRequest($"/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }


        [Order(4)]
        [Test]

        public void DeleteIdea_ShouldReturnSuccess()
        {
           
            var request = new RestRequest($"/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));
        }

        [Order(5)]
        [Test]

        public void CreateIdea_WithoutRequiredFields_ShouldReturnSuccessAgain()
        {
            var ideaRequest = new IdeaDTO
            {
                Title = "",
                Description = "",
               
            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this.client.Execute(request);
            

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
        }
        [Order(6)]
        [Test]

        public void EditNonExistingIdea_ShouldReturnSuccess()
        {
            string nonExistingIdeaId = "123";
            var editRequest = new IdeaDTO
            {
                Title = "Test new idea",
                Description = "My new Description",
                Url = ""
            };

            var request = new RestRequest($"/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
            
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingIdea_ShouldReturnSuccess()
        {
            string nonExistingIdeaId = "123";
            var request = new RestRequest($"/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }

        [OneTimeTearDown]
        public void Teardown ()
        {
            this.client?.Dispose();
        }
    }
}