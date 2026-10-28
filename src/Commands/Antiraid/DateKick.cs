using AGC_Management;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DreamyManagement.Helpers;

namespace DreamyManagement.Commands.Antiraid;

[EventHandler]
public class DateKickEventhandler : BaseCommandModule
{
    [Event]
    private Task GuildMemberAdded(DiscordClient client, GuildMemberAddEventArgs eventArgs)
    {
        _ = Task.Run(async () =>
        {
            if (eventArgs.Guild?.Id != ulong.Parse(BotConfig.GetConfig()["ServerConfig"]["ServerId"])) return;

            var member = eventArgs.Member;
            if (member.IsBot) return;

            if (!bool.TryParse(BotConfig.GetConfig()["AntiRaid"]["DateKickActive"], out var isDateKickActive) ||
                !isDateKickActive) return;

            if (!int.TryParse(BotConfig.GetConfig()["AntiRaid"]["DateKickDays"], out var minAge))
                minAge = 14;

            var age = (DateTime.Now - member.CreationTimestamp.Date).Days;
            if (age < minAge)
            {
                await DateKickUtils.NotifyUser(member);
                await member.RemoveAsync($"DateKick - Konto jünger als {minAge}");
                await DateKickUtils.SendToSecurityLogChannel(member.Guild, member, minAge);
            }
        });
        return Task.CompletedTask;
    }
}

public class DateKickCommands : BaseCommandModule
{
    [Command("datekick")]
    [RequireTeamCat]
    [RequirePermissions(Permissions.Administrator)]
    public async Task DateKickCommand(CommandContext ctx, int minAge)
    {
        BotConfig.SetConfig("AntiRaid", "DateKickDays", minAge.ToString());
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("DateKick-System");
        embed.WithDescription($"Das DateKick-System wurde auf ``{minAge} Tage`` eingestellt!");
        embed.WithColor(DiscordColor.Green);
        await ctx.RespondAsync(embed);
    }

    [Command("datekick")]
    [RequireTeamCat]
    [RequirePermissions(Permissions.Administrator)]
    public async Task DateKickCommand(CommandContext ctx, bool active)
    {
        BotConfig.SetConfig("AntiRaid", "DateKickActive", active.ToString());
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("DateKick-System");
        embed.WithDescription($"Das DateKick-System wurde auf ``{active}`` eingestellt!");
        embed.WithColor(DiscordColor.Green);
        await ctx.RespondAsync(embed);
    }
}

internal class DateKickUtils
{
    public static async Task SendToSecurityLogChannel(DiscordGuild guild, DiscordMember member, int MinDays)
    {
        if (!ulong.TryParse(BotConfig.GetConfig()["AntiRaid"]["DateKickLog"], out var channelId)) return;
        var channel = guild.GetChannel(channelId);
        if (channel == null) return;

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("User durch Datekick gekickt!");
        embed.WithDescription(
            $"``{member.GetFormattedUserName()}`` ({member.Id}) wurde durch das DateKick-System entfernt" +
            $", da der Account jünger als ``{MinDays} Tage alt`` ist. \n" +
            $"Account erstellt: {member.CreationTimestamp.Timestamp()}");
        embed.WithFooter($"DateKick-System | {guild.Name}");
        embed.WithTimestamp(DateTime.Now);
        embed.WithColor(DiscordColor.Red);
        await channel.SendMessageAsync(embed);
    }

    public static async Task NotifyUser(DiscordMember member)
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("Du wurdest durch das DateKick-System entfernt!");
        embed.WithDescription(
            "Dein Account ist zu neu und wurde daher nicht für den Server zugelassen!" +
            $" Dein Account wurde erstellt am {member.CreationTimestamp.Timestamp()}");
        embed.WithTimestamp(DateTime.Now);
        embed.WithColor(DiscordColor.Red);
        try
        {
            await member.SendMessageAsync(embed);
        }
        catch
        {
            // ignored
        }
    }
}
