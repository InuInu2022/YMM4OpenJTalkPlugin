using System.Reflection;

using YmmeUtil.Common;

using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

/// <summary>
/// OpenJTalkをYMM4上で直接使うためのプラグイン
/// </summary>
[PluginDetails(AuthorName = "InuInu", ContentId = "")]
public class OpenJTalkPlugin : IVoicePlugin, IDisposable
{
	private bool _disposedValue;

	public string Name => "YMM4 OpenJTalk プラグイン";
	public PluginDetailsAttribute Details =>
		GetType().GetCustomAttribute<PluginDetailsAttribute>() ?? new();

	public IEnumerable<IVoiceSpeaker> Voices
		=> OpenJTalkSettings
			.Default
			.Speakers
			.Select((v,i) => new OpenJTalkSpeaker(v));
	public bool CanUpdateVoices { get; } = true;
	public bool IsVoicesCached
		=> OpenJTalkSettings.Default.IsCached;

	public OpenJTalkPlugin()
	{
		Console.WriteLine(
			$"{nameof(OpenJTalkPlugin)}: {AssemblyUtil.GetVersionString(typeof(OpenJTalkPlugin))}"
		);
	}

	public async Task UpdateVoicesAsync()
	{
		await OpenJTalkSettings
			.Default
			.UpdateSpeakersAsync()
			.ConfigureAwait(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				foreach (var voice in Voices)
				{
					if (voice is not IDisposable disposable) continue;
					disposable.Dispose();
				}
			}

			// アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// 大きなフィールドを null に設定します
			_disposedValue = true;
		}
	}

	// 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	~OpenJTalkPlugin()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}