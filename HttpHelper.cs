using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Drawing;
using log4net;
using System.Threading.Tasks;

namespace iLean.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpHelper
    {
        static ILog log = LogManager.GetLogger("HttpHelper");
        public static List<KeyValuePair<string, string>> SaveFilesAndReturnFilePath(HttpContext context, string basePath)
        {
            var filePathList = new List<KeyValuePair<string, string>>();

            var appPath = context.Server.MapPath("~/");

            string path = Path.Combine(
                appPath, @"upload",
                basePath,
                DateTime.Now.ToString("yyyyMM"));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }


            for (int iFile = 0; iFile < context.Request.Files.Count; iFile++)
            {
                ///'检查文件扩展名字  
                HttpPostedFile postedFile = context.Request.Files[iFile];


                string fileSavePath = Path.Combine(path, Guid.NewGuid() + Path.GetExtension(postedFile.FileName));
                postedFile.SaveAs(fileSavePath);

                var fileSrc = fileSavePath.Replace(appPath, ""); //转换成相对路径  
                fileSrc = fileSrc.Replace(@"\", @"/");

                var nameAndPath = new KeyValuePair<string, string>(postedFile.FileName, "/" + fileSrc);

                filePathList.Add(nameAndPath);
            }

            return filePathList;
        }

        public static List<dynamic> SaveFilesAndReturnFileInfo(HttpContext context, string basePath)
        {
            List<dynamic> filePathList = new List<dynamic>();

            var appPath = context.Server.MapPath("~/");

            string path = Path.Combine(
                appPath, @"upload",
                basePath,
                DateTime.Now.ToString("yyyyMM"));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }


            for (int iFile = 0; iFile < context.Request.Files.Count; iFile++)
            {
                ///'检查文件扩展名字  
                HttpPostedFile postedFile = context.Request.Files[iFile];


                string fileSavePath = Path.Combine(path, Guid.NewGuid() + Path.GetExtension(postedFile.FileName));
                postedFile.SaveAs(fileSavePath);

                var fileSrc = fileSavePath.Replace(appPath, ""); //转换成相对路径  
                fileSrc = fileSrc.Replace(@"\", @"/");

                //var nameAndPath = new KeyValuePair<string, string>(postedFile.FileName, "/" + fileSrc);

                //filePathList.Add(nameAndPath);

                dynamic file = new
                {
                    FileTitle = postedFile.FileName,
                    FilePath = "/" + fileSrc,
                    FileType = postedFile.ContentType,
                    FileSize = postedFile.ContentLength
                };

                filePathList.Add(file);
            }

            return filePathList;
        }

        #region 公共方法：请求webApi接口，获取并返回结果
        public static dynamic GetWebApi(string uri,int timeOut=3000)
        {
            string result = string.Empty;
            WebClient client = new WebClientto(timeOut);
            
            System.IO.Stream stream = client.OpenRead(uri);
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            result = reader.ReadToEnd();
            reader.Close(); 
            return JsonConvert.DeserializeObject(result);
      
        }
        public async static Task<ResponseObject> GetWebApiAsync(string uri,int timeOut=3000)
        {
            string result = string.Empty;
            WebClient client = new WebClientto(timeOut);

            System.IO.Stream stream = client.OpenRead(uri);
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            result = await reader.ReadToEndAsync();
            reader.Close();
            return new ResponseObject(true,JsonConvert.DeserializeObject(result));
        }
        /// <summary>
        /// Get请求WEBAPI
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="url">用户名</param>
        /// <param name="url">密码</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public async static Task<ResponseObject> GetWebApi(string url, string user, string password, int timeOut)
        {
            HttpClient client = new HttpClient();
            client.Timeout = DateTime.Now.AddSeconds(timeOut) - DateTime.Now;

            string resultStr = "";

            HttpResponseMessage response = null;
            // 设置HTTP头Http Basic认证
            string authorization = user + ":" + password;
            string base64 = Convert.ToBase64String(Encoding.Default.GetBytes(authorization));
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64);

            response = await client.GetAsync(url);

            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = await response.Content.ReadAsStringAsync();
            }

            if (response.IsSuccessStatusCode)
            {
                return new ResponseObject(true, JsonConvert.DeserializeObject(resultStr));
            }
            else
            {
                return new ResponseObject(false, JsonConvert.DeserializeObject(resultStr));
            }
        }

        /// <summary>
        /// POST请求WEBAPI
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="data">POST参数</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static dynamic PostWebApi(string url, dynamic data ,int timeOut)
        {
            JObject param = JsonConvert.DeserializeObject(data);
            param.Add("User",SysHelper.GetUserId());
            param.Add("Tenant",SysHelper.GetTenantId());
            data = JsonConvert.SerializeObject(param);
            HttpClient client = new HttpClient();
            client.Timeout=DateTime.Now.AddSeconds(timeOut)-DateTime.Now;
            string resultStr = "";

            HttpResponseMessage response = null;

            HttpContent content = new StringContent(data.ToString());

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = client.PostAsync(url, content).Result;

            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = response.Content.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject(resultStr);
        }

        /// <summary>
        /// POST请求WEBAPI
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="data">POST参数</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public async static Task<ResponseObject> PostWebApi(string url, dynamic data,string contentType,string user,string password,int timeOut)
        {
            JObject param = JsonConvert.DeserializeObject<dynamic>(data);
            data = JsonConvert.SerializeObject(param);
            HttpClient client = new HttpClient();
            client.Timeout = DateTime.Now.AddSeconds(timeOut) - DateTime.Now;

            string resultStr = "";

            HttpResponseMessage response = null;

            HttpContent content = new StringContent(data.ToString());

            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            // 设置HTTP头Http Basic认证
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", user, password))));
            try
            {
                response = await client.PostAsync(url, content);
            }catch(Exception ex)
            {
                log.Error(ex.Message);
                return new ResponseObject(false,ex.Message);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = await response.Content.ReadAsStringAsync();
            }
            if(response.IsSuccessStatusCode)
            {
                return new ResponseObject(true, JsonConvert.DeserializeObject(resultStr));
            }
            else
            {
                return new ResponseObject(false, JsonConvert.DeserializeObject(resultStr));
            }
        }

        /// <summary>
        /// POST请求WEBAPI
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="data">POST参数</param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static ResponseObject PostWebApi(string url, dynamic data, string user, string password, int timeOut)
        {
            string contentType = "application/json";
            JObject param = JsonConvert.DeserializeObject<dynamic>(data);
            data = JsonConvert.SerializeObject(param);
            HttpClient client = new HttpClient();
            client.Timeout = DateTime.Now.AddSeconds(timeOut) - DateTime.Now;

            string resultStr = "";

            HttpResponseMessage response = null;

            HttpContent content = new StringContent(data.ToString());

            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            // 设置HTTP头Http Basic认证
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", user, password))));
            try
            {
                response = client.PostAsync(url, content).Result;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return new ResponseObject(false, ex.Message);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = response.Content.ReadAsStringAsync().Result;
            }
            if (response.IsSuccessStatusCode)
            {
                return new ResponseObject(true, JsonConvert.DeserializeObject(resultStr));
            }
            else
            {
                return new ResponseObject(false, JsonConvert.DeserializeObject(resultStr));
            }
        }

        /// <summary>
        /// WEBAPI请求
        /// </summary>
        /// <param name="ParamApi">接口参数</param>
        /// <returns></returns>
        public async static Task<ResponseObject> WebApi(ParamApi paramapi)
        {
            var isCheck = ParamApi.Check(paramapi);
            if (!isCheck.status)
            {
                return isCheck;
            }

            HttpClient client = new HttpClient();
            client.Timeout = DateTime.Now.AddSeconds(paramapi.timeOut) - DateTime.Now;

            string resultStr = "";

            HttpResponseMessage response;
            HttpContent content = new StringContent(JsonConvert.SerializeObject(paramapi.data));
            content.Headers.ContentType = new MediaTypeHeaderValue(paramapi.contentType);
            if (!string.IsNullOrWhiteSpace(paramapi.user) && !string.IsNullOrWhiteSpace(paramapi.password))
            {
                // 设置HTTP头Http Basic认证
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", paramapi.user, paramapi.password))));
            }
            try
            {
                switch (paramapi.method)
                {
                    case "Get": response = await client.GetAsync(paramapi.url); break;
                    case "Put": response = await client.PutAsync(paramapi.url, content); break;
                    case "Delete": response = await client.DeleteAsync(paramapi.url); break;
                    case "Post": response = await client.PostAsync(paramapi.url, content); break;
                    case "Send": response = await client.SendAsync(new HttpRequestMessage()); break;
                    case "Update": response = await client.GetAsync(paramapi.url); break;
                    default: response = await client.GetAsync(paramapi.url); break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return new ResponseObject(false, ex.Message);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = await response.Content.ReadAsStringAsync();
            }
            if (response.IsSuccessStatusCode)
            {
                return new ResponseObject(true, JsonConvert.DeserializeObject(resultStr));
            }
            else
            {
                return new ResponseObject(false, JsonConvert.DeserializeObject(resultStr));
            }
        }

        /// <summary>
        /// 包含文件WEBAPI请求
        /// </summary>
        /// <param name="ParamApi">接口参数</param>
        /// <returns></returns>
        public async static Task<ResponseObject> FileWebApi(ParamApi paramapi)
        {
            var isCheck = ParamApi.Check(paramapi);
            if (!isCheck.status)
            {
                return isCheck;
            }

            HttpClient client = new HttpClient();
            client.Timeout = DateTime.Now.AddSeconds(paramapi.timeOut) - DateTime.Now;

            string resultStr = "";

            HttpResponseMessage response;
            var content = new MultipartFormDataContent();
            content.Headers.ContentType = new MediaTypeHeaderValue(paramapi.contentType);
            if (HttpContext.Current != null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.FilePath))
                {
                    var files = HttpContext.Current.Request.Files;
                    foreach (HttpPostedFile item in files)
                    {
                        //添加文件参数，参数名为files，文件名为123.png
                        content.Add(new StreamContent(item.InputStream), "file", item.FileName);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(paramapi.user) && !string.IsNullOrWhiteSpace(paramapi.password))
            {
                // 设置HTTP头Http Basic认证
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", paramapi.user, paramapi.password))));
            }
            try
            {
                switch (paramapi.method)
                {
                    case "Get": response = await client.GetAsync(paramapi.url); break;
                    case "Put": response = await client.PutAsync(paramapi.url, content); break;
                    case "Delete": response = await client.DeleteAsync(paramapi.url); break;
                    case "Post": response = await client.PostAsync(paramapi.url, content); break;
                    case "Send": response = await client.SendAsync(new HttpRequestMessage()); break;
                    case "Update": response = await client.GetAsync(paramapi.url); break;
                    default: response = await client.GetAsync(paramapi.url); break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return new ResponseObject(false, ex.Message);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                resultStr = await response.Content.ReadAsStringAsync();
            }
            if (response.IsSuccessStatusCode)
            {
                return new ResponseObject(true, JsonConvert.DeserializeObject(resultStr));
            }
            else
            {
                return new ResponseObject(false, JsonConvert.DeserializeObject(resultStr));
            }
        }
        public string HttpHead(string code)
        {
            string lable = "";
            switch(code)
            {
                case "100": lable = "继续） 请求者应当继续提出请求。 服务器返回此代码表示已收到请求的第一部分，正在等待其余部分。"; break;
                case "101":  lable = "（切换协议） 请求者已要求服务器切换协议，服务器已确认并准备切换。"; break;
                case "200":  lable = "成功处理了请求，一般情况下都是返回此状态码；"; break;
                case "201":  lable = "请求成功并且服务器创建了新的资源。 "; break;
                case "202":  lable = "接受请求但没创建资源； "; break;
                case "203":  lable = "返回另一资源的请求； "; break;
                case "204":  lable = "服务器成功处理了请求，但没有返回任何内容；"; break;
                case "205":  lable = "服务器成功处理了请求，但没有返回任何内容；"; break;
                case "206":  lable = "处理部分请求；"; break;
                case "300":  lable = "（多种选择）  针对请求，服务器可执行多种操作。 服务器可根据请求者 (user agent) 选择一项操作，或提供操作列表供请求者选择。 "; break;
                case "301":  lable = "（永久移动）  请求的网页已永久移动到新位置。 服务器返回此响应（对 GET 或 HEAD 请求的响应）时，会自动将请求者转到新位置。 "; break;
                case "302":  lable = "（临时移动）  服务器目前从不同位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求。 "; break;
                case "303":  lable = "（查看其他位置） 请求者应当对不同的位置使用单独的 GET 请求来检索响应时，服务器返回此代码。 "; break;
                case "304":  lable = "（未修改） 自从上次请求后，请求的网页未修改过。 服务器返回此响应时，不会返回网页内容。 "; break;
                case "305":  lable = "（使用代理） 请求者只能使用代理访问请求的网页。 如果服务器返回此响应，还表示请求者应使用代理。 "; break;
                case "307":  lable = "（临时重定向）  服务器目前从不同位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求。"; break;
                case "400":  lable = "服务器不理解请求的语法。"; break;
                case "401":  lable = "请求要求身份验证。 对于需要登录的网页，服务器可能返回此响应。 "; break;
                case "403":  lable = "服务器拒绝请求。"; break;
                case "404":  lable = "服务器找不到请求的网页。 "; break;
                case "405":  lable = "禁用请求中指定的方法。 "; break;
                case "406":  lable = "无法使用请求的内容特性响应请求的网页。"; break;
                case "407":  lable = "此状态代码与 401类似，但指定请求者应当授权使用代理。"; break;
                case "408":  lable = "服务器等候请求时发生超时。 "; break;
                case "409":  lable = "服务器在完成请求时发生冲突。 服务器必须在响应中包含有关冲突的信息。"; break;
                case "410":  lable = "如果请求的资源已永久删除，服务器就会返回此响应。 "; break;
                case "411":  lable = "服务器不接受不含有效内容长度标头字段的请求。 "; break;
                case "412":  lable = "服务器未满足请求者在请求中设置的其中一个前提条件。 "; break;
                case "413":  lable = "服务器无法处理请求，因为请求实体过大，超出服务器的处理能力。 "; break;
                case "414":  lable = "请求的 URI（通常为网址）过长，服务器无法处理。 "; break;
                case "415":  lable = "请求的格式不受请求页面的支持。 "; break;
                case "416":  lable = "如果页面无法提供请求的范围，则服务器会返回此状态代码。 "; break;
                case "417":  lable = "服务器未满足”期望”请求标头字段的要求。"; break;
                case "500":  lable = "（服务器内部错误）  服务器遇到错误，无法完成请求。"; break;
                case "501":  lable = "（尚未实施） 服务器不具备完成请求的功能。 例如，服务器无法识别请求方法时可能会返回此代码。"; break;
                case "502":  lable = "（错误网关） 服务器作为网关或代理，从上游服务器收到无效响应。 "; break;
                case "503":  lable = "（服务不可用） 服务器目前无法使用（由于超载或停机维护）。 通常，这只是暂时状态。 "; break;
                case "504":  lable = "（网关超时）  服务器作为网关或代理，但是没有及时从上游服务器收到请求。 "; break;
                case "505":  lable = "（HTTP 版本不受支持） 服务器不支持请求中所用的 HTTP 协议版本。"; break;
            }
            return lable;
        }
        /// <summary>
        /// 推送接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static dynamic PostPushWebApi(string url, dynamic data, int timeOut)
        {
            try
            {
                HttpClient client = new HttpClient();
                data = JsonConvert.SerializeObject(data);
                string resultStr = "";

                HttpResponseMessage response = null;
                //data = JsonConvert.DeserializeObject(data);
                HttpContent content = new StringContent(data);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = client.PostAsync(url, content).Result;

                if (response != null && response.IsSuccessStatusCode)
                {
                    resultStr = response.Content.ReadAsStringAsync().Result;
                }

                dynamic result = JsonConvert.DeserializeObject(resultStr);

                return result;
            }
            catch (Exception ex)
            {
                return new { status = false, message = ex.Message };
            }
        }
        public static string GetCurrentServer()
        {
            string server = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            if (HttpContext.Current.Request.ServerVariables["HTTPS"] == "off")
                server = "http://" + server + ":" + HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            else
                server = "https://" + server + ":" + HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            return server;
        }
        #endregion

        #region base64编码的文本转为图片 
        /// <summary>  
        /// base64编码的文本转为图片  
        /// </summary>  
        /// <param name="txtFilePath">文件相对路径</param>  
        /// <param name="str">图片字符串</param>  
        public static dynamic Base64StringToImage(HttpContext context, string str, string filePath)
        {
            try
            {
                String inputStr = str;
                byte[] arr = Convert.FromBase64String(inputStr);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);

                bmp.Save(context.Server.MapPath(filePath), System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Close();
                return new
                {
                    status = true,
                    messsage = "保存图片成功"
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    status = false,
                    messsage = "保存图片失败," + ex.Message
                };
            }
        }
        #endregion
    }


    public class ParamApi
    {
        public ParamApi()
        {

        }
        public ParamApi(string url)
        {
            this.url = url;
        }
        public ParamApi(string url,string method)
        {
            this.url = url;
            this.method = method;
        }
        public ParamApi(string url, dynamic data)
        {
            this.url = url;
            this.data = data;
        }
        public ParamApi(string url, string method, dynamic data)
        {
            this.url = url;
            this.method = method;
            this.data = data;
        }
        public ParamApi(string url, string method, dynamic data, string user, string password)
        {
            this.url = url;
            this.method = method;
            this.data = data;
            this.user = user;
            this.password = password;
        }
        public ParamApi(string url, string method, dynamic data, string contentType)
        {
            this.url = url;
            this.method = method;
            this.data = data;
            this.contentType = contentType;
        }
        public ParamApi(string url, string method, dynamic data, string contentType, string user, string password)
        {
            this.url = url;
            this.method = method;
            this.data = data;
            this.user = user;
            this.password = password;
            this.contentType = contentType;
        }
        public ParamApi(string url, string method, dynamic data, string contentType, int timeOut)
        {
            this.url = url;
            this.method = method;
            this.data = data;
            this.contentType = contentType;
            this.timeOut = timeOut;
        }
        public ParamApi(string url,string method,dynamic data,string contentType,string user,string password,int timeOut)
        {
            this.url = url;
            this.method = method;
            this.data = data;
            this.contentType = contentType;
            this.user = user;
            this.password = password;
            this.timeOut = timeOut;
        }
        public static ResponseObject Check(ParamApi param)
        {
            if(string.IsNullOrEmpty(param.url))
            {
                return new ResponseObject(false,"url不能为空");
            }
            if(string.IsNullOrEmpty(param.method))
            {
                param.method = "Get";
            }
            if(param.data==null)
            {
                param.data = new JObject();
            }
            if(string.IsNullOrWhiteSpace(param.contentType))
            {
                param.contentType = "application/json";
            }
            param.timeOut=param.timeOut == null ? 5 : param.timeOut;
            param.timeOut = param.timeOut <= 0 ? 5 : param.timeOut;

            return new ResponseObject(true);
        }

        public string url { get; set; }
        /// <summary>
        /// 请求方法（默认为GET）,首字母大写
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// json格式参数 Get、Delete、Update没有此参数
        /// </summary>
        public dynamic data { get; set; }
        /// <summary>
        /// 传送方式有 例如：application/json、 multipart/form-data  
        /// </summary>
        public string contentType { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public int timeOut { get; set; }
    }
}