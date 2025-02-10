using System.ComponentModel;

using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public class OpenJTalkResource : IVoiceResource
{


	public string Name { get; init; }
	public string Terms { get; init; }
	public bool IsDownloaded { get; }
	public string? FileSize { get; }

	public string? Author { get; init; }
	public string? ContentId { get; init; }

	public event EventHandler DownloadStarted = delegate { };
	public event PropertyChangedEventHandler? PropertyChanged;

	public OpenJTalkResource(
		string name,
		string terms = "",
		bool isDownloaded = false,
		string? fileSize = null,
		string author = "",
		string contentId = ""
	)
	{
		Name = name;
		Terms = terms;
		IsDownloaded = isDownloaded;
		FileSize = fileSize;
		Author = author;
		ContentId = contentId;
	}

	public Task DownloadAsync(ProgressMessage progress)
	{
		throw new NotImplementedException();
	}

	public Task<bool> HasUpdateAsync()
	{
		throw new NotImplementedException();
	}
}
