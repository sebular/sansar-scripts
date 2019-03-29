using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItemCollector
{
    [RegisterReflective]
    public class PickupHttpClient : SceneObjectScript
    {

        public override void Init()
        {
        }

        public void PostResults(Guid personaId, TimeSpan time)
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.POST;
            options.Headers = new Dictionary<string, string>()
            {
                {"content-type", "application/json" }
            };
            options.Body = $"{{\"personaId\":\"{personaId}\",\"milliseconds\": \"{time.TotalMilliseconds}\"}}";
            var result = WaitFor(ScenePrivate.HttpClient.Request, "https://postman-echo.com/post", options) as HttpClient.RequestData;
            if (result.Success)
            {
                Log.Write(LogLevel.Info, $"{result.Response.Body}");
            }
        }

    }
}