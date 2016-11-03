using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBot
{
	public class VotingDialog
    {
        public static DateTime? VoteStarts { get; set; }
        public static TimeSpan? VoteDuration { get; set; }
        public static DateTime? NextUpdate { get; set; }
        public static TimeSpan? UpdateDuration { get; set; } = new TimeSpan(0, 0, 30);

		public static string GetReply(Activity activity)
		{
		    if (VoteStarts == null)
		    {
			    return null;
		    }

		    if (VoteDuration == null)
		    {
			    return "Can't start voting without a vote duration yo.";
		    }

		    if (UpdateDuration == null)
		    {
			    return "Can't start voting without an update duration yo.";
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
				    return $"Voting ends in {completionTime - now} ({completionTime})";
			    }
		    }

		    if (now > completionTime)
		    {
			    var stringBuilder = new StringBuilder("The Results are in!").AppendLine();
			    stringBuilder.Append("Location").Append("\t\t\t").Append("Points").AppendLine();

			    IList<ElectionResult> electionResults = Ballot.Instance.GetOrderedResults();

			    foreach (ElectionResult electionResult in electionResults)
			    {
				    stringBuilder.Append(electionResult.Text).Append("\t\t\t").Append(electionResult.Value).AppendLine();
			    }
			    VoteStarts = null;
			    return stringBuilder.ToString();
		    }
			return null;
		}
    }
}