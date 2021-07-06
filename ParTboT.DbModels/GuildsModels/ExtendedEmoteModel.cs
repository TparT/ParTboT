using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace ParTboT.DbModels.DSharpPlus
{
    public class ExtendedEmoteModel
    {
        //
        // Summary:
        //     Gets whether the emoji is available for use. An emoji may not be available due
        //     to loss of server boost.
        public bool IsAvailable { get; set; }
        //
        // Summary:
        //     Gets whether this emoji is animated.
        public bool IsAnimated { get; set; }
        //
        // Summary:
        //     Gets whether this emoji is managed by an integration.
        public bool IsManaged { get; set; }
        //
        // Summary:
        //     Gets whether this emoji requires colons to use.
        public bool RequiresColons { get; set; }
        //
        // Summary:
        //     Gets IDs the roles this emoji is enabled for.
        public List<ulong> Roles { get; set; }
        //
        // Summary:
        //     Gets the name of this emoji.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the image URL of this emoji.
        public string Url { get; set; }

        //
        // Summary:
        //     Gets the user that created this emoji.
        public ExtendedMemberModel User { get; set; }

        //
        // Summary:
        //     Gets the ID of this object.
        public ulong Id { get; set; }

        //
        // Summary:
        //     Gets the date and time this emote was created.
        public DateTimeOffset CreationTimestamp { get; set; }
    }
}
