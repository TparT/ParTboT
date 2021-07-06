using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.DbModels.DSharpPlus
{
    public class ExtendedBotIntegration
    {
        //
        // Summary:
        //     Gets the integration name.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the integration type.
        public string Type { get; set; }
        //
        // Summary:
        //     Gets whether this integration is enabled.
        public bool IsEnabled { get; set; }
        //
        // Summary:
        //     Gets whether this integration is syncing.
        public bool IsSyncing { get; set; }
        //
        // Summary:
        //     Gets ID of the role this integration uses for subscribers.
        public ulong RoleId { get; set; }
        //
        // Summary:
        //     Gets the expiration behaviour.
        public int ExpireBehavior { get; set; }
        //
        // Summary:
        //     Gets the grace period before expiring subscribers.
        public int ExpireGracePeriod { get; set; }
        //
        // Summary:
        //     Gets the user that owns this integration.
        public DiscordUser IntegrationOwnerUser { get; set; }
        //
        // Summary:
        //     Gets the 3rd party service account for this integration.
        public DiscordIntegrationAccount Account { get; set; }
        //
        // Summary:
        //     Gets the date and time this integration was last synced.
        public DateTimeOffset SyncedAt { get; set; }

        public ExtendedMemberModel BotAsMemberInGuild { get; set; }
    }
}
