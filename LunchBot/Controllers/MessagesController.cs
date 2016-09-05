using LunchBot.Model;
using LunchBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchBot.Controllers
{
	[BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new LunchDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        private static Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

    [Serializable]
    public class LunchDialog : IDialog<object>
    {
        protected const string ModPassword = "YouShallNotPa$$";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument;
            if (activity.Text.ToLower() == "resetlists")
            {
                if (DataStorage.UserList.Exists(x => x.Id == activity.From.Id))
                {
                    var sendingUser = DataStorage.UserList.FirstOrDefault(x => x.Id == activity.From.Id);
                    if (sendingUser == null) return;
                    if (true) //TODO check mod 
                    {
                        PromptDialog.Confirm(
                            context,
                            AfterResetAsync,
                            "Are you sure you want to reset the lists? (yes or no)",
                            $"Didn't get that {sendingUser.Name}! Are you sure you want to reset the lists? (yes or no)",
                            promptStyle: PromptStyle.None);
                    }
                }
            }
            //else if (activity.Text.ToLower() == "mod me")
            //{
            //    await RequestUserModAsync(activity, context);
            //    context.Wait(MessageReceivedAsync);
            //}
            else
            {
                await context.PostAsync(ExecuteCommand(activity));
                context.Wait(MessageReceivedAsync);
            }
            
        }
        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                DataStorage.UserList = new List<User>();
                DataStorage.RestaurantList = new List<Restaurant>();
                DataStorage.InLineRestaurantList = new List<Restaurant>();
                DataStorage.WaitingList = new List<Restaurant>();
                await context.PostAsync("Lists reset.");
            }
            else
            {
                await context.PostAsync("Did not reset lists.");
            }
            context.Wait(MessageReceivedAsync);
        }

        private string ExecuteCommand(IMessageActivity act)
        {
            var message = act.Text.ToLower();
            var sendingUser = new User
            {
                Id = act.From.Id,
                Name = act.From.Name
            };
            if (!DataStorage.UserList.Exists(x => x.Id == sendingUser.Id))
            {
                //send request to moderator for approval
                DataStorage.UserList.Add(sendingUser);
            }
            else
            {
                sendingUser = DataStorage.UserList.FirstOrDefault(x => x.Id == sendingUser.Id);
            }
            if (message.StartsWith("nominate "))
            {
                return Nominate(message, sendingUser);
            }
            if (message.StartsWith("second "))
            {
                return Second(message, sendingUser);
            }
            if (message.StartsWith("veto "))
            {
                return Veto(message, sendingUser);
            }
            if (message.StartsWith("show list"))
            {
                return ShowList(message, sendingUser);
            }
            if (message.StartsWith("mod me"))
            {
                
            }
            return "";
        }

        private string Nominate(string pMessage, User pUser)
        {
            var message = pMessage.Substring(9);
            var rest = new Restaurant
            {
                Name = message,
                UserThatNomiated = pUser
            };
            if (true)//TODO mod approval needed here
            {

                if (DataStorage.RestaurantList.Exists(x => x.Name == rest.Name))
                {
                    return "Already in list.";
                }

                rest.LinePos = DataStorage.RestaurantList.Count;
                DataStorage.RestaurantList.Add(rest);
                return rest.Name + " added to the list by and needs a second! Nominated by " + pUser.Name;
            }
            else
            {
                return "Request DENIED.";
            }
        }

        private static string Second(string pMessage, User pUser)
        {
            var message = pMessage.Substring(7);
            if (!DataStorage.RestaurantList.Exists(x => x.Name == message))
                return "Can't second something that hasn't been nominated.";
            {
                if (DataStorage.InLineRestaurantList.Exists(x => x.Name == message))
                    return "You cannot second something twice!";
                {
                    var restaurant = DataStorage.RestaurantList.FirstOrDefault(x => x.Name == message);
                    if (restaurant == null) return "";
                    if (restaurant.UserThatNomiated.Id == pUser.Id)
                        return "You can't second your own nomination silly!";
                    restaurant.UserThatSeconded = pUser;
                    restaurant.IsSeconded = true;
                    if (DataStorage.InLineRestaurantList.Count < 5)
                    {
                        DataStorage.InLineRestaurantList.Add(restaurant);
                        return restaurant.Name + 
                               " has been seconded and is in " + 
                               DataStorage.InLineRestaurantList.Count + 
                               " position in voting order.";
                    }
                    DataStorage.WaitingList.Add(restaurant);
                    return restaurant.Name + 
                           " has been seconded and is waiting at " + 
                           DataStorage.WaitingList.Count + 
                           " position in waiting order.";
                }
            }
        }

        private static string Veto(string pMessage, User pUser)
        {
            var message = pMessage.Substring(5);
            {
                if (!DataStorage.RestaurantList.Exists(x => x.Name == message))
                    return "You cannot veto things that weren't nominated.";
                {
                    var restaurant = DataStorage.RestaurantList.FirstOrDefault(x => x.Name == message);
                    if (restaurant == null) return "";
                    if (pUser.NumVetos <= 0) return pUser.Name + " you have no more vetos to use.";
                    {
                        if (restaurant.VetoList.Exists(x => x.Id == pUser.Id))
                            return "You cannot veto something more than once!";
                        {
                            restaurant = DataStorage.RestaurantList.FirstOrDefault(x => x.Name == message);
                            if (restaurant == null) return "";
                            restaurant.VetoList.Add(pUser);
                            restaurant.Vetos++;
                            pUser.NumVetos--;
                            //send veto confirmed message
                            if (restaurant.Vetos < 3)
                                return restaurant.Name + " was vetoed by " + pUser.Name + " and needs " +
                                       (3 - restaurant.Vetos) + " more veto(s) to be knocked out of voting.";
                            {
                                restaurant.IsVetoed = true;
                                if (!DataStorage.InLineRestaurantList.Exists(x => x.Name == restaurant.Name))
                                    return restaurant.Name + " was vetoed by " + pUser.Name +
                                           " and is already on waiting list";
                                {
                                    DataStorage.InLineRestaurantList.Remove(DataStorage.InLineRestaurantList.FirstOrDefault(x => x.Name == restaurant.Name));
                                    DataStorage.WaitingList.Add(restaurant);
                                    return restaurant.Name + " was vetoed by " + pUser.Name + " and is now off the voting list.";
                                }
                            }
                        }
                    }
                }
            }

        }

        private static string ShowList(string pMessage, User pUser)
        {
            var list = "Voting line:" + Environment.NewLine;
            var i = 1;
            if (DataStorage.InLineRestaurantList.Count > 0)
            {
                foreach (var rest in DataStorage.InLineRestaurantList)
                {
                    list += i + ": " + rest.Name + $" ({rest.Vetos} vetos)" + Environment.NewLine;
                    i++;
                }
            }
            else
            {
                list += "None in voting list yet...";
            }

            list += "Waiting RestaurantList:" + Environment.NewLine;
            i = 1;
            foreach (var rest in DataStorage.WaitingList)
            {
                list += i + ": " + rest.Name + $" ({rest.Vetos} vetos) " + Environment.NewLine;
                i++;
            }
            return list;
        }

        private async Task<bool> GetModApprovalAsync(IMessageActivity pActivity, Restaurant pRestaurant)
        {
            var modUser = DataStorage.UserList.FirstOrDefault(user => user.IsModerator);
            if (modUser == null) return false;

            var modUserAccount = new ChannelAccount(modUser.Id, modUser.Name);
            var botUserAccount = new ChannelAccount(pActivity.Recipient.Id, "Lunch Bot");

            var connector = new ConnectorClient(new Uri(pActivity.ServiceUrl));
            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botUserAccount, modUserAccount);

            var message = Activity.CreateMessageActivity();
            message.From = botUserAccount;
            message.Recipient = modUserAccount;
            message.Conversation = new ConversationAccount(id: conversationId.Id);
            message.Text = $"Approve new restaurant: {pRestaurant.Name}? 1 for yes. 2 for no.";

            var response = await connector.Conversations.SendToConversationAsync((Activity)message);
            return response.Message == "1";
        }

        private async Task RequestUserModAsync(IMessageActivity pActivity, IDialogContext pContext)
        {
            //var query = from x in new PromptDialog.PromptString("p1", "p2", 1)
            //    from y in new PromptDialog.PromptString("p2", "p2", 1)
            //    select string.Join("", x, y);

            //pContext.Call(new FormDialog<LunchDialog>(), );

            return;
            var userAccount = pActivity.From;
            var botUserAccount = new ChannelAccount(pActivity.Recipient.Id, "Lunch Bot");

            var connector = new ConnectorClient(new Uri(pActivity.ServiceUrl));
            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botUserAccount, userAccount);
            var conversation = new ConversationAccount(id: conversationId.Id);

            var message = Activity.CreateMessageActivity();
            message.From = botUserAccount;
            message.Recipient = userAccount;
            message.Conversation = conversation;
            message.Text = "Enter the password to become a moderator: ";

            var response = connector.Conversations.SendToConversationAsync((Activity)message).Result;
            //int poo = 0;
            //while (response == null && poo < 10)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    poo ++;
            //}
            if (response.Message == ModPassword)
            {
                var acceptMessage = Activity.CreateMessageActivity();
                acceptMessage.From = botUserAccount;
                acceptMessage.Recipient = userAccount;
                acceptMessage.Conversation = conversation;
                acceptMessage.Text = "Congratulations. You have become a mod for lunch bot!";
                return;
            }
            var denyMessage = Activity.CreateMessageActivity();
            denyMessage.From = botUserAccount;
            denyMessage.Recipient = userAccount;
            denyMessage.Conversation = conversation;
            denyMessage.Text = "Sorry. Wrong password. Goodbye";
        }
    }


    //public class ModDialog : IDialog<object>
    //{
    //    public async Task StartAsync(IDialogContext context)
    //    {
    //        context.Wait(MessageReceivedAsync);
    //    }

    //    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    //    {
    //        var modActivity = await argument;
            
    //    }

    //}
}