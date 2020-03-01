using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoDoorServer3.Models.DataModels;
using Microsoft.Extensions.Logging;

namespace GeoDoorServer3.CustomService
{
    public class OpenHabMessageService : IOpenHabMessageService
    {
        private readonly IDataSingleton _iDataSingleton;

        public OpenHabMessageService(IDataSingleton iDataSingleton)
        {
            _iDataSingleton = iDataSingleton;
        }

        /// <summary>
        /// Gets Data from the OpenHab Rest API
        /// </summary>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        public async Task<string> GetData(string itemPath)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            var result = await client.GetStringAsync(itemPath);
            return result;
        }

        /// <summary>
        /// Posts Data to the OpenHab Rest API
        /// </summary>
        /// <param name="itemPath"></param>
        /// <param name="value"></param>
        /// <param name="toggle">s</param>
        /// <returns></returns>
        public async Task PostData(string itemPath, string value, bool toggle = false)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, itemPath);
            request.Content = new StringContent(value,
                Encoding.UTF8,
                "text/plain");

            await client.SendAsync(request)
                .ContinueWith(responseTask =>
                {
                    _iDataSingleton.AddErrorLog(new ErrorLog()
                    {
                        LogLevel = LogLevel.Debug,
                        MsgDateTime = DateTime.Now,
                        Message = $"{typeof(OpenHabMessageService)}:PostData => Response: {responseTask.Result}"
                    });

                    _iDataSingleton.GetSystemStatus().OpenHabStatus = responseTask.Result.IsSuccessStatusCode;
                });
            
            if (toggle && _iDataSingleton.GetSystemStatus().OpenHabStatus)
            {
                Thread.Sleep(500);
                await PostData(itemPath, "OFF");
            }
        }
    }
}
