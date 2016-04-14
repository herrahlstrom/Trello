namespace TrelloApi.Cache
{
	interface IRequestCache
	{
		string GetValue(string key);
		void SetValue(string key, string value);
	}
}
