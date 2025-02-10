
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public record OpenJTalkVoiceLicense : IVoiceLicense
{
	//規約をどこに表示するか
	public VoiceLicenseDisplayLocation SummaryLocation
		=> VoiceLicenseDisplayLocation.CharacterEditor;
	//規約概要。なくてもよい
	public string? Summary { get; } = "音響モデル（ボイスライブラリ）の規約も確認してください。";
	public bool IsTermsAgreed { get; set; } = true;
	//ここにメッセージがあるとリンク文字列をクリックすると承認ダイアログ表示
	public string? Terms { get; }
	//TermsがnullならこちらのURLへ飛ぶようになる
	public string? TermsURL { get; }
		= "https://open-jtalk.sourceforge.net/";

	public OpenJTalkVoiceLicense(
		string? voiceTermsUrl = null
	)
	{
		if(voiceTermsUrl is not null)
		{
			TermsURL = voiceTermsUrl;
		}
	}

	public ValueTask<bool> ValidateLicenseAsync()
	{
		return new ValueTask<bool>(true);
	}
}