using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LunchBot
{
	public class VotingDialog : IDialog<object>
    {
        public static DateTime? VoteStarts { get; set; }
        public static TimeSpan? VoteDuration { get; set; }
        public static DateTime? NextUpdate { get; set; }
        public static TimeSpan? UpdateDuration { get; set; } = new TimeSpan(0, 0, 30);
        public async Task StartAsync(IDialogContext context)
        {
	        await ProcessContext(context);
        }

	    private static async Task ProcessContext(IDialogContext context)
	    {
		    if (VoteStarts == null)
		    {
			    context.Done(new object());
			    return;
		    }

		    if (VoteDuration == null)
		    {
			    await context.PostAsync("Can't start voting without a vote duration yo.");
			    return;
		    }

		    if (UpdateDuration == null)
		    {
			    await context.PostAsync("Can't start voting without an update duration yo.");
			    return;
		    }

		    DateTime now = DateTime.Now;
		    DateTime completionTime = VoteStarts.Value.Add(VoteDuration.Value);
		    if (completionTime > now)
		    {
			    //first update
			    if (NextUpdate == null)
			    {
				    NextUpdate = VoteStarts.Value.Add(UpdateDuration.Value);
			    }

			    if (now > NextUpdate.Value)
			    {
				    NextUpdate = NextUpdate.Value.Add(UpdateDuration.Value);
				    await context.PostAsync($"Voting ends in {completionTime - now} ({completionTime})");
			    }
		    }

		    if (now > completionTime)
		    {
			    await context.PostAsync("The Results are in!");
			    var stringBuilder = new StringBuilder();
			    stringBuilder.Append("Location").Append("\t\t\t").Append("Points");

			    IList<ElectionResult> electionResults = Ballot.Instance.GetOrderedResults();

			    foreach (ElectionResult electionResult in electionResults)
			    {
				    stringBuilder.Append(electionResult.Text).Append("\t\t\t").Append(electionResult.Value);
			    }
			    await context.PostAsync(stringBuilder.ToString());
			    VoteStarts = null;
		    }

		    context.Done(new object());
	    }

	    public static async Task MesageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
	    {
		    await ProcessContext(context);
	    }
    }
}