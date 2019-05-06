using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Slack_Api_Practice.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace Slack_Api_Practice.Controllers
{

    public class SlackController : Controller
    {
        private readonly IConfiguration configuration;

        public SlackController(IConfiguration config)
        {
            this.configuration = config;
        }

        [HttpPost("slack")]
        public IActionResult SendEmail([FromForm] string email, [FromForm] string messageToSend )
        {
            var Client = new HttpClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Token"]);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/users.lookupByEmail");

            var list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("email", email));
  
            request.Content = new FormUrlEncodedContent(list);

            var response = Client.SendAsync(request).Result;
            EmailLookupResponse responseObject = new EmailLookupResponse();
            responseObject = JsonConvert.DeserializeObject<EmailLookupResponse>(response.Content.ReadAsStringAsync().Result);
            string responseString = response.Content.ReadAsStringAsync().Result;

            var postMessageRequest = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage");

            var messageRequestBody = new List<KeyValuePair<string, string>>();
            messageRequestBody.Add(new KeyValuePair<string, string>("channel", responseObject.user.id));
            messageRequestBody.Add(new KeyValuePair<string, string>("text", messageToSend));
            postMessageRequest.Content = new FormUrlEncodedContent(messageRequestBody);

            Client.SendAsync(postMessageRequest);

            return Ok(new {messageSentTo = email, userId = responseObject.user.id });
        }
    }
}
