#region

using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using AGC_Management.Entities;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DreamyManagement;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NpgsqlDataSource = Npgsql.NpgsqlDataSource;

#endregion

namespace AGC_Management.Utils;

public static class ToolSet
{
    public static string GetFaviconUrl()
    {
        return CurrentApplication.TargetGuild.IconUrl != null
            ? CurrentApplication.TargetGuild.IconUrl
            : "favicon.png";
    }

    public static string GettextfromBase64(string base64)
    {
        var data = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(data);
    }

    public static string GetFormattedTimeFromUnix(long unixTime)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        return dateTimeOffset.ToString("dd.MM.yyyy HH:mm:ss");
    }


    public static string GetFormattedTimeFromUnixAndRespectTimeZone(long unixTime)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        return TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.DateTime, TimeZoneInfo.Local)
            .ToString("dd.MM.yyyy HH:mm:ss");
    }


    public static async Task<bool> IsUserInCache(ulong userId)
    {
        try
        {
            var cachedmembers = CurrentApplication.DiscordClient.UserCache.Values.ToList();

            return cachedmembers.Any(member => member.Id == userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Abrufen der Servermitglieder: " + ex.Message);
            return false;
        }
    }


    public static string GetBuildNumber(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0) return value.Substring(index + BuildVersionMetadataPrefix.Length);
        }

        return string.Empty;
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }

    public static long GetBuildDateToUnixTime(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.CurrentCulture,
                        DateTimeStyles.AssumeLocal, out var result))
                    return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(result, TimeZoneInfo.Local))
                        .ToUnixTimeSeconds();
            }
        }

        return default;
    }

    public static async Task<bool> IsUserOnServer(ulong userId)
    {
        try
        {
            var serverMembers = CurrentApplication.TargetGuild.Members.Values.ToList();

            return serverMembers.Any(member => member.Id == userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Abrufen der Servermitglieder: " + ex.Message);
            return false;
        }
    }

    public static PartialUser GetFallbackUser(ulong userId)
    {
        return new PartialUser
        {
            UserId = userId,
            UserName = userId.ToString(),
            Avatar = GetDefaultAvatarUrlForUserId(userId)
        };
    }

    public static string GetDefaultAvatarUrlForUserId(ulong userId)
    {
        var domainUrl = DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url;
        var avatarIndex = (userId >> 22) % 6;
        var avatarUrl = $"{domainUrl}{Endpoints.EMBED}{Endpoints.AVATARS}/{avatarIndex}.png?size=1024";

        return avatarUrl;
    }

    public static string GetFormattedName(ulong userid)
    {
        var member = CurrentApplication.TargetGuild.Members.Values.FirstOrDefault(x => x.Id == userid);
        if (member != null)
        {
            if (!string.IsNullOrEmpty(member.Nickname)) return $"{member.Username} ({member.Nickname})";

            if (!string.IsNullOrEmpty(member.DisplayName)) return $"{member.Username} ({member.DisplayName})";

            return member.Username;
        }

        return userid.ToString();
    }

    public static string GetFormattedName(DiscordMember member)
    {
        if (!string.IsNullOrEmpty(member.Nickname)) return $"{member.Username} ({member.Nickname})";

        if (!string.IsNullOrEmpty(member.DisplayName)) return $"{member.Username} ({member.DisplayName})";

        return member.Username;
    }

    public static string GenerateCaseID()
    {
        var guid = Guid.NewGuid().ToString("N");
        var uniqueID = guid.Substring(0, 8);
        return uniqueID;
    }



    public static async Task<bool> TicketUrlCheck(CommandContext ctx, string reason)
    {
        var TicketUrl = "ticketsystem.animegamingcafe.de";
        if (reason == null) return false;
        if (reason.ToLower().Contains(TicketUrl.ToLower()))
        {
            Console.WriteLine("Ticket-URL enthalten");
            var embedBuilder = new DiscordEmbedBuilder().WithTitle("Fehler: Ticket-URL enthalten")
                .WithDescription("Bitte schreibe den Grund ohne Ticket-URL").WithColor(DiscordColor.Red);
            var embed = embedBuilder.Build();
            var msg_e = new DiscordMessageBuilder().AddEmbed(embed).WithReply(ctx.Message.Id);
            await ctx.Channel.SendMessageAsync(msg_e);

            return true;
        }

        return false;
    }
}