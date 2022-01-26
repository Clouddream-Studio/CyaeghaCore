using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BDSAddrApi
{
    internal class BDSAddressWebAPI
    {
        public static string APIURLs = "https://cngege.github.io/BDSMoudles/apis.json";

        //建议线程中调用
        public static bool GetAddress_Try(string VERSION, string[] symbols, out int[] addrs)
        {
            return GetAddress_Try(VERSION, symbols, out addrs, new Option());
        }

        public static bool GetAddress_Try(string VERSION, string[] symbols, out int[] addrs, Option option)
        {
            if (option.localapiinfo)
            {
                Directory.CreateDirectory(option.tmppath);
            }

            if (option.symbol_save)
            {
                if (!File.Exists($"{option.tmppath}{VERSION}.json"))
                {
                    File.Create($"{option.tmppath}{VERSION}.json").Close();
                }
                else
                {
                    string symbolstr = File.ReadAllText($"{option.tmppath}{VERSION}.json");
                    if (!string.IsNullOrEmpty(symbolstr))
                    {
                        dynamic obj = JsonConvert.DeserializeObject(symbolstr);
                        int state = 0;
                        int[] addr = new int[symbols.Length];
                        for (int i = 0; i < symbols.Length; i++)
                        {
                            if (!IsPropertyExist(obj, symbols[i]) || string.IsNullOrEmpty($"{obj[symbols[i]]}"))
                            {
                                //远程获取
                                state = 1;
                                break;
                            }
                            addr[i] = Convert.ToInt32(obj[symbols[i]]);
                        }
                        if (state == 0) //
                        {
                            addrs = addr;
                            return true;
                        }
                    }
                }
            }

            string localapi = string.Empty;

            string symbolsstr = string.Join(",", symbols);                          //将符号数组拼接成字符串以便查询

            if (option.localapiinfo && File.Exists($"{option.tmppath}api.txt"))
            {
                localapi = File.ReadAllText($"{option.tmppath}api.txt").Trim();
                //如果本地存储的api获取失败,则远程重新获取并选择一个有效的获取地址并保存本地
                BackAddrData address = GetAddressInfoOFAPI($"{localapi}?version={VERSION}&{((symbols.Length == 1) ? "key" : "keys")}={symbolsstr}");
                if (address != null)
                {
                    if (address.code == 1)
                    {
                        addrs = null;
                        return false;                                               //要提示检查key是否正确,并稍候重试
                    }
                    else if (address.code == 200)
                    {

                        if (symbols.Length == 1)
                        {
                            addrs = new int[]
                            {
                                Convert.ToInt32(address.value, 16)
                            };
                        }
                        else
                        {
                            addrs = address.values.ConvertAll(new Converter<string, int>((i) => Convert.ToInt32(i, 16))).ToArray();
                        }
                        if (option.symbol_save)
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                FileStream stream = null;
                                try
                                {
                                    stream = new FileStream($"{option.tmppath}{VERSION}.json", FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                                }
                                catch
                                {
                                    stream?.Close();
                                    continue;
                                }
                                string symstr = ReadAllTextFromStream(stream);
                                if (string.IsNullOrEmpty(symstr))
                                {
                                    symstr = "{}";
                                }
                                dynamic symjson = JsonConvert.DeserializeObject(symstr);
                                for (int j = 0; j < symbols.Length; j++)
                                {
                                    symjson[symbols[j]] = addrs[j];
                                }
                                symstr = JsonConvert.SerializeObject(symjson);
                                byte[] b = Encoding.UTF8.GetBytes(symstr);
                                stream.Write(b, 0, b.Length);
                                stream.Close();
                                break;
                            }
                        }
                        return true;
                    }
                }
            }
            string apisjsonstr = getHttpData(APIURLs);                        //通过Web查询 获取API节点
            if (string.IsNullOrEmpty(apisjsonstr))
            {
                addrs = null;
                return false;
            }
            foreach (Apilist item in JsonConvert.DeserializeObject<List<Apilist>>(apisjsonstr))
            {
                if (option.localapiinfo && item.url == localapi)
                {
                    continue;
                }
                BackAddrData address = GetAddressInfoOFAPI($"{item.url}?version={VERSION}&{((symbols.Length == 1) ? "key" : "keys")}={symbolsstr}");
                if (address == null)
                {
                    continue;
                }
                switch (address.code)
                {
                    case 200:
                        {
                            if (symbols.Length == 1)
                            {
                                addrs = new int[]
                                {
                                    Convert.ToInt32(address.value, 16)
                                };
                            }
                            else
                            {
                                addrs = address.values.ConvertAll(new Converter<string, int>((i) => Convert.ToInt32(i, 16))).ToArray();
                            }
                            if (option.symbol_save)
                            {
                                for (int i = 0; i < 200; i++)
                                {
                                    FileStream stream = null;
                                    try
                                    {
                                        stream = new FileStream($"{option.tmppath}{VERSION}.json", FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                                    }
                                    catch
                                    {
                                        stream?.Close();
                                        continue;
                                    }
                                    string symstr = ReadAllTextFromStream(stream);
                                    if (string.IsNullOrEmpty(symstr))
                                    {
                                        symstr = "{}";
                                    }
                                    dynamic symjson = JsonConvert.DeserializeObject(symstr);
                                    for (int j = 0; j < symbols.Length; j++)
                                    {
                                        symjson[symbols[j]] = addrs[j];
                                    }
                                    symstr = JsonConvert.SerializeObject(symjson);
                                    byte[] b = Encoding.UTF8.GetBytes(symstr);
                                    stream.Write(b, 0, b.Length);
                                    stream.Close();
                                    break;
                                }
                            }
                            if (option.localapiinfo)
                            {
                                File.WriteAllText($"{option.tmppath}api.txt", item.url);
                            }

                            return true;
                            //写本地
                        }

                    case 1:
                        //写本地
                        if (option.localapiinfo)
                        {
                            File.WriteAllText($"{option.tmppath}api.txt", item.url);
                        }

                        addrs = null;
                        return false;                                               //要提示检查key是否正确,并稍候重试
                    default:
                        continue;
                }

            }
            addrs = null;
            return false;                                                       //要提示检查key是否正确,并稍候重试
        }

        private static string ReadAllTextFromStream(FileStream stream)
        {
            byte[] heByte = new byte[stream.Length];
            stream.Read(heByte, 0, heByte.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString(heByte);
        }

        private static bool IsPropertyExist(dynamic data, string propertyname)
        {
            return data.ContainsKey(propertyname);
        }

        /// <summary>
        /// 从云端数据库中获取符号地址信息
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        private static BackAddrData GetAddressInfoOFAPI(string api)
        {
            string addrjsonsstr = getHttpData(api);
            return string.IsNullOrEmpty(addrjsonsstr) ? null : JsonConvert.DeserializeObject<BackAddrData>(addrjsonsstr);
        }

        private static string getHttpData(string Url, WebHeaderCollection Headers = null)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebRequest request = WebRequest.Create(Url);
                //声明一个HttpWebRequest请求
                request.Timeout = 30 * 1000;  //设置连接超时时间
                if (Headers != null)
                {
                    request.Headers = Headers;
                }
                else
                {
                    request.Headers.Set("Pragma", "no-cache");
                }
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                if (!string.IsNullOrWhiteSpace($"{response}"))
                {
                    return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                }
            }
            catch { }
            return null;
        }
    }

    internal class Option
    {
        /// <summary>
        /// 是否优先使用保存本地api节点缓存,且保存本地api节点。解释:由于api节点列表挂载在GitHub，从GitHub获取节点时而出错,所以可尝试在本地保存一份节点，且下次直接从本地读取
        /// </summary>
        public bool localapiinfo = true;

        /// <summary>
        /// 如果保存或使用本地的一些信息,那么就在这个目录下进行
        /// </summary>
        public string tmppath = "plugins\\BDSAddressApi\\";

        /// <summary>
        /// 优先从本地库文件读取符号地址，如果没有则远程获取，获取之后再保存在本地<br/>一来节约云端资源，二来提高读取成功率，加快读取速度,即使云端服务器短时间出了问题,也不会影响插件的使用
        /// </summary>
        public bool symbol_save = true;
    }

    internal class Apilist
    {
        public string url { get; set; }
        public string manager { get; set; }
        public string home { get; set; }
    }

    internal class BackAddrData
    {
        public int code { get; set; }
        public string message { get; set; }
        public string value { get; set; }
        public List<string> values { get; set; }
        public object error { get; set; }
    }
}
