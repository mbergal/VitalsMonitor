using System;
using RestSharp;

namespace Monitor.Services
{
    public class PagerDutyService : ISendAlertService
    {
        private string pagerDutyAccessKey;

        public PagerDutyService(string pagerDutyAccessKey)
        {
            this.pagerDutyAccessKey = pagerDutyAccessKey;
        }

        public void SendAlert(string title, string description)
        {
            var client = new RestClient("https://api.pagerduty.com/");
            var request = new RestRequest("/incidents/", Method.POST);
            var authorizationToken = this.pagerDutyAccessKey;
            request.AddHeader("Authorization", "Token token=" + authorizationToken);
            request.AddHeader("From", "misha.bergal@gmail.com");


            string json = @"
{
""incident"": {
            ""type"": ""incident"",
            ""title"": ""The server is on fire."",
            ""service"": {
                ""id"": ""PJT4ID6"",
                ""type"": ""service_reference""
            }
}
}
";

            request.AddParameter("application/json; charset=utf-8", json,
                ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            var r = client.Execute(request);
            if (!r.IsSuccessful)
            {
                throw new Exception(r.ErrorMessage);
            }
        }
    }
}