using System.Collections.Generic;
using System.Linq;
using Chronic;

namespace LunchBot
{
    public class Ballot
    {
        private readonly string[] _locations;
        private readonly Dictionary<string, string> _votes = new Dictionary<string, string>();

        public Ballot(params string[] locations)
        {
            _locations = locations;
        }

        public void Cast(string user, string vote)
        {
            _votes[user] = vote;
        }

        public IList<ElectionResult> GetOrderedResults()
        {
            var dictionary = new Dictionary<string, int>();
            _locations.ForEach(x=>dictionary[x]=0);

            foreach (string vote in _votes.Values)
            {
                int weight = _locations.Length;
                IEnumerable<string> chads = vote.Split(',', ';', ':').Select(x=>x.Trim());
                foreach (string chad in chads)
                {
                    string temp = chad.ToTitleCase();
                    //If the user passed a string
                    if (_locations.Contains(temp))
                    {
                        dictionary[temp] += weight--;
                    }
                    else
                    {
                        //If the user passed an int
                        if (temp.EndsWith(".")) temp = temp.Substring(0, temp.Length - 1);
                        int result;
                        if (int.TryParse(temp, out result))
                        {
                            dictionary[_locations[result-1]] += weight--;
                        }
                    }
                }
            }

            return dictionary.Select(x => new ElectionResult(x.Key, x.Value))
                .OrderBy(x=>x.Value)
                .Reverse()
                .ToList();
        }
    }

    public class ElectionResult
    {
        public ElectionResult(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public int Value { get; }
        public string Text { get; }
    }
}