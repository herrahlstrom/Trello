namespace TrelloWindow.PrintCompactModels
{
	internal class Member
	{
		public string Avatar30Px => string.IsNullOrWhiteSpace(AvatarHash) ? "" : $"https://trello-avatars.s3.amazonaws.com/{AvatarHash}/30.png";
		public string AvatarHash { get; set; }
		public string Initials { get; internal set; }
		public string Name { get; internal set; }
		public string Username { get; internal set; }
	}
}