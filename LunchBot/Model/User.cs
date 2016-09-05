using System;

namespace LunchBot.Model
{

	[Serializable]
	public class User
	{
		public User()
		{
			NumVetos = 2;
		}
		public string Name { get; set; }
		public string Id { get; set; }
		public bool IsModerator { get; set; }
		public int NumVetos { get; set; }
	}

}