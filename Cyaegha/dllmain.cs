using BDSAddrApi;
using CSR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace CyaeghaCore
{
    public class Dllmain
    {
        private class Config
        {
            public string token { get; set; }
            public ChatId chatId { get; set; }
            public List<long> opsId { get; set; }
            public string language { get; set; }
            public string proxy { get; set; }
        }
        private class LanguagePack
        {
            public class MessageType
            {
                public string unknown { get; set; }
                public string photo { get; set; }
                public string audio { get; set; }
                public string video { get; set; }
                public string voice { get; set; }
                public string document { get; set; }
                public string sticker { get; set; }
                public string location { get; set; }
                public string contact { get; set; }
                public string venue { get; set; }
                public string game { get; set; }
                public string videonote { get; set; }
                public string invoice { get; set; }
                public string successfulpayment { get; set; }
                public string websiteconnected { get; set; }
                public string chatmembersadded { get; set; }
                public string chatmemberleft { get; set; }
                public string chattitlechanged { get; set; }
                public string chatphotochanged { get; set; }
                public string messagepinned { get; set; }
                public string chatphotodeleted { get; set; }
                public string groupcreated { get; set; }
                public string supergroupcreated { get; set; }
                public string channelcreated { get; set; }
                public string migratedtosupergroup { get; set; }
                public string migratedfromgroup { get; set; }
                public string poll { get; set; }
                public string dice { get; set; }
                public string messageautodeletetimerchanged { get; set; }
                public string proximityalerttriggered { get; set; }
                public string voicechatscheduled { get; set; }
                public string voicechatstarted { get; set; }
                public string voicechatended { get; set; }
                public string voicechatparticipantsinvited { get; set; }
            }
            public string start { get; set; }
            public string messagetoserver { get; set; }
            public string messagetochat { get; set; }
            public string connected { get; set; }
            public string disconnected { get; set; }
            public string dead { get; set; }
            public string killed { get; set; }
            public string feedback { get; set; }
            public string notop { get; set; }
            public MessageType messagetype { get; set; }
        }
        private delegate uint getPlatform(IntPtr playerPtr);
        public static unsafe void onLoad(MCNETAPI api)
        {
            int playerCount = 0;
            Deserializer deserializer = new Deserializer();
            Config config = new Config
            {
                chatId = 0,
                language = "zh",
                opsId = new List<long>
                {
                    0
                },
                proxy = "127.0.0.1",
                token = "1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"
            };
            LanguagePack langPack = new LanguagePack
            {
                start = "聊天互通已开始",
                messagetoserver = "[Telegram]<%p> %m",
                messagetochat = "<%p> %m",
                connected = "%p 使用 %d 加入了服务器 当前在线%c人",
                disconnected = "%p 退出了服务器 当前在线%c人",
                dead = "%p 死了",
                killed = "%p 被 %s 杀死了",
                feedback = "%f",
                notop = "你不是管理员",
                messagetype = new LanguagePack.MessageType
                {
                    audio = "§o音频文件",
                    channelcreated = "§o创建频道",
                    chatmemberleft = "§o退出群聊",
                    chatmembersadded = "§o邀请加入群聊",
                    chatphotochanged = "§o修改群头像",
                    chatphotodeleted = "§o删除群头像",
                    chattitlechanged = "§o修改群聊名称",
                    contact = "§o用户名片（%n）",
                    dice = "§o骰子（%v）",
                    document = "§o文件（%n）",
                    game = "§o游戏",
                    groupcreated = "§o创建群聊",
                    invoice = "§o发票",
                    location = "§o定位",
                    messageautodeletetimerchanged = "§o修改定时删除消息",
                    messagepinned = "§o置顶消息",
                    migratedfromgroup = "§o迁移自群聊",
                    migratedtosupergroup = "§o迁移自超级群聊",
                    photo = "§o图片",
                    poll = "§o投票（%q）",
                    proximityalerttriggered = "§o触发接近警报",
                    sticker = "§o表情",
                    successfulpayment = "§o支付成功",
                    supergroupcreated = "§o创建超级群聊",
                    unknown = "§o未知消息",
                    venue = "§o地点",
                    video = "§o视频文件",
                    videonote = "§o视频消息",
                    voice = "§o语音消息（%d秒）",
                    voicechatended = "§o语言通话结束",
                    voicechatparticipantsinvited = "§o语言通话邀请",
                    voicechatscheduled = "§o语言通话日程",
                    voicechatstarted = "§o语言通话开始",
                    websiteconnected = "§o授权网页"
                }
            };
            LanguagePack langPacken = new LanguagePack
            {
                start = "Chat server started",
                messagetoserver = "[Telegram]<%p> %m",
                messagetochat = "<%p> %m",
                connected = "%p joined the server using %d, now %c player(s) online",
                disconnected = "%p left the game, now %c player(s) online",
                dead = "%p dead",
                killed = "%p killed by %s",
                feedback = "%f",
                notop = "You're not an admin",
                messagetype = new LanguagePack.MessageType
                {
                    audio = "§o音频文件",
                    channelcreated = "§o创建频道",
                    chatmemberleft = "§o退出群聊",
                    chatmembersadded = "§o加入群聊",
                    chatphotochanged = "§o修改群头像",
                    chatphotodeleted = "§o删除群头像",
                    chattitlechanged = "§o修改群聊名称",
                    contact = "§o用户名片（%n）",
                    dice = "§o骰子（%v）",
                    document = "§o文件（%n）",
                    game = "§o游戏",
                    groupcreated = "§o创建群聊",
                    invoice = "§o发票",
                    location = "§o定位",
                    messageautodeletetimerchanged = "§o修改定时删除消息",
                    messagepinned = "§o置顶消息",
                    migratedfromgroup = "§o迁移自群聊",
                    migratedtosupergroup = "§o迁移自超级群聊",
                    photo = "§o图片",
                    poll = "§o投票（%q）",
                    proximityalerttriggered = "§o触发接近警报",
                    sticker = "§o表情",
                    successfulpayment = "§o支付成功",
                    supergroupcreated = "§o创建超级群聊",
                    unknown = "§o未知消息",
                    venue = "§o地点",
                    video = "§o视频文件",
                    videonote = "§o视频消息",
                    voice = "§o语音消息（%d秒）",
                    voicechatended = "§o语言通话结束",
                    voicechatparticipantsinvited = "§o语言通话邀请",
                    voicechatscheduled = "§o语言通话日程",
                    voicechatstarted = "§o语言通话开始",
                    websiteconnected = "§o授权网页"
                }
            };
            if (!File.Exists("plugins\\CyaeghaCore\\config.yaml"))
            {
                Directory.CreateDirectory("plugins\\CyaeghaCore");
                Directory.CreateDirectory("plugins\\CyaeghaCore\\LanguagePack");
                File.WriteAllText("plugins\\CyaeghaCore\\config.yaml", new Serializer().Serialize(config));
                File.WriteAllText("plugins\\CyaeghaCore\\LanguagePack\\zh.yaml", new Serializer().Serialize(langPack));
                File.WriteAllText("plugins\\CyaeghaCore\\LanguagePack\\en.yaml", new Serializer().Serialize(langPacken));
                Logger.Trace("请先配置位于\"plugins\\CyaeghaCore\\config.yaml\"的配置文件，配置完毕后按任意键继续", Logger.LogLevel.WARN);
                Console.Read();
            }
            try
            {
                BDSAddressWebAPI.GetAddress_Try(Plugin.api.VERSION, new string[] { "?getPlatform@Player@@QEBA?AW4BuildPlatform@@XZ" }, out int[] address);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                config = deserializer.Deserialize<Config>(File.ReadAllText("plugins\\CyaeghaCore\\config.yaml"));
                langPack = deserializer.Deserialize<LanguagePack>(File.ReadAllText($"plugins\\CyaeghaCore\\LanguagePack\\{config.language}.yaml"));
                bool runcmded = false;
                TelegramBotClient botClient = new TelegramBotClient(config.token, new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy(config.proxy, true)
                }));
                botClient.SendTextMessageAsync(config.chatId, langPack.start);
                User me = botClient.GetMeAsync().Result;
                Logger.Trace($"用户名：{me.Username}  名称：{me.FirstName}");
                botClient.StartReceiving((botClient1, update, cancellationToken) =>
                {
                    if (update.Message.Chat.Id == config.chatId)
                    {
                        string outmsg = string.Empty;
                        if (update.Type == UpdateType.Message)
                        {
                            switch (update.Message.Type)
                            {
                                case MessageType.Text:
                                    {
                                        if (update.Message.Text.StartsWith("/"))
                                        {
                                            if (update.Message.Text == "/list" || update.Message.Text == $"/list@{me.Username}")
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
                                            else if (update.Message.Text.EndsWith($"@{me.Username}"))
                                            {
                                                if (config.opsId.Contains(update.Message.From.Id))
                                                {
                                                    runcmded = true;
                                                    api.runcmd(update.Message.Text.Substring(1, update.Message.Text.Length - me.Username.Length - 1));
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
                                                    botClient1.SendTextMessageAsync(config.chatId, langPack.notop);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            outmsg = update.Message.Text;
                                        }
                                        break;
                                    }
                                case MessageType.Unknown:
                                    outmsg = langPack.messagetype.unknown;
                                    break;
                                case MessageType.Photo:
                                    outmsg = langPack.messagetype.photo;
                                    break;
                                case MessageType.Audio:
                                    outmsg = langPack.messagetype.audio.Replace("%n", update.Message.Audio.FileName);
                                    break;
                                case MessageType.Video:
                                    outmsg = langPack.messagetype.video;
                                    break;
                                case MessageType.Voice:
                                    outmsg = langPack.messagetype.voice.Replace("%d", $"{update.Message.Voice.Duration}");
                                    break;
                                case MessageType.Document:
                                    outmsg = langPack.messagetype.document.Replace("%n", update.Message.Document.FileName);
                                    break;
                                case MessageType.Sticker:
                                    outmsg = langPack.messagetype.sticker.Replace("%e", update.Message.Sticker.Emoji);
                                    break;
                                case MessageType.Location:
                                    outmsg = langPack.messagetype.location;
                                    break;
                                case MessageType.Contact:
                                    outmsg = langPack.messagetype.contact.Replace("%n", update.Message.Contact.FirstName);
                                    break;
                                case MessageType.Venue:
                                    outmsg = langPack.messagetype.venue.Replace("%t", update.Message.Venue.Title);
                                    break;
                                case MessageType.Game:
                                    outmsg = langPack.messagetype.game.Replace("%t", update.Message.Game.Title);
                                    break;
                                case MessageType.VideoNote:
                                    outmsg = langPack.messagetype.videonote;
                                    break;
                                case MessageType.Invoice:
                                    outmsg = langPack.messagetype.invoice.Replace("%t", update.Message.Invoice.Title);
                                    break;
                                case MessageType.SuccessfulPayment:
                                    outmsg = langPack.messagetype.successfulpayment;
                                    break;
                                case MessageType.WebsiteConnected:
                                    outmsg = langPack.messagetype.websiteconnected;
                                    break;
                                case MessageType.ChatMembersAdded:
                                    outmsg = langPack.messagetype.chatmembersadded;
                                    break;
                                case MessageType.ChatMemberLeft:
                                    outmsg = langPack.messagetype.chatmemberleft;
                                    break;
                                case MessageType.ChatTitleChanged:
                                    outmsg = langPack.messagetype.chattitlechanged;
                                    break;
                                case MessageType.ChatPhotoChanged:
                                    outmsg = langPack.messagetype.chatphotochanged;
                                    break;
                                case MessageType.MessagePinned:
                                    outmsg = langPack.messagetype.messagepinned;
                                    break;
                                case MessageType.ChatPhotoDeleted:
                                    outmsg = langPack.messagetype.chatphotodeleted;
                                    break;
                                case MessageType.GroupCreated:
                                    outmsg = langPack.messagetype.groupcreated;
                                    break;
                                case MessageType.SupergroupCreated:
                                    outmsg = langPack.messagetype.supergroupcreated;
                                    break;
                                case MessageType.ChannelCreated:
                                    outmsg = langPack.messagetype.channelcreated;
                                    break;
                                case MessageType.MigratedToSupergroup:
                                    outmsg = langPack.messagetype.migratedtosupergroup;
                                    break;
                                case MessageType.MigratedFromGroup:
                                    outmsg = langPack.messagetype.migratedfromgroup;
                                    break;
                                case MessageType.Poll:
                                    outmsg = langPack.messagetype.poll.Replace("%q", update.Message.Poll.Question);
                                    break;
                                case MessageType.Dice:
                                    outmsg = langPack.messagetype.dice.Replace("%v", $"{update.Message.Dice.Value}");
                                    break;
                                case MessageType.MessageAutoDeleteTimerChanged:
                                    outmsg = langPack.messagetype.messageautodeletetimerchanged;
                                    break;
                                case MessageType.ProximityAlertTriggered:
                                    outmsg = langPack.messagetype.proximityalerttriggered;
                                    break;
                                case MessageType.VoiceChatScheduled:
                                    outmsg = langPack.messagetype.voicechatscheduled;
                                    break;
                                case MessageType.VoiceChatStarted:
                                    outmsg = langPack.messagetype.voicechatstarted;
                                    break;
                                case MessageType.VoiceChatEnded:
                                    outmsg = langPack.messagetype.voicechatended;
                                    break;
                                case MessageType.VoiceChatParticipantsInvited:
                                    outmsg = langPack.messagetype.voicechatparticipantsinvited;
                                    break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(outmsg))
                        {
                            api.runcmd($"tellraw @a {{\"rawtext\":[{{\"text\":\"{langPack.messagetoserver.Replace("%p", $"{update.Message.From.FirstName}").Replace("%m", outmsg.Replace("\"", "\\\""))}\"}}]}}");
                        }
                    }
                }, (botClient2, exception, cancellationToken) =>
                {
                    Logger.Trace(exception.Message, Logger.LogLevel.ERROR);
                });
                api.addBeforeActListener("onInputText", es =>
                {
                    InputTextEvent e = InputTextEvent.getFrom(es);
                    botClient.SendTextMessageAsync(config.chatId, langPack.messagetochat.Replace("%p", e.playername).Replace("%m", e.msg));
                    return true;
                });
                api.addBeforeActListener("onLoadName", es =>
                {
                    LoadNameEvent e = LoadNameEvent.getFrom(es);
                    playerCount++;
                    string platform = new List<string>
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
                            "tvOS",
                            "PlayStation",
                            "Switch",
                            "Xbox",
                            "WindowsMobile"
                    }[Convert.ToInt32(Marshal.GetDelegateForFunctionPointer<getPlatform>(api.dlsym(address[0]))(e.playerPtr))];
                    botClient.SendTextMessageAsync(config.chatId, langPack.connected.Replace("%p", e.playername).Replace("%d", platform).Replace("%c", $"{playerCount}"));
                    return true;
                });
                api.addBeforeActListener("onPlayerLeft", es =>
                {
                    playerCount--;
                    botClient.SendTextMessageAsync(config.chatId, langPack.disconnected.Replace("%p", PlayerLeftEvent.getFrom(es).playername).Replace("%c", $"{playerCount}"));
                    return true;
                });
                api.addBeforeActListener("onMobDie", es =>
                {
                    MobDieEvent e = MobDieEvent.getFrom(es);
                    if (!string.IsNullOrWhiteSpace(e.playername))
                    {
                        if (string.IsNullOrWhiteSpace(e.srctype))
                        {
                            botClient.SendTextMessageAsync(config.chatId, langPack.dead.Replace("%p", e.playername));
                        }
                        else if (string.IsNullOrWhiteSpace(e.srcname))
                        {
                            botClient.SendTextMessageAsync(config.chatId, langPack.killed.Replace("%p", e.playername).Replace("%s", e.srctype.Split('.')[1]));
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(config.chatId, langPack.killed.Replace("%p", e.playername).Replace("%s", e.srcname));
                        }
                    }
                    return true;
                });
                api.addBeforeActListener("onServerCmdOutput", es =>
                {
                    if (runcmded)
                    {
                        botClient.SendTextMessageAsync(config.chatId, langPack.feedback.Replace("%f", ServerCmdOutputEvent.getFrom(es).output));
                        runcmded = false;
                        return false;
                    }
                    return true;
                });
                Logger.Trace("已装载");
            }
            catch (Exception ex)
            {
                Logger.Trace(ex.Message, Logger.LogLevel.ERROR);
            }
        }
    }
}
