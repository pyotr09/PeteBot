using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunchBot
{
    [Serializable]
    [LuisModel("c7093cf8-f4aa-4cd8-b4f7-4c14af84c494", "008c175af7e844d28aa97cbcf6556914")]
    public class LunchDialog : LuisDialog<object>
    {
        public string User { get; set; }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            User = item.GetAwaiter().GetResult().From.Name;
            return base.MessageReceived(context, item);
        }

        public static string[] AdminUsers { get; set; }

        [LuisIntent(nameof(None))]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string userUtterance = result.Query;
            await context.PostAsync($"Sorry, I didn't understand \"{userUtterance}\".");
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Nominate))]
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
                DataStore.Instance.Nominate(location, User);
                message = $"{location} has been \"{DataStore.Instance.Status(location)}\".";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Second))]
        public async Task Second(IDialogContext context, LuisResult result)
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
                DataStore.Instance.Second(location, User);
                message = $"{location} has been \"{DataStore.Instance.Status(location)}\".";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent(nameof(Veto))]
        public async Task Veto(IDialogContext context, LuisResult result)
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
            if (AdminUsers?.Contains(User) ?? false)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{User} has called for a vote!");
                DateTime now = DateTime.Now;
                stringBuilder.AppendLine($"Timer begins now {now.ToShortTimeString()}, and ends at {now.AddMinutes(5)}");
                stringBuilder.AppendLine(
                    "Here is how it works: I'll list the choices and you send me a private message with your prefferences in descending order.");
                List<string> seconds = DataStore.Instance.GetSeconds();
                for (int i = 0; i < seconds.Count; i++)
                {
                    stringBuilder.AppendLine($"{i}. {seconds[i]}");
                }
                await context.PostAsync(stringBuilder.ToString());
                context.Wait(MessageReceived);
            }
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
            if (AdminUsers.Contains(User))
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
                    DataStore.Instance.Remove(location);
                    message = $"{location} is now \"{DataStore.Instance.Status(location)}\".";
                }
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }
    }
}