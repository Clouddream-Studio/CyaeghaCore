/*
 * 由SharpDevelop创建。
 * 用户： BDSNetRunner
 * 日期: 2020/7/18
 * 时间: 12:32
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace CSR
{
    internal partial class Plugin
    {
        /// <summary>
        /// 静态api对象
        /// </summary>
        public static MCNETAPI api { get; set; }
        #region 插件统一调用接口，请勿随意更改
        public static int onServerStart(string pathandversion)
        {
            string[] pav = pathandversion.Split(',');
            if (pav.Length > 1)
            {
                api = new MCNETAPI(pav[0], pav[1], pav[pav.Length - 1] == "1");
                if (api != null)
                {
                    Cyaegha.Dllmain.onLoad(api);
                    GC.KeepAlive(api);
                    return 0;
                }
            }
            Console.WriteLine("Load failed.");
            return -1;
        }
        #endregion

        ~Plugin()
        {
            //Console.WriteLine("[DNR Plugin] Ref released.");
        }
    }
}