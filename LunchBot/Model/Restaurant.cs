using System;
using System.Collections.Generic;

namespace LunchBot.Model
{

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
}