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
    [Plugin(Author = "Sepi", Description = "명령어를 관리합니다. Manages commands.", PluginName = "MiCommand", PluginVersion = "v1.0 - Beta")]
    public class MiCommand : Plugin
    {
        private static ILog Log = LogManager.GetLogger(typeof(MiCommand));
        private const string prefix = "[MiCommand] ";

        protected override void OnEnable()
        {
            Log.Info("MiCommand가 로드되었습니다.");
        }

        [PacketHandler]
        public Package HandleChatCommand(McpeText packet, Player player)
        {
            var msg = packet.message;
            if (msg.First() != '!')
            {
                return packet;
            }
            Log.Debug($"명령어 로드 시작. 받은 명령어: {msg}");
            var commandList = this.Context.PluginManager.Commands;
            var commandMsg = msg.Remove(0, 1);
            var command = commandMsg.Split(' ');
            var commandArgs = new List<string>();
            if (command.Count() > 1)
            {
                commandArgs = command.ToList().GetRange(1, command.Count() - 1);
            }
            Log.Debug($"명령어: {command[0]}, 인자갯수: {commandArgs.Count}");
            Log.Debug("명령어 검색 시작");
            var commandResults = (from commandInfo in commandList
                                  where commandInfo.Key == command[0]
                                  select commandInfo).ToList();
            if (commandResults.Count() == 0)
            {
                Log.Debug("명령어 검색 실패. 존재안함.");
                player.SendMessage($"{ChatColors.Yellow}명령어가 존재하지 않아요!");
                return null;
            }
            Log.Debug($"명령어 검색 성공");
            dynamic commandInputJson = null;
            if (commandArgs.Count > 0)
            {
                var targetArgsType = new List<string>();
                commandArgs.ForEach(x => targetArgsType.Add(GetArgType(x)));
                foreach (var item in targetArgsType)
                {
                    Log.Debug($"명령어 인자 타입:{item}");
                }
                string overloadKey = null;
                List<string> parameterNames = new List<string>();
                foreach (var targetCommand in commandResults)
                {
                    foreach (var overload in targetCommand.Value.Versions.First().Overloads)
                    {
                        var methodParmsType = new List<string>();
                        foreach (var parameter in overload.Value.Method.GetParameters())
                        {
                            if (parameter.ParameterType == typeof(Player))
                            {
                                continue;
                            }
                            methodParmsType.Add(GetParameterType(parameter));
                        }

                        targetArgsType.Sort();
                        methodParmsType.Sort();
                        if (targetArgsType.SequenceEqual(methodParmsType))
                        {
                            overloadKey = overload.Key;
                            foreach (var parameter in overload.Value.Method.GetParameters())
                            {
                                parameterNames.Add(parameter.Name);
                            }
                            break;
                        }
                    }
                    if (overloadKey != null)
                    {
                        break;
                    }
                }

                if (overloadKey == null)
                {
                    Log.Debug("명령어 검색 실패. 명령어와 맞는 인자가 있는 함수가 존재하지 않음.");
                    player.SendMessage($"{ChatColors.Yellow}명령어가 존재하지 않아요!");
                    return null;
                }

                Log.Debug("인자를 json으로 변경중...");
                string json = ConvertJson(parameterNames, commandArgs);
                Log.Debug($"변환 결과: {json}");
                if (json != null)
                {
                    commandInputJson = JsonConvert.DeserializeObject<dynamic>(json);
                }
            }

            this.Context.Server.PluginManager.HandleCommand(player, command[0], "default", commandInputJson ?? null);
            Log.Debug("명령어 실행함");

            return null;
        }

        private string ConvertJson(List<string> key, List<string> value)
        {
            string json = "{ ";
            if (key.Count == 0)
            {
                return null;
            }
            for (int i = 0; i < value.Count; i++)
            {
                if (key.Count - 1 >= i)
                {
                    json += $@"""{ key[i].ToString() }"": ""{ value[i].ToString() }"", ";
                }                
            }
            json = json.Remove(json.Count() - 2);
            json += " }";

            return json;
        }

        private static string GetArgType(string arg)
        {
            string value = string.Empty;
            if (int.TryParse(arg, out int i))
            {
                value = "int";
            }
            else if (bool.TryParse(arg, out bool b))
            {
                value = "bool";
            }
            else
            {
                value = "string";
            }
            return value;
        }

        // Code from https://github.com/NiclasOlofsson/MiNET/blob/master/src/MiNET/MiNET/Plugins/PluginManager.cs#L345
        private static string GetParameterType(ParameterInfo parameter)
        {
            string value = parameter.ParameterType.ToString();
            if (parameter.ParameterType == typeof(int))
                value = "int";
            else if (parameter.ParameterType == typeof(short))
                value = "int";
            else if (parameter.ParameterType == typeof(byte))
                value = "int";
            else if (parameter.ParameterType == typeof(bool))
                value = "bool";
            else if (parameter.ParameterType == typeof(string))
                value = "string";
            else if (parameter.ParameterType == typeof(string[]))
                value = "rawtext";
            else if (parameter.ParameterType == typeof(Target))
                value = "target";
            else if (parameter.ParameterType == typeof(BlockPos))
                value = "blockpos";
            else if (parameter.ParameterType.BaseType == typeof(EnumBase))
            {
                value = "stringenum";
            }
            else if (typeof(IParameterSerializer).IsAssignableFrom(parameter.ParameterType))
            {
                // Custom serialization
                value = "string";
            }
            else
            {
                Log.Warn("No parameter type mapping for type: " + parameter.ParameterType.ToString());
            }
            return value;
		}

        [Command]
        public void Test(Player player)
        {
            player.SendMessage("성공");
        }

        [Command]
        public void cmd(Player player)
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
