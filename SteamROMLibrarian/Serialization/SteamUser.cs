using System.Collections;
using ValveKeyValue;

namespace SteamROMLibrarian.Serialization;

internal class SteamLoginUsersVDF : IEnumerable<SteamLoginUser>
{
	public SteamLoginUsersVDF(List<SteamLoginUser> usersList)
	{
		this.Users = usersList;
	}

	[KVIgnore]
	public List<SteamLoginUser> Users { get; }

	public static SteamLoginUsersVDF Load(string filename)
	{
		using var fs = File.OpenRead(filename);
		var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

		KVObject? kvObject;
		try
		{
			kvObject = kv.Deserialize(fs);
		}
		catch (Exception e)
		{
			throw new InvalidVDFException($"Can't load user list from {filename}!", e);
		}

		if (kvObject.Name != "users")
			throw new InvalidVDFException($"Key \"users\" not found in {filename}");

		var users = new List<SteamLoginUser>();
		foreach (var u in kvObject.Children)
			users.Add(new SteamLoginUser(u.Name, u.Value));

		return new SteamLoginUsersVDF(users);
	}

	#region IEnumerable

	public IEnumerator<SteamLoginUser> GetEnumerator()
	{
		return this.Users.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.Users.GetEnumerator();
	}

	#endregion
}

internal class SteamLoginUser
{
	public SteamLoginUser(string userID, KVValue baseObject)
	{
		this.UserID = userID;
		this.AccountName = (string)baseObject["AccountName"];
		this.PersonaName = (string)baseObject["PersonaName"];
		this.RememberPassword = (string)baseObject["RememberPassword"];
		this.WantsOfflineMode = (string)baseObject["WantsOfflineMode"];
		this.SkipOfflineModeWarning = (string)baseObject["SkipOfflineModeWarning"];
		this.AllowAutoLogin = (string)baseObject["AllowAutoLogin"];
		this.MostRecent = (string)baseObject["MostRecent"];
		this.Timestamp = (string)baseObject["Timestamp"];
	}

	public string UserID { get; }
	public string AccountName { get; }
	public string PersonaName { get; }
	public string RememberPassword { get; }
	public string WantsOfflineMode { get; }
	public string SkipOfflineModeWarning { get; }
	public string AllowAutoLogin { get; }
	public string MostRecent { get; }
	public string Timestamp { get; }
}
