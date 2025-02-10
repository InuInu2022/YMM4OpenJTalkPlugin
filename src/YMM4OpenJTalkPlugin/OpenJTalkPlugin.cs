using System.Reflection;

using YmmeUtil.Common;

using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

/// <summary>
/// OpenJTalkをYMM4上で直接使うためのプラグイン
/// </summary>
[PluginDetails(AuthorName = "InuInu", ContentId = "")]
public class OpenJTalkPlugin : IVoicePlugin
{


	public string Name => "YMM4 OpenJTalk プラグイン";
	public PluginDetailsAttribute Details =>
		GetType().GetCustomAttribute<PluginDetailsAttribute>() ?? new();

	public IEnumerable<IVoiceSpeaker> Voices
		=> OpenJTalkSettings
			.Default
			.Speakers
			.Select(v => new OpenJTalkSpeaker(v));
	public bool CanUpdateVoices { get; }
	public bool IsVoicesCached
		=> OpenJTalkSettings.Default.IsCached;

	public OpenJTalkPlugin()
	{
		Console.WriteLine(
			$"{nameof(OpenJTalkPlugin)}: {AssemblyUtil.GetVersionString(typeof(OpenJTalkPlugin))}"
		);
	}

	public Task UpdateVoicesAsync()
	{
		throw new NotImplementedException();
	}
}