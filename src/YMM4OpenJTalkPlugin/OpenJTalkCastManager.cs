using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace YMM4OpenJTalkPlugin;

public static class OpenJTalkCastManager
{
	static Dictionary<string, OpenJTalkResource> castData = [];

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0004:Use Task.ConfigureAwait", Justification = "<保留中>")]
	internal static async ValueTask InitAsync()
	{
		var fileName = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
			"lib",
			"voices",
			"voices.json"
		);
		await using FileStream openStream = File.OpenRead(fileName);
		var result = await JsonSerializer
			.DeserializeAsync<PluginResources>(openStream)
			.ConfigureAwait(false);
		if (result is null) { return; }
		castData = result
			.OpenJTalkResources?
			.Select(v => new KeyValuePair<string, OpenJTalkResource>(v.Name, v))
			.ToDictionary()
			?? [];
	}

	internal static IReadOnlyCollection<string> GetCastNames()
	{
		return [.. castData.Select(v => v.Value.Name)];
	}

	internal static OpenJTalkResource GetCastData(string castName)
	{
		var result = castData.TryGetValue(castName, out var data);
		return !result || data is null
			? new OpenJTalkResource("", "", new Dictionary<string, string>(StringComparer.Ordinal))
			: data;
	}
	internal static OpenJTalkResource GetCastData(int index)
	{
		var result = castData.ElementAtOrDefault(index).Value;
		return result;
	}

	internal static Dictionary<string, Dictionary<string, double>> GetCastStyles()
	{
		return castData
			.Select(static v => new KeyValuePair<string, Dictionary<string, double>>(
				v.Value.Id,
				ToStyleWeights(v)
			))
			.ToDictionary();

		static Dictionary<string, double> ToStyleWeights(KeyValuePair<string, OpenJTalkResource> v)
		{
			return v.Value.StylePaths.Count == 0
				? []
				: v.Value.StylePaths
					.Select((x, i) => i == 0 ? (x.Key, 1.0) : (x.Key, 0.0))
					.ToDictionary();
		}
	}
}
