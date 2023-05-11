using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace BCI.CustomAuthenticationExtensionsAPI
{
    public static class HttpTrigger_CustomAuthenticationExtensionsAPI
    {
        [FunctionName("HttpTrigger_CustomAuthenticationExtensionsAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Read the correlation ID from the Azure AD  request    
            string correlationId = data?.data.authenticationContext.correlationId;

            // Claims to return to Azure AD
            ResponseContent r = new ResponseContent();
            r.data.actions[0].claims.CorrelationId = correlationId;
            r.data.actions[0].claims.ApiVersion = "1.0.0";
            r.data.actions[0].claims.DateOfBirth = "01/01/2000";
            r.data.actions[0].claims.CustomRoles.Add("Writer");
            r.data.actions[0].claims.CustomRoles.Add("Editor");
            return new OkObjectResult(r);
        }

        public class ResponseContent{
            [JsonProperty("data")]
            public Data data { get; set; }
            public ResponseContent()
            {
                data = new Data();
            }
        }

        public class Data{
            [JsonProperty("@odata.type")]
            public string odatatype { get; set; }
            public List<Action> actions { get; set; }
            public Data()
            {
                odatatype = "microsoft.graph.onTokenIssuanceStartResponseData";
                actions = new List<Action>();
                actions.Add(new Action());
            }
        }

        public class Action{
            [JsonProperty("@odata.type")]
            public string odatatype { get; set; }
            public Claims claims { get; set; }
            public Action()
            {
                odatatype = "microsoft.graph.provideClaimsForToken";
                claims = new Claims();
            }
        }

        public class Claims{
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string CorrelationId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DateOfBirth { get; set; }
            public string ApiVersion { get; set; }
            public List<string> CustomRoles { get; set; }
            public Claims()
            {
                CustomRoles = new List<string>();
            }
        }    
    }
}
