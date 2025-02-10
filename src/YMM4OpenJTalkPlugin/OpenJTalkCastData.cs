namespace YMM4OpenJTalkPlugin;

/// <summary>
/// ボイス（音響モデル）のデータ
/// </summary>
/// <param name="TermUrl">規約</param>
/// <param name="Author">製作者</param>
/// <param name="ContentId">ニコニコID</param>
public record OpenJTalkCastData(
	string TermUrl,
	string Author,
	string ContentId,
	string DownloadUrl = ""
);
