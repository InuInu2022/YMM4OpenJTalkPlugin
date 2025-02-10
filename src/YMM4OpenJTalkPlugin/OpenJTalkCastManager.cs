namespace YMM4OpenJTalkPlugin;

public static class OpenJTalkCastManager
{
	static readonly Dictionary<string, OpenJTalkCastData> castData = [];

	internal static string GetTermUrl(string castName)
	{
		return GetCastData(castName).TermUrl;
	}

	internal static string GetAuthor(string castName)
	{
		return GetCastData(castName).Author;
	}

	internal static string GetContentId(string castName)
	{
		return GetCastData(castName).ContentId;
	}

	internal static OpenJTalkCastData GetCastData(string castName)
	{
		var result = castData.TryGetValue(castName, out var data);
		return !result || data is null
			? new OpenJTalkCastData("","","")
			: data;
	}
}
