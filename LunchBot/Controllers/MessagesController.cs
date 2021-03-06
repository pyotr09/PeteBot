﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
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
            if (activity != null)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Ping:
                        var reply = VotingDialog.GetReply(activity);
                        if (string.IsNullOrEmpty(reply)) return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));

                        await connectorClient.Conversations.ReplyToActivityAsync(activity.CreateReply(reply));
                        break;
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => new NominationsDialog());
                        break;
                }
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

            return null;
        }
    }
}