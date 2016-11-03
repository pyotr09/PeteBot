using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunchBot
{
	[Serializable]
    [LuisModel("c7093cf8-f4aa-4cd8-b4f7-4c14af84c494", "008c175af7e844d28aa97cbcf6556914")]
    public class NominationsDialog : LuisDialog<object>
    {
        public string User { get; set; }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            User = item.GetAwaiter().GetResult().From.Name;
            if (item.GetAwaiter().GetResult().GetActivityType() == ActivityTypes.Ping)
            {
                return VotingDialog.MesageReceived(context, item);
            }
            return base.MessageReceived(context, item);
        }

        [LuisIntent(nameof(None))]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string userUtterance = result.Query;
            await context.PostAsync($"Sorry, I didn't understand \"{userUtterance}\".");
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Nominate))]
        [LuisIntent("Second")]
        public async Task Nominate(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation = result.Entities.FirstOrDefault();
            string location = entityRecommendation?.Entity;
            string message;
            if (string.IsNullOrEmpty(location))
            {
                message = $"LUIS didn't get an entity out of {result.Query}.";
            }
            else
            {
                DataStore.Instance.AddRequest(location, User);
                message = $"{location} has been {DataStore.Instance.Status(location)}.";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Veto))]
        public async Task Veto(IDialogContext context, LuisResult result)
        {
            bool task = await Task.Run(()=> { Thread.Sleep(new TimeSpan(0, 5, 0)); return true;});
            
            EntityRecommendation entityRecommendation = result.Entities.FirstOrDefault();
            string location = entityRecommendation?.Entity;
            string message;
            if (string.IsNullOrEmpty(location))
            {
                message = $"LUIS didn't get an entity out of {result.Query}.";
            }
            else
            {
                if (DataStore.Instance.CanVeto(User))
                {
                    DataStore.Instance.Veto(location, User);
                    message = $"{location} has been \"{DataStore.Instance.Status(location)}\".";
                }
                else
                {
                    message = $"{User} can't veto anymore";
                }
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(CallToVote))]
        public async Task CallToVote(IDialogContext context, LuisResult result)
        {
            if (DataStore.Instance.GetAdmins().Contains(User))
            {
                await context.PostAsync($"{User} has called for a vote!");
                DateTime now = DateTime.Now;
                await context.PostAsync($"Timer begins now {now.ToShortTimeString()}, and ends at {now.AddMinutes(5)}");
                await context.PostAsync(
                    "Here is how it works: I'll list the choices and you send me a message (private or public) with your preferences in descending order.");
                List<string> seconds = DataStore.Instance.GetSeconds();
                for (int i = 0; i < seconds.Count; i++)
                {
                    await context.PostAsync($"{i+1}: {seconds.Skip(i).Take(1).First()}");
                }
                Ballot.Instance = new Ballot(DataStore.Instance.GetSeconds().ToArray());
                VotingDialog.VoteStarts = now;
                VotingDialog.VoteDuration = new TimeSpan(0,10,0);
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(List))]

        public async Task List(IDialogContext context, LuisResult result)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Nominations: ");
            stringBuilder.AppendLine(string.Join(", ", DataStore.Instance.GetNominations()));
            stringBuilder.Append("Seconded: ");
            stringBuilder.AppendLine(string.Join(", ", DataStore.Instance.GetSeconds()));
            await context.PostAsync(stringBuilder.ToString());
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Remove))]

        public async Task Remove(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation = result.Entities.FirstOrDefault();
            string location = entityRecommendation?.Entity;
            string message;
            if (string.IsNullOrEmpty(location))
            {
                message = $"LUIS didn't get an entity out of {result.Query}.";
            }
            else
            {
                message = DataStore.Instance.Remove(location, User) 
                    ? $"{location} is now \"{DataStore.Instance.Status(location)}\"." 
                    : $"Sorry {User}, but you are not an admin user, only one of {DataStore.Instance.GetAdmins()} can do that.";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Vote))]
        public async Task Vote(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation = result.Entities.FirstOrDefault();
            string ballot = entityRecommendation?.Entity;
            string message;
            if (string.IsNullOrEmpty(ballot))
            {
                message = $"LUIS didn't get an entity out of {result.Query}.";
            }
            else
            {
                if (Ballot.Instance == null)
                {
                    Ballot.Instance = new Ballot(DataStore.Instance.GetSeconds().ToArray());
                }
                Ballot.Instance.Cast(User, ballot);
                message = $"{User}, I've received your vote. Feel free to recast your vote, only yoru last vote will be counted.";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}