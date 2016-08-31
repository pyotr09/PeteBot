using System;

namespace LunchBot.Model
{

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

}