using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SensorConfiguration.Helper.Http
{
    public class LegrandResponse
    {
        public bool Status;

        public object Content;

        private LegrandResponse()
        {

        }

        public static LegrandResponse OK(object content)
        {
            return new LegrandResponse
            {
                Status = true,
                Content = content
            };
        }

        public static LegrandResponse Error(object content)
        {
            return new LegrandResponse
            {
                Status = false,
                Content = content
            };
        }
    }

    public class HttpHelper
    {
        public static string BaseUrl = "http://127.0.0.1:9991/";

        private HttpHelper()
        {

        }

        private string GetUrl(string serviceMethod)
        {
            return BaseUrl + serviceMethod;
        }

        private string GetUrl(string serviceMethod, Dictionary<string, string> paras)
        {
            var url = BaseUrl + serviceMethod;
            if (paras != null && paras.Any())
            {
                url += "?";
                var index = 1;
                foreach (var para in paras)
                {
                    if (index > 1)
                    {
                        url += "&";
                    }
                    url += string.Format("{0}={1}", para.Key, para.Value);
                    index++;
                }
            }
            return url;
        }

        public async Task<LegrandResponse> Post(string serviceMethod, object data)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                // 设置请求头（可选）
                httpClient.DefaultRequestHeaders.Add("charset", "utf-8");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
                // 创建 HttpContent 对象，将要发送的数据放入请求体
                string json = JsonConvert.SerializeObject(data);
                HttpContent httpContent = new StringContent("", Encoding.UTF8, "application/json");

                // 发送 POST 请求
                HttpResponseMessage response = await httpClient.PostAsync(GetUrl(serviceMethod), httpContent);

                // 确保请求成功（200-299 范围内的状态码）
                response.EnsureSuccessStatusCode();
                // 响应数据
                return await Task.Run(() =>
                {
                    return LegrandResponse.OK(response.Content.ReadAsStringAsync());
                });
            }
            catch (Exception e)
            {
                return LegrandResponse.Error(e.Message);
            }

        }

        public async Task<LegrandResponse> Get(string serviceMethod, Dictionary<string, string> param)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                // 设置请求头（可选）
                httpClient.DefaultRequestHeaders.Add("charset", "utf-8");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");

                // 发送 GET 请求
                HttpResponseMessage response = await httpClient.GetAsync(GetUrl(serviceMethod, param));

                // 确保请求成功（200-299 范围内的状态码）
                response.EnsureSuccessStatusCode();

                // 响应数据
                return await Task.Run(() =>
                {
                    return LegrandResponse.Error(response.Content.ReadAsStringAsync());
                });
            }
            catch (Exception e)
            {
                return LegrandResponse.Error(e.Message);
            }
        }

        public static async Task<LegrandResponse> HttpPost(string serviceMethod, object data)
        {
            return await new HttpHelper().Post(serviceMethod, data);
        }

        public static async Task<LegrandResponse> HttpGet(string serviceMethod, Dictionary<string, string> param)
        {
            return await new HttpHelper().Get(serviceMethod, param);
        }

        public static async Task<LegrandResponse> HttpGet(string serviceMethod)
        {
            return await new HttpHelper().Get(serviceMethod, null);
        }
    }
}
