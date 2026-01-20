#region

using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DreamyManagement;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

#endregion

namespace AGC_Management.Attributes;

public class RequireStaffRole : CheckBaseAttribute
{
    private readonly ulong RoleId = ulong.Parse(BotConfig.GetConfig()["ServerConfig"]["StaffRoleId"]);

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (CurrentApplication.GlobalProperties.DebugMode) return true;

        // Check if user has staff role
        if (ctx.Member.Roles.Any(r => r.Id == RoleId))
            return true;
        return false;
    }
}

public class RequireBotOwner : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.User.Id == CurrentApplication.GlobalProperties.BotOwnerId)
            return true;
        await ctx.RespondAsync("⚠️ Du musst der Botbesitzer sein, um diese Aktion auszuführen!");
        return false;
    }
}

public class ApplicationCommandsRequireBotOwner : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.User.Id == CurrentApplication.GlobalProperties.BotOwnerId)
            return true;
        return false;
    }
}



public class ACRequireStaffRole : CheckBaseAttribute
{
    private readonly ulong RoleId = ulong.Parse(BotConfig.GetConfig()["ServerConfig"]["StaffRoleId"]);

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (CurrentApplication.GlobalProperties.DebugMode) return true;

        // Check if user has staff role
        if (ctx.Member.Roles.Any(r => r.Id == RoleId))
            return true;
        return false;
    }
}

