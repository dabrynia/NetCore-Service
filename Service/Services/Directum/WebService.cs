using Microsoft.Extensions.Logging;
using Service.Models;
using Service.Properties;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.Directum
{
    public class WebService
    {
        private readonly ILogger<WebService> _logger;
        private readonly DirectumWebServiceSettings _connectionSettings;

        public WebService(ILogger<WebService> logger, DirectumWebServiceSettings settings)
        {
            _logger = logger;
            _connectionSettings = settings;
        }

        public async Task RunAsync(string message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                _logger.LogInformation($"[.]Получено сообщение: {message}");

                if (SendTestPostRequest(SetSOAPRequestData(message)))
                {
                    _logger.LogInformation($"[->]Отправлено сообщение: {message}");
                }

            }, token);
        }

        private HttpWebRequest CreateRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_connectionSettings.Url);
            request.Method = "POST";
            request.Host = Resources.SoapPort;
            request.Headers.Add("SOAPAction", Resources.SOAPAction);
            request.ContentType = Resources.ContentType;

            return request;
        }

        private void SetRequestData(HttpWebRequest request, string data)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] postByteArray = encoding.GetBytes(data);
            request.ContentLength = postByteArray.Length;

            System.IO.Stream postStream = request.GetRequestStream();
            postStream.Write(postByteArray, 0, postByteArray.Length);
            postStream.Close();
        }

        private string SetSOAPRequestData(string data)
        {
            string soapData = string.Format(Resources.soapData, data);

            return soapData;
        }

        private bool SendTestPostRequest(string data)
        {
            bool isSend = false;
            try
            {
                var request = CreateRequest();
                SetRequestData(request, data);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    _logger.LogInformation("Response Status Description: " + response.StatusDescription);
                    using (Stream dataSteam = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataSteam))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            _logger.LogInformation("Response: " + responseFromServer);

                            isSend = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"При отправке запроса на web-сервис возникла ошибка: {ex.Message}");
            }

            return isSend;
        }
    }
}
