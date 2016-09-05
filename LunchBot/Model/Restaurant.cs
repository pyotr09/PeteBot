using System;
using System.Collections.Generic;

namespace LunchBot.Model
{

	[Serializable]
	public class Restaurant
	{
		private List<User> _vetoList;
		private List<User> _votedList;

		public List<User> VetoList
		{
			get { return _vetoList ?? (_vetoList = new List<User>()); }
			set
			{
				_vetoList = value;
			}
		}
		public List<User> VotedList
		{
			get { return _votedList ?? (_votedList = new List<User>()); }
			set
			{
				_votedList = value;
			}
		}


		public Restaurant()
		{
			IsSeconded = false;
			IsVetoed = false;
			Vetos = 0;
			VotePoints = 0;
			LinePos = 9999999;
		}
		public string Name { get; set; }
		public bool IsSeconded { get; set; }
		public bool IsVetoed { get; set; }
		public User UserThatNomiated { get; set; }
		public User UserThatSeconded { get; set; }
		public int Vetos { get; set; }
		public double VotePoints { get; set; }
		public string Location { get; set; }
		public int LinePos { get; set; }
	}
}