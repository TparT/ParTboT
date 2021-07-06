using System;
using DSharpPlus;

namespace ParTboT.DbModels.DSharpPlus
{
    public class ExtendedRoleModel
    {
        public ulong Id { get; set; }
        //public ulong RoleId { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
        public int Position { get; set; }
        public Permissions Permissions { get; set; }
        public bool IsManaged { get; set; }
        public bool IsMentionable { get; set; }
        public DateTime CreationTimeStamp { get; set; }
    }
}
