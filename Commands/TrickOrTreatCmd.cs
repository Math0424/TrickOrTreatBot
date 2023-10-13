using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using TrickOrTreatBot.Objects;
using DiscordBot.Services;
using Discord;

namespace DiscordBot.Commands
{
    public class TrickOrTreatCmd : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TrickOrTreatModule _mod;
        public TrickOrTreatCmd(TrickOrTreatModule mod)
        {
            _mod = mod;
        }

        [SlashCommand("trick", "Trick them!")]
        public async Task Trick()
        {
            await ClaimPrize(true);
        }

        [SlashCommand("treat", "Treat them!")]
        public async Task Treat()
        {
            await ClaimPrize(false);
        }

        private async Task ClaimPrize(bool trick)
        {
            User u = Storage.GetUser(Context.User.Id);
            if (u == null)
            {
                await RespondAsync("You dont have a costume, use /costume", ephemeral: true);
                return;
            }

            await DeferAsync(true);
            ClaimStatus s = _mod.ClaimDrop(Context.Channel.Id, u, trick);
            switch (s)
            {
                case ClaimStatus.Claimed:
                    await FollowupAsync("Claimed prize!", ephemeral: true);
                    break;
                case ClaimStatus.Incorrect:
                    await FollowupAsync("Wrong answer!", ephemeral: true);
                    break;
                case ClaimStatus.AlreadyClaimed:
                    await FollowupAsync("Already claimed!", ephemeral: true);
                    break;
                case ClaimStatus.NothingToClaim:
                    await FollowupAsync("Nothing to claim!", ephemeral: true);
                    break;
            }
        }

    }
}
