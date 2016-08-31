using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Web.Services.Description;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;

namespace LunchBot
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

        private Activity HandleSystemMessage(Activity message)
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
                User User1 = new User();
                User1.id = activity.From.Id;
                if (DataStorage.UserList.Exists(x => x.id == User1.id))
                {
                    var User2 = DataStorage.UserList.FirstOrDefault(x => x.id == User1.id);
                    if (true) //check mod 
                    {

                        PromptDialog.Confirm(
                            context,
                            AfterResetAsync,
                            "Are you sure you want to reset the lists? (yes or no)",
                            $"Didn't get that {User2.name}! Are you sure you want to reset the lists? (yes or no)",
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
                await context.PostAsync("Reset lists.");
            }
            else
            {
                await context.PostAsync("Did not reset lists.");
            }
            context.Wait(MessageReceivedAsync);
        }

        private string ExecuteCommand(IMessageActivity act)
        {
            string message = act.Text;
            User User = new User();
            User.id = act.From.Id;
            User.name = act.From.Name;
            if (!DataStorage.UserList.Exists(x => x.id == User.id))
            {
                //send request to moderator for approval
                DataStorage.UserList.Add(User);
            }
            else
            {
                User = DataStorage.UserList.FirstOrDefault(x => x.id == User.id);
            }
            if (message.ToLower().StartsWith("nominate "))
            {
                message = message.Substring(9);
                Restaurant rest = new Restaurant();
                rest.name = message.ToLower();
                rest.userThatNomiated = User;
                if (true)//mod approval needed here
                {

                    if (DataStorage.RestaurantList.Exists(x => x.name == rest.name))
                    {
                        return "Already in list.";
                    }

                    rest.linePos = DataStorage.RestaurantList.Count;
                    DataStorage.RestaurantList.Add(rest);
                    return rest.name + " added to the list by and needs a second! Nominated by " + User.name;
                }
                else
                {
                    return "Request DENIED.";
                }

            }
            else if (message.ToLower().StartsWith("second "))
            {
                message = message.Substring(7);
                Restaurant rest = new Restaurant();
                rest.name = message.ToLower();

                if (DataStorage.RestaurantList.Exists(x => x.name == rest.name))
                {
                    if (!DataStorage.InLineRestaurantList.Exists(x => x.name == rest.name))
                    {
                        var restOld = DataStorage.RestaurantList.FirstOrDefault(x => x.name == rest.name);
                        if (restOld.userThatNomiated.id != User.id)//dont allow someone to second their own nominations    fix me
                        {
                            restOld.userThatSeconded = User;
                            restOld.isSeconded = true;
                            if (DataStorage.InLineRestaurantList.Count < 5)
                            {
                                DataStorage.InLineRestaurantList.Add(restOld);
                                return restOld.name + " has been seconded and is in " + DataStorage.InLineRestaurantList.Count + " position in voting order.";
                            }
                            else
                            {
                                DataStorage.WaitingList.Add(restOld);
                                return restOld.name + " has been seconded and is waiting at " + DataStorage.WaitingList.Count + " position in waiting order.";
                            }
                        }
                        else
                        {
                            return "You can't second your own nomination silly!";
                        }

                    }
                    return "You cannot second something twice!";

                }
                return "Can't second something that hasn't been nominated.";

            }
            else if (message.ToLower().StartsWith("veto "))
            {
                message = message.Substring(5);
                Restaurant rest = new Restaurant();
                rest.name = message.ToLower();
                {
                    if (DataStorage.RestaurantList.Exists(x => x.name == rest.name))
                    {
                        rest = DataStorage.RestaurantList.FirstOrDefault(x => x.name == rest.name);
                        if (User.vetos > 0)
                        {
                            if (!rest.vetoList.Exists(x => x.id == User.id))
                            {
                                rest = DataStorage.RestaurantList.FirstOrDefault(x => x.name == rest.name);
                                rest.vetoList.Add(User);
                                rest.vetos++;
                                User.vetos--;
                                //send veto confirmed message
                                if (rest.vetos >= 3)
                                {
                                    rest.isVetoed = true;
                                    if (DataStorage.InLineRestaurantList.Exists(x => x.name == rest.name))
                                    {
                                        DataStorage.InLineRestaurantList.Remove(DataStorage.InLineRestaurantList.FirstOrDefault(x => x.name == rest.name));
                                        DataStorage.WaitingList.Add(rest);
                                        return rest.name + " was vetoed by " + User.name + " and is now off the voting list.";
                                    }
                                    return rest.name + " was vetoed by " + User.name + " and is already on waiting list";
                                }
                                return rest.name + " was vetoed by " + User.name + " and needs " + (3 - rest.vetos) + " more veto(s) to be knocked out of voting.";
                            }
                            else
                            {
                                return "You cannot veto something more than once!";
                            }

                        }
                        else
                        {
                            return User.name + " you have no more vetos to use.";
                        }
                    }
                    else
                    {
                        return "You cannot veto things that weren't nominated.";
                    }

                }

            }
            else if (message.ToLower().Contains("show list"))
            {
                string list = "Voting line:" + Environment.NewLine;
                int i = 1;
                if (DataStorage.InLineRestaurantList.Count > 0)
                {
                    foreach (var rest in DataStorage.InLineRestaurantList)
                    {
                        list += i + ": " + rest.name  + $" ({rest.vetos} vetos)" + Environment.NewLine;
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
                    list += i + ": " + rest.name + $" ({rest.vetos} vetos) " + Environment.NewLine;
                    i++;
                }
                return list;
            }
            else if (message.ToLower().Contains("mod me"))
            {
                
            }
            return "";
        }

        private async Task<bool> GetModApprovalAsync(IMessageActivity pActivity, Restaurant pRestaurant)
        {
            var modUser = DataStorage.UserList.FirstOrDefault(user => user.isModerator);
            if (modUser == null) return false;

            var modUserAccount = new ChannelAccount(modUser.id, modUser.name);
            var botUserAccount = new ChannelAccount(pActivity.Recipient.Id, "Lunch Bot");

            var connector = new ConnectorClient(new Uri(pActivity.ServiceUrl));
            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botUserAccount, modUserAccount);

            var message = Activity.CreateMessageActivity();
            message.From = botUserAccount;
            message.Recipient = modUserAccount;
            message.Conversation = new ConversationAccount(id: conversationId.Id);
            message.Text = $"Approve new restaurant: {pRestaurant.name}? 1 for yes. 2 for no.";

            var response = await connector.Conversations.SendToConversationAsync((Activity)message);
            if (response.Message == "1") return true;
            return false;
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


        [Serializable]
        public class User
        {
            public User()
            {
                vetos = 2;
            }
            public string name { get; set; }
            public string id { get; set; }
            public bool isModerator { get; set; }
            public int vetos { get; set; }
        }


        [Serializable]
        public class Restaurant
        {
            private List<User> _vetoList;
            private List<User> _votedList;

            public List<User> vetoList
            {
                get
                {
                    if (_vetoList == null)
                    {
                        _vetoList = new List<User>();
                    }
                    return _vetoList;
                }
                set
                {
                    _vetoList = value;
                }
            }
            public List<User> votedList
            {
                get
                {
                    if (_votedList == null)
                    {
                        _votedList = new List<User>();
                    }
                    return _votedList;
                }
                set
                {
                    _votedList = value;
                }
            }


            public Restaurant()
            {
                isSeconded = false;
                isVetoed = false;
                vetos = 0;
                votePoints = 0;
                linePos = 9999999;
            }
            public string name { get; set; }
            public bool isSeconded { get; set; }
            public bool isVetoed { get; set; }
            public User userThatNomiated { get; set; }
            public User userThatSeconded { get; set; }
            public int vetos { get; set; }
            public double votePoints { get; set; }
            public string location { get; set; }
            public int linePos { get; set; }
        }

        [Serializable]
        public class DataStorage
        {
            private static List<Restaurant> _restaurantList;
            private static List<Restaurant> _restaurantListLine;
            private static List<Restaurant> _restaurantListWait;
            private static List<User> _userList;

            public static List<Restaurant> RestaurantList
            {
                get
                {
                    if (_restaurantList == null)
                    {
                        _restaurantList = new List<Restaurant>();
                    }
                    return _restaurantList;
                }
                set
                {
                    _restaurantList = value;
                }
            }
            public static List<Restaurant> InLineRestaurantList
            {
                get
                {
                    if (_restaurantListLine == null)
                    {
                        _restaurantListLine = new List<Restaurant>();
                    }
                    return _restaurantListLine;
                }
                set
                {
                    _restaurantListLine = value;
                }
            }
            public static List<Restaurant> WaitingList
            {
                get
                {
                    if (_restaurantListWait == null)
                    {
                        _restaurantListWait = new List<Restaurant>();
                    }
                    return _restaurantListWait;
                }
                set
                {
                    _restaurantListWait = value;
                }
            }

            public static List<User> UserList
            {
                get
                {
                    if (_userList == null)
                    {
                        _userList = new List<User>();
                    }
                    return _userList;
                }
                set
                {
                    _userList = value;
                }
            }
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