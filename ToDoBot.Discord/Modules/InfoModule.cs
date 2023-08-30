using Discord.Commands;

namespace ToDoBot.Discord.Modules;

public class InfoModule : ModuleBase<SocketCommandContext>
{
    [Command("say")]
    [Summary("Echoes a message.")]
    public Task SayAsync([Remainder] [Summary("The text to echo")] string echo) => ReplyAsync(echo);
}