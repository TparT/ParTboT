using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.DbModels.DSharpPlus
{
    public class ExtendedMemberModel
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Nickname { get; set; }
        public Dictionary<string, ExtendedRoleModel> Roles { get; set; }
        public string MentionString { get; set; }
        public int Color { get; set; }
        
        public DateTimeOffset JoinedAt { get; set; }
        
        public DateTimeOffset? PremiumSince { get; set; }
        
        //public bool IsDeafened { get; set; }
        
        public bool IsMuted { get; set; }
        
        public bool? IsPending { get; set; }
        
        //public DiscordVoiceState VoiceState { get; set; }
        
        //public DiscordGuild Guild { get; set; }
        
        public bool IsOwner { get; set; }
        
        public int Hierarchy { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string AvatarHash { get; set; }
        public bool IsBot { get; set; }
        public string Email { get; set; }
        public bool? MfaEnabled { get; set; }
        public bool? Verified { get; set; }
        public string Locale { get; set; }
        
        public string DisplayName { get; set; }
    }
}
