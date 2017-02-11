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
    public class CommandHandler : MiCommand
    {
        [PacketHandler]
        public Package HandleCommand(McpeText packet, Player player)
        {
            var msg = packet.message;
            if (msg.First() != '!')
            {
                return packet;
            }

            var commands = this.Context.PluginManager.Commands.ToList();
            msg = msg.Remove(0, 1);
            if (string.IsNullOrWhiteSpace(msg))
            {
                return packet;
            }
            var msgs = msg.Split(' ').ToList();
            var targetCommand = msgs[0];
            var targetCommandArgs = new List<string>();
            if (msgs.Count > 1)
            {
                msgs.Remove(msgs[0]);
                targetCommandArgs = msgs;
            }

            string overloadKey = null;
            JObject commandInputJson = null;

            if (commands.Exists(x => x.Key == targetCommand))
            {
                var command = commands.Find(x => x.Key == targetCommand);
                var commandArgNames = new List<string>();

                if (targetCommandArgs.Count > 0)
                {
                    var targetCommandArgTypes = new List<string>();
                    foreach (var arg in targetCommandArgs)
                    {
                        targetCommandArgTypes.Add(GetArgType(arg));
                    }
                    targetCommandArgTypes.Sort();

                    foreach (var overload in command.Value.Versions.First().Overloads)
                    {
                        var commandArgTypes = new List<string>();
                        foreach (var parameter in overload.Value.Method.GetParameters())
                        {
                            if (parameter.ParameterType == typeof(Player))
                            {
                                continue;
                            }
                            commandArgTypes.Add(GetParameterType(parameter));
                            commandArgNames.Add(parameter.Name);
                        }

                        commandArgTypes.Sort();
                        if (targetCommandArgTypes.SequenceEqual(commandArgTypes))
                        {
                            overloadKey = overload.Key;
                            commandInputJson = JObject.Parse(ConvertJson(commandArgNames, targetCommandArgs));
                        }
                        commandArgNames.Clear();
                    }
                    if (overloadKey == null)
                    {
                        player.SendMessage($"{ChatColors.Yellow}명령어가 존재하지 않아요!");
                        return null;
                    }
                }
                else
                {
                    overloadKey = "default";
                }
            }
            else
            {
                player.SendMessage($"{ChatColors.Yellow}명령어가 존재하지 않아요!");
                return null;
            }

            this.Context.PluginManager.HandleCommand(player, targetCommand, overloadKey, commandInputJson);
            return null;
        }

        private string ConvertJson(List<string> commandParamName, List<string> targetCommandArgs)
        {
            string json = "{ ";
            if (commandParamName.Count == 0)
            {
                return null;
            }
            for (int i = 0; i < targetCommandArgs.Count; i++)
            {
                if (commandParamName.Count - 1 >= i)
                {
                    json += $@"""{ commandParamName[i].ToString() }"": ""{ targetCommandArgs[i].ToString() }"", ";
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
    }
}
