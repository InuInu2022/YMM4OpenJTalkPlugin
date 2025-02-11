using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

/// <summary>
/// OpenJTalk の音声リソースを表します。
/// </summary>
/// <param name="name"></param>
/// <param name="stylePaths">key:感情スタイル名、value: .htsvoice へのパス</param>
/// <param name="terms"></param>
/// <param name="isDownloaded"></param>
/// <param name="fileSize"></param>
/// <param name="author"></param>
/// <param name="contentId"></param>
public class OpenJTalkResource(
	string name,
	string id,
	IDictionary<string, string> stylePaths,
	string terms = "",
	bool isDownloaded = true,
	string? fileSize = null,
	string author = "",
	string contentId = ""
) : IVoiceResource
{
	public string Name { get; init; } = name;
	public string Id { get; init; } = id;
 	public string Terms { get; init; } = terms;
	[JsonIgnore]
	public bool IsDownloaded { get; } = isDownloaded;
	public string? FileSize { get; } = fileSize;

	public string? Author { get; init; } = author;
	public string? ContentId { get; init; } = contentId;
	public IDictionary<string, string> StylePaths { get; init; } = stylePaths;

	public event EventHandler DownloadStarted = delegate { };
	public event PropertyChangedEventHandler? PropertyChanged;

	public Task DownloadAsync(ProgressMessage progress)
	{
		throw new NotImplementedException();
	}

	public Task<bool> HasUpdateAsync()
	{
		return new Task<bool>(() => false);
	}
}

public record PluginResources
{
	public IEnumerable<OpenJTalkResource>? OpenJTalkResources { get; set; }
}
