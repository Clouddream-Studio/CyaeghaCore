using CSR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Types;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace Cyaegha
{
    public class Dllmain
    {
        private delegate uint getPlatform(IntPtr playerPtr);
        private class Config
        {
            public string token { get; set; }
            public ChatId chatId { get; set; }
            public string serverName { get; set; }
            public List<long> opsId { get; set; }
            public string language { get; set; }
            public Dictionary<string, int> getplatformptrs { get; set; }
        }
        private class LanguagePack
        {
            public string start { get; set; }
            public string messagetoserver { get; set; }
            public string messagetochat { get; set; }
            public string connected { get; set; }
            public string disconnected { get; set; }
            public string dead { get; set; }
            public string killed { get; set; }
            public string feedback { get; set; }
            public string notop { get; set; }
        }
        public static unsafe void onLoad(MCNETAPI api)
        {
            Deserializer deserializer = new Deserializer();
            Config config = new Config
            {
                serverName = Regex.Replace(GetVaule("server.properties", "server-name"), "§.", string.Empty),
                language = "zh",
                getplatformptrs = new Dictionary<string, int> { { "1.17.32.02", 12005248 } }
            };
            LanguagePack langPack = new LanguagePack
            {
                start = "[%n] 服务器已开启",
                messagetoserver = "[Telegram] <%p> %m",
                messagetochat = "[%n] <%p> %m",
                connected = "[%n] %p 使用 %d 加入了服务器",
                disconnected = "[%n] %p 退出了服务器",
                dead = "[%n] %p 死了",
                killed = "[%n] %p 被 %s 杀死了",
                feedback = "[%n] %f",
                notop = "[%n] 你不是管理员"
            };
            LanguagePack langPacken = new LanguagePack
            {
                start = "[%n] Server Started",
                messagetoserver = "[Telegram] <%p> %m",
                messagetochat = "[%n] <%p> %m",
                connected = "[%n] %p joined the server using %d",
                disconnected = "[%n] %p left the game",
                dead = "[%n] %p dead",
                killed = "[%n] %p killed by %s",
                feedback = "[%n] %f",
                notop = "[%n] You're not an operator"
            };
            try
            {
                if (!Directory.Exists("plugins\\Cyaegha"))
                {
                    Directory.CreateDirectory("plugins\\Cyaegha");
                    Directory.CreateDirectory("plugins\\Cyaegha\\LanguagePack");
                    File.WriteAllText("plugins\\Cyaegha\\config.yaml", new Serializer().Serialize(config));
                    File.WriteAllText("plugins\\Cyaegha\\LanguagePack\\zh.yaml", new Serializer().Serialize(langPack));
                    File.WriteAllText("plugins\\Cyaegha\\LanguagePack\\en.yaml", new Serializer().Serialize(langPacken));
                    Console.WriteLine($"[{DateTime.Now} WARN] [CYA] 请先配置位于\"plugins\\Cyaegha\\config.yaml\"的配置文件并重启服务器以使用本插件");
                }
                else
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    config = deserializer.Deserialize<Config>(File.ReadAllText("plugins\\Cyaegha\\config.yaml"));
                    config.serverName = config.serverName == string.Empty ? Regex.Replace(GetVaule("server.properties", "server-name"), "§.", string.Empty) : config.serverName;
                    langPack = deserializer.Deserialize<LanguagePack>(File.ReadAllText($"plugins\\Cyaegha\\LanguagePack\\{config.language}.yaml"));
                    bool runcmded = false;
                    TelegramBotClient botClient = new TelegramBotClient(config.token);
                    botClient.StartReceiving();
                    botClient.SendTextMessageAsync(config.chatId, langPack.start.Replace("%n", config.serverName));
                    User me = botClient.GetMeAsync().Result;
                    Console.WriteLine($"[{DateTime.Now} INFO] [CYA] ID：{me.Id}  名称：{me.FirstName}");
                    botClient.OnMessage += (sender, e) =>
                    {
                        if (e.Message.Chat.Id == config.chatId)
                        {
                            if (e.Message.Text.StartsWith("/"))
                            {
                                if (e.Message.Text == $"/{config.serverName} list" || e.Message.Text == "/list")
                                {
                                    runcmded = true;
                                    api.runcmd("list");
                                    Timer timer = new Timer
                                    {
                                        Interval = 500,
                                        Enabled = true,
                                        AutoReset = false
                                    };
                                    timer.Elapsed += (sender1, e1) =>
                                    {
                                        runcmded = false;
                                    };
                                    timer.Start();
                                }
                                else if (e.Message.Text.StartsWith($"/{config.serverName} "))
                                {
                                    if (config.opsId.Contains(e.Message.From.Id))
                                    {
                                        runcmded = true;
                                        api.runcmd(e.Message.Text.Substring(3 + config.serverName.Length));
                                        Timer timer = new Timer
                                        {
                                            Interval = 500,
                                            Enabled = true,
                                            AutoReset = false
                                        };
                                        timer.Elapsed += (sender1, e1) =>
                                        {
                                            runcmded = false;
                                        };
                                        timer.Start();
                                    }
                                    else
                                    {
                                        botClient.SendTextMessageAsync(config.chatId, langPack.notop.Replace("%n", config.serverName));
                                    }
                                }
                            }
                            else
                            {
                                api.runcmd($"tellraw @a {{\"rawtext\":[{{\"text\":\"{langPack.messagetoserver.Replace("%p", e.Message.From.Username).Replace("%m", e.Message.Text.Replace("\"", "\\\""))}\"}}]}}");
                            }
                        }
                    };
                    api.addBeforeActListener("onInputText", es =>
                    {
                        InputTextEvent e = InputTextEvent.getFrom(es);
                        botClient.SendTextMessageAsync(config.chatId, langPack.messagetochat.Replace("%n", config.serverName).Replace("%p", e.playername).Replace("%m", e.msg));
                        return true;
                    });
                    api.addBeforeActListener("onLoadName", es =>
                    {
                        LoadNameEvent e = LoadNameEvent.getFrom(es);
                        string platform = "Unknown";
                        if (config.getplatformptrs.ContainsKey(api.VERSION))
                        {
                            platform = new List<string>
                            {
                                "Unknown",
                                "Android",
                                "iOS",
                                "macOS",
                                "FireOS",
                                "GearVR",
                                "HoloLens",
                                "UWP",
                                "Windows",
                                "Dedicated",
                                "tvOS",
                                "PlayStation",
                                "Switch",
                                "Xbox",
                                "WindowsMobile"
                            }[Convert.ToInt32(Marshal.GetDelegateForFunctionPointer<getPlatform>(api.dlsym(config.getplatformptrs[api.VERSION]))(e.playerPtr))];
                        }
                        else
                        {
                            MessageBox.Show($"警告！您未配置与您当前版本相应的地址！接下来可能会导致崩溃！\n源自：main -> GetPlatform", "Cyaegha - ERROR!!");
                        }
                        botClient.SendTextMessageAsync(config.chatId, langPack.connected.Replace("%n", config.serverName).Replace("%p", e.playername).Replace("%d", platform));
                        return true;
                    });
                    api.addBeforeActListener("onPlayerLeft", es =>
                    {
                        botClient.SendTextMessageAsync(config.chatId, langPack.disconnected.Replace("%n", config.serverName).Replace("%p", PlayerLeftEvent.getFrom(es).playername));
                        return true;
                    });
                    api.addBeforeActListener("onMobDie", es =>
                    {
                        MobDieEvent e = MobDieEvent.getFrom(es);
                        if (e.playername != null)
                        {
                            if (e.srctype == string.Empty)
                            {
                                botClient.SendTextMessageAsync(config.chatId, langPack.dead.Replace("%n", config.serverName).Replace("%p", e.playername));
                            }
                            else if (e.srcname == string.Empty)
                            {
                                botClient.SendTextMessageAsync(config.chatId, langPack.killed.Replace("%n", config.serverName).Replace("%p", e.playername).Replace("%s", e.srctype.Split('.')[1]));
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(config.chatId, langPack.killed.Replace("%n", config.serverName).Replace("%p", e.playername).Replace("%s", e.srcname));
                            }
                        }
                        return true;
                    });
                    api.addBeforeActListener("onServerCmdOutput", es =>
                    {
                        if (runcmded)
                        {
                            botClient.SendTextMessageAsync(config.chatId, langPack.feedback.Replace("%n", config.serverName).Replace("%f", ServerCmdOutputEvent.getFrom(es).output));
                            runcmded = false;
                            return false;
                        }
                        return true;
                    });
                    Console.WriteLine($"[{DateTime.Now} INFO] [CYA] 载入成功");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private static string GetVaule(string path, string key)
        {
            foreach (string text in File.ReadAllLines(path))
            {
                if (!text.StartsWith("#") && text.Split('=')[0] == key)
                {
                    return text.Split('=')[1];
                }
            }
            return null;
        }
    }
}
