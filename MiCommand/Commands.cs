﻿using System;
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
    public class Commands : MiCommand
    {
        private List<List<string>> GetHelpList()
        {
            var commands = this.Context.Server.PluginManager.Commands;
            var helpList = new List<string>();
            foreach (var command in commands)
            {
                string helpMsg = $"!{command.Value.Name}";
                if (!string.IsNullOrWhiteSpace(command.Value.Versions.First().Overloads["default"].Description))
                {
                    helpMsg += $" -> {command.Value.Versions.First().Overloads["default"].Description}";
                }
                helpList.Add(helpMsg);
            }

            var result = new List<List<string>>();
            var list = new List<string>();
            for (int i = 0; i < helpList.Count; i++)
            {
                list.Add(helpList[i]);
                if (list.Count == 4)
                {
                    result.Add(list);
                    list.Clear();
                    continue;
                }
                if (i == helpList.Count - 1)
                {
                    result.Add(list);
                    break;
                }
            }

            return result;
        }

        [Command(Description = "명령어 목록을 보여줍니다.")]
        public void Help(Player player)
        {
            var helpList = GetHelpList();
            if (helpList.Count == 0)
            {
                player.SendMessage($"{ChatColors.Yellow}서버에서 검색되는 명령어가 없습니다.");
            }
            else
            {
                GetHelpList().First().ForEach(x => player.SendMessage(ChatColors.Green + x));
                player.SendMessage($"{ChatColors.LightPurple}1/{helpList.Count} 페이지입니다.");
                player.SendMessage($"{ChatColors.LightPurple}더 많은 명령어를 보고 싶으시면 {ChatColors.Green}[!help (페이지 숫자)]");
            }
        }

        [Command]
        public void Test(Player player)
        {
            player.SendMessage("GGGGGGGGGG");
        }

        [Command]
        public void Test(Player player, int num)
        {
            player.SendMessage(num.ToString());
        }

        //[Command]
        //public void Help(Player player, int pageNumber)
        //{
        //    var helpList = GetHelpList();
        //    if (helpList.Count == 0)
        //    {
        //        player.SendMessage($"{ChatColors.Yellow}서버에서 검색되는 명령어가 없습니다.");
        //        return;
        //    }

        //    if (pageNumber > helpList.Count || pageNumber < 1)
        //    {
        //        player.SendMessage($"{ChatColors.Yellow}맞는 페이지 번호가 없습니다.");
        //        player.SendMessage($"명령어 목록 페이지 수는 {ChatColors.Green}{helpList.Count}{ChatColors.Yellow}페이지까지 존재합니다.");
        //    }
        //    else
        //    {
        //        helpList[pageNumber - 1].ForEach(x => player.SendMessage(ChatColors.Green + x));
        //        player.SendMessage($"{ChatColors.LightPurple}{pageNumber}/{helpList.Count} 페이지입니다.");
        //        player.SendMessage($"{ChatColors.LightPurple}더 많은 명령어를 보고 싶으시면 {ChatColors.Green}[!help (페이지 숫자)]");
        //    }
        //}
    }
}
