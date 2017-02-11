using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using log4net;
using System.Reflection;

namespace MiCommand
{
    [Plugin(Author = "Sepi", Description = "명령어를 관리합니다. Manages commands.", 
        PluginName = "MiCommand", PluginVersion = "v1.0 - Beta")]
    public class MiCommand : Plugin
    {
        protected static ILog Log = LogManager.GetLogger(typeof(MiCommand));

        protected override void OnEnable()
        {
            Log.Info("MiCommand가 로드되었습니다.");
        }

        public override void OnDisable()
        {
            Log.Info("MiCommand가 언로드되었습니다.");
        }

        [Command]
        public void Test(Player player)
        {
            player.SendMessage("성공");
        }

        [Command]
        public void Cmd(Player player)
        {
            player.SendMessage("cmd 명령어임.");
        }

        [Command]
        public void Test(Player player, string args1, string args2)
        {
            player.SendMessage("성공");
            player.SendMessage($"인자: {args1}, {args2}");
        }
    }
}
