using DSharpPlus;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ParTboT.Web.Data
{
    public class LoginService
    {
        public ConcurrentDictionary<ulong, DiscordRestClient> ActiveLogins { get; set; } = new ConcurrentDictionary<ulong, DiscordRestClient>();

        public DiscordRestClient Login(ulong userId, string token)
        {
            return ActiveLogins.GetOrAdd(userId, new DiscordRestClient(new DiscordConfiguration
            {
                TokenType = TokenType.Bearer,
                Token = token
            }));
        }

        public bool Logout(ulong userId)
        {
            return ActiveLogins.TryRemove(userId, out DiscordRestClient user);
        }
    }
}
