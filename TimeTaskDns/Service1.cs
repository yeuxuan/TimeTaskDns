using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TimeTaskDns
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        //模式选择
        private static dnsMode _dnsOption = dnsMode.Radom;

        private static readonly string _optionFile = @"D:\TimeTaskDns\dns.txt";

        private static string _fileMd5 = "";

        private static readonly List<string> _optionsSafeDns = new List<string>() {
            "【One Dns】117.50.11.11:52.80.66.66",
            "【One Dns】117.50.11.11:117.50.22.22"
        };
        private static readonly List<string> _optionsPueDns = new List<string>() {
            "【One Dns】117.50.10.10:52.80.52.52",
            "【One Dns】117.50.10.10:117.50.20.20"
        };
        private static readonly List<string> _optionsHomeDns = new List<string>() {
            "【One Dns】117.50.60.30:52.80.60.30"
        };
        private static readonly List<string> _optionsRadDns = new List<string>() {
            "【114 Dns】101.198.198.198:114.114.114.114",
             "【Google Dns】8.8.8.8:114.114.114.114",
              "【360 Dns】101.226.4.6:218.30.118.6",
              "【360 Dns】123.125.81.6:140.207.198.6",
              "【Alibaba】223.5.5.5:223.6.6.6 ",
              "【Baidu】180.76.76.76:114.114.114.114",
              "【Cloudflare】1.1.1.1:1.0.0.1",
              "【CNNIC DNS】1.2.4.8:210.2.4.8"
        };
        private static readonly List<string> _optionstxDns = new List<string>() {
            "【Tx Dns】119.29.29.29:182.254.116.116"
        };
        private static readonly List<string> _optionszdyDns = new List<string>() {
        };

        /// <summary>
        /// Safe 拦截模式  Pue 纯净模式 Home 家庭模式 Radom 自动模式 腾讯游戏Dns
        /// </summary>
        public enum dnsMode
        {
            Safe = 0, Pue = 1, Home = 2, Radom = 3, tx = 4,zdy=5
        }
        protected override void OnStart(string[] args)
        {
            Task.Run(() => {
                MainDns();
            });
        }

        private static void MainDns() {

            while (true)
            {
                CheckFileExists();

                //if (_fileMd5.Equals(GetMD5HashFromFile(_optionFile)))
                //{
                //    Thread.Sleep(TimeSpan.FromMinutes(1));
                //    continue;
                //}
                using (StreamReader stramRead = new StreamReader(_optionFile))
                {
                    string line = "";
                    Regex regex = new Regex(@"((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}");
                    while ((line = stramRead.ReadLine()) != null)
                    {
                        if (line.Contains("setDns")&&regex.IsMatch(line))
                        {
                            var ip = regex.Match(line);
                            if (!_optionszdyDns.Contains($"{ip.Value}:{ip.NextMatch()}")) {
                                _optionszdyDns.Add($"{ip.Value}:{ip.NextMatch()}");
                            }
                            _dnsOption = dnsMode.zdy;
                            break;
                        }
                        if (line.Contains("dnsMode=Radom"))
                        {
                            _dnsOption = dnsMode.Radom;
                            break;
                        }
                        else if (line.Contains("dnsMode=Safe"))
                        {
                            _dnsOption = dnsMode.Safe;
                            break;
                        }
                        else if (line.Contains("dnsMode=Pue"))
                        {
                            _dnsOption = dnsMode.Pue;
                            break;
                        }
                        else if (line.Contains("dnsMode=Home"))
                        {
                            _dnsOption = dnsMode.Home;
                            break;
                        }
                        else if (line.Contains("dnsMode=tx"))
                        {
                            _dnsOption = dnsMode.tx;
                            break;
                        }

                    }
                }
                DnsSpeed();
                Task.Delay(TimeSpan.FromMinutes(60)).Wait();
                
            }
        
        
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="file">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string file)
        {
            try
            {
                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();
                foreach (var byteMd5 in retVal)
                {
                    sb.Append(byteMd5.ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("获取文件MD5值error:" + ex.Message);
            }
        }

        public static List<string> GetDnsList(dnsMode mode)
        {
            List<string> dnslist = new List<string>();
            switch (mode)
            {
                case dnsMode.Safe:
                    dnslist = _optionsSafeDns;
                    break;
                case dnsMode.Pue:
                    dnslist = _optionsPueDns;
                    break;
                case dnsMode.Home:
                    dnslist = _optionsHomeDns;
                    break;
                case dnsMode.Radom:
                    dnslist = _optionsRadDns;
                    break;
                case dnsMode.tx:
                    dnslist = _optionstxDns;
                    break;
                case dnsMode.zdy:
                    dnslist = _optionszdyDns;
                    break;
            }
            return dnslist;
        }

        public static void DnsSpeed()
        {


            Dictionary<string, long> dnsDic = new Dictionary<string, long>();

            string[] dnslist = GetDnsList(_dnsOption).ToArray();

            foreach (var dns in dnslist)
            {
                string[] list = dns.Split(':');

                for (int i = 0; i < list.Length; i++)
                {
                    list[0] = list[0].Replace("【One Dns】", "").Replace("【114 Dns】", "").Replace("【Google Dns】", "").Replace("【360 Dns】", "").Replace("【Alibaba】", "").Replace("【Baidu】", "").Replace("【Cloudflare】", "").Replace("【CNNIC DNS】", "").Replace("【Tx Dns】", "").Trim();
                }
                //设置成功后开始测速
                if (SetDNS(list))
                {
                    dnsDic.Add(dns.Replace("【One Dns】", "").Replace("【114 Dns】", "").Replace("【Google Dns】", "").Replace("【360 Dns】", "").Replace("【Alibaba】", "").Replace("【Baidu】", "").Replace("【Cloudflare】", "").Replace("【CNNIC DNS】", "").Replace("【Tx Dns】", "").Trim(), LongPingMin());
                }
            }

            var dicSort = from dic in dnsDic orderby dic.Value ascending select dic;
            var dnsList = GetActiveEthernetOrWifiNetworkInterface().GetIPProperties().DnsAddresses;
            string dnsStr = dnsList[0]+":"+ dnsList[1];
            if (!dnsStr.Equals(dicSort.ToList()[0].Key))
            {
                SetDNS(dicSort.ToList()[0].Key.Split(':'));
               // _fileMd5 = GetMD5HashFromFile(_optionFile);//更新MD5值
                LogLevel.Info.FullOutput("设置Dns", JsonConvert.SerializeObject(dicSort.ToList(), Formatting.Indented));
            }
            else
            {
                Thread.Sleep(TimeSpan.FromMinutes(10));
                return;
            }
         
        }

        public static long LongPingMin()
        {
            string[] ipStr = new string[] { "www.baidu.com", "www.qq.com", "www.sina.cn", "www.bilibili.com" };
            //构造Ping实例
            Ping pingSender = new Ping();
            //Ping 选项设置
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            //测试数据
            string data = "test data abcabcsdadasdasdsadfsadsadasdadasdasdsadasdasdasdasdasdsadasdasdas";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            //设置超时时间
            int timeout = 2000;
            long min = long.MaxValue;
            foreach (var host in ipStr)
            {
                long timeSum = 0;
                int count = 0;
                for (int i = 0; i < 5; i++)
                {
                    //调用同步 send 方法发送数据,将返回结果保存至PingReply实例
                    PingReply reply = pingSender.Send(host, timeout, buffer, options);
                    long time = reply.RoundtripTime;
                    if (reply.Status == IPStatus.Success)
                    {
                        timeSum += time; count++;
                    }
                    Task.Delay(1000);
                }
                long minSum = timeSum / count;
                if (minSum < min)
                {
                    min = minSum;
                }
            }
            return min;
        }


        public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
        {
            var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

            return Nic;
        }
        public static bool SetDNS(string[] DnsString)
        {
            string[] Dns = null;
            if (DnsString.Length == 2)
            {
                Dns = DnsString;
            }
            else
            {
                return false;
            }
            var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return false;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            objdns["DNSServerSearchOrder"] = Dns;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void CheckFileExists()
        {
            if (!Directory.Exists(@"D:\TimeTaskDns"))
            {
                Directory.CreateDirectory(@"D:\TimeTaskDns");
            }
            if (!File.Exists(@"D:\first.txt"))
            {
                OptionWrite();
                _fileMd5 = GetMD5HashFromFile(_optionFile);
            }
        }

        public static void OptionWrite()
        {

            //拦截DNS
            using (StreamWriter streamWriter = new StreamWriter(_optionFile))
            {
                FileStream file = new FileStream(@"D:\first.txt", FileMode.Create);
                streamWriter.WriteLine($"DNS自动测速服务配置文件\n\n格式为：主Dns:副Dns\n\n\n");
                streamWriter.WriteLine($"-------------------DNS配置区------------------\n");
                streamWriter.WriteLine($"setDns=auto //auto代表自动  指定的Dns格式为 格式为：主Dns:副Dns");
                streamWriter.WriteLine($"dnsMode=Radom //Safe 拦截模式  Pue 纯净模式 Home 家庭模式 Radom 自动模式 tx 腾讯游戏Dns\n\n\n");

                streamWriter.WriteLine("【拦截DNS】");
                foreach (string safe in _optionsSafeDns)
                {

                    streamWriter.WriteLine(safe);

                }

                streamWriter.WriteLine("【纯净DNS】");
                foreach (string pue in _optionsPueDns)
                {

                    streamWriter.WriteLine(pue);

                }

                streamWriter.WriteLine("【家庭DNS】");
                foreach (string home in _optionsHomeDns)
                {

                    streamWriter.WriteLine(home);

                }

                streamWriter.WriteLine("【自动DNS】");
                foreach (string zd in _optionsRadDns)
                {

                    streamWriter.WriteLine(zd);

                }


                streamWriter.WriteLine("【腾讯DNS】");
                foreach (string tx in _optionstxDns)
                {

                    streamWriter.WriteLine(tx);

                }

               
            }

        }


        protected override void OnStop()
        {


        }
    }
}
