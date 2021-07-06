using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    public class Handler
    {
        public static async Task Client_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
        {
            Console.WriteLine(e.Interaction.Data.Id);

            switch (e.Interaction.Data.Id)
            {
                case 821427821816709152:
                    {
                        e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder
                            {
                                Content = $"Hello {e.Interaction.User.Username}!"
                            });

                    }
                    break;

                case 821525777156800532:
                    {
                        var FollowCommand = e.Interaction.Data;

                        foreach (var Option in FollowCommand.Options)
                        {
                            Console.WriteLine($@"Type = {Option.Type} {Option.Name}: {Option.Value}");
                        }

                        //foreach (var Resolved in  FollowCommand.Resolved)
                        //{

                        //}
                    }
                    break;

            }
        }
    }











    public class Rootobject
    {
        public int type { get; set; }
        public Data data { get; set; }
        public string token { get; set; }
        public int version { get; set; }
        public long application_id { get; set; }
        public long id { get; set; }
    }

    public class Data
    {
        public string name { get; set; }
        public Option[] options { get; set; }
        public Resolved resolved { get; set; }
        public long id { get; set; }
    }

    public class Resolved
    {
        public Channel[] channels { get; set; }
        public Role[] roles { get; set; }
    }

    public class Channel
    {
        public object[] permission_overwrites { get; set; }
        public long guild_id { get; set; }
        public object parent_id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public int position { get; set; }
        public string topic { get; set; }
        public int last_message_id { get; set; }
        public int bitrate { get; set; }
        public int user_limit { get; set; }
        public object rate_limit_per_user { get; set; }
        public bool nsfw { get; set; }
        public long id { get; set; }
    }

    public class Role
    {
        public int color { get; set; }
        public string name { get; set; }
        public bool hoist { get; set; }
        public int position { get; set; }
        public long permissions { get; set; }
        public bool managed { get; set; }
        public bool mentionable { get; set; }
        public string Mention { get; set; }
        public long id { get; set; }
    }

    public class Option
    {
        public string name { get; set; }
        public int type { get; set; }
        public object value { get; set; }
    }

}
