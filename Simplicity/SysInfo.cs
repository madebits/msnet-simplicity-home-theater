using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Simplicity
{
    class SysInfo
    {
        public static SysInfo Default = new SysInfo();
        public string[] GetInfo()
        {
            List<string> data = new List<string>();
            DateTime now = DateTime.Now;
            data.Add(Config.AppNameShort + " " + Config.AppVersion);
            data.Add(Config.AppUrl);
            data.Add("EXE: " + Config.Default.ExePath);
            data.Add("MD5: " + HashFile(Application.ExecutablePath));
            data.Add("Time: " + now.ToString("HH:mm yyyy-MM-dd"));
            string osv = Environment.OSVersion.ToString();
            osv = osv.Replace("Microsoft Windows", string.Empty);
            osv = osv.Replace("Service Pack", "SP");
            osv += (" " + IntPtr.Size * 8 + " bit");
            data.Add("OS: " + osv);
            data.Add(".NET: " + Environment.Version.ToString());
            try
            {
                Program.Win32.MEMORYSTATUSEX memStatus = new Program.Win32.MEMORYSTATUSEX();
                if (Program.Win32.GlobalMemoryStatusEx(memStatus))
                {
                    
                    data.Add("Memory Load: " + memStatus.dwMemoryLoad + "%");
                    data.Add("Memory Total: " + Config.SizeStr(memStatus.ullTotalPhys, null));
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            
            try
            {
                string myHost = System.Net.Dns.GetHostName();
                System.Net.IPHostEntry myIPs = System.Net.Dns.GetHostEntry(myHost);
                foreach (System.Net.IPAddress ip in myIPs.AddressList)
                {
                    if (System.Net.IPAddress.IsLoopback(ip)) continue;
                    if (ip.GetAddressBytes().Length > 4)
                    {
                        continue;
                    }
                    data.Add("IP: " + ip.ToString());
                }
                foreach (System.Net.IPAddress ip in myIPs.AddressList)
                {
                    if (System.Net.IPAddress.IsLoopback(ip)) continue;
                    if (ip.GetAddressBytes().Length <= 4)
                    {
                        continue;
                    }
                    data.Add("IP: " + ip.ToString());
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            try
            {
                data.Add("Internal Version: " + this.GetType().Assembly.FullName);
            }
            catch { }
            data.Add("Run-Time Errors: " + Config.Default.errorCount);
            return data.ToArray();
        }

        public static string HashFile(string file)
        {
            try
            {
                System.Security.Cryptography.HashAlgorithm hasher =
                    new System.Security.Cryptography.MD5CryptoServiceProvider();
                using (Stream s = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024))
                {
                    byte[] h = hasher.ComputeHash(s);
                    StringBuilder sb = new StringBuilder(h.Length * 2);
                    for (int i = 0; i < h.Length; i++)
                    {
                        sb.AppendFormat("{0:x2}", h[i]);
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
            return string.Empty;
        }

        public void HandleSelect(int index)
        {
            try
            {
                switch(index)
                {
                    case 0:
                    case 1:
                        System.Diagnostics.Process.Start(Config.AppUrl);
                        break;
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

    }
}
