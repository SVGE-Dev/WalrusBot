using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusBot.Modules
{
    [DontAutoLoad]
    [Group("meeting")]
    [RequireUserPermission(GuildPermission.Administrator)]
    class Meeting : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultVoteAsync()
            => await HelpVotesAsync();

        private static Dictionary<IGuildChannel, Meeting> meetings = new Dictionary<IGuildChannel, Meeting>();

        #region Help
        [Command("help")]
        public async Task HelpVotesAsync()
        {

        }
        #endregion

        #region Commands
        [Command("create")]
        public async Task CreateMeetingAsync(IGuildChannel voiceChannel, IGuildUser chair)
        {

        }

        [Command("openvote")]
        public async Task OpenVoteAsync(string name, params string[] options)
        {
            // get the voice channel of the caller
            // find the meeting associated with that voice channel
            // check that the caller is the chair
        }

        [Command("closevote")]
        [RequireContext(ContextType.DM)]  // this sends the results to the chair directly so that they can anonymously handle ties
        public async Task CloseVoteAsync(string name)
        {

        }

        [Command("votefor")]
        [RequireContext(ContextType.DM)]
        public async Task VoteForAsync(string voteName, string choice)
        {
            // check the user is a student
            // get the current voice channel of the user
            // check there's an associated meeting
            // find the vote with "voteName" in that meeting
        }
        #endregion

        #region Subclasses
        private class MeetingData  // might work as a struct, we'll see
        {
            Dictionary<string, Vote> votes = new Dictionary<string, Vote>();
        }
        private class Vote
        {
            public Vote()
            {

            }
            public bool VoteFor(string name)
            {
                if (!choices.ContainsKey(name)) return false;  // let us know that that isn't an option
                choices[name]++;
                return true;
            }
            public string name;  // am being lazy, deal with it B|
            Dictionary<string, ushort> choices = new Dictionary<string, ushort>();  // choice-name/vote-number pairs. Limited to 255 votes
        }
        #endregion
    }
}
