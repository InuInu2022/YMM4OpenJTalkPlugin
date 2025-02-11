using YukkuriMovieMaker.Plugin;
using YMM4OpenJTalkPlugin.ViewModel;
using YukkuriMovieMaker.Plugin.Voice;
using System.Collections.ObjectModel;
using Epoxy;
using YmmeUtil.Ymm4;
using System.Diagnostics.CodeAnalysis;

namespace YMM4OpenJTalkPlugin;

/// <summary>
/// 設定クラス
/// Speaker一覧をキャッシュする
/// </summary>
public partial class OpenJTalkSettings : SettingsBase<OpenJTalkSettings>
{
	public override SettingsCategory Category
		=> SettingsCategory.Voice;
	public override string Name => "YMM4 OpenJTalk";
	public override bool HasSettingView => true;
	public override object? SettingView
	{
		get
		{
			var view = new YMM4OpenJTalkPlugin.View.TalkSettingsView
			{
				DataContext = new TalkSettingViewModel()
			};
			return view;
		}
	}

	public bool IsCached
	{
		get { return _isCached; }
		set { Set(ref _isCached, value); }
	}
	public string[] Speakers
	{
		get { return _speakers; }
		set { Set(ref _speakers, value); }
	}
	[SuppressMessage("Design", "MA0016")]
	public Dictionary<string, Dictionary<string, double>> SpeakersStyles
	{
		get { return _speakersStyles; }
		set { Set(ref _speakersStyles, value); }
	}

	[SuppressMessage("Design", "MA0016")]
	public Dictionary<string, IList<string>> SpeakersPresets
	{
		get { return _speakersPresets; }
		set { Set(ref _speakersPresets, value); }
	}

	//ITalkAutoService? _service;
	bool _isCached;
	string[] _speakers = [];
	Dictionary<string, Dictionary<string, double>> _speakersStyles = new(StringComparer.Ordinal);
	Dictionary<string, IList<string>> _speakersPresets = new(StringComparer.Ordinal);

	public override void Initialize()
	{
		//var provider = new TalkServiceProvider();
		//_service = provider.GetService<ITalkAutoService>();
	}

	/// <summary>
	/// 話者一覧を更新する
	/// </summary>
	public async Task UpdateSpeakersAsync()
	{
		await UIThread.InvokeAsync(() => {
			TaskbarUtil.StartIndeterminate();
			return ValueTask.CompletedTask;
		}).ConfigureAwait(false);

		await OpenJTalkCastManager
			.InitAsync()
			.ConfigureAwait(false);

		Speakers = [.. OpenJTalkCastManager.GetCastNames()];
		SpeakersStyles = OpenJTalkCastManager.GetCastStyles();

		await UIThread.InvokeAsync(() =>
		{
			TaskbarUtil.FinishIndeterminate();
			TaskbarUtil.ShowNormal();
			return ValueTask.CompletedTask;
		}).ConfigureAwait(false);



		IsCached = true;

		await UIThread.InvokeAsync(()=>{
			TaskbarUtil.FinishIndeterminate();
			WindowUtil.FocusBack();
			return ValueTask.CompletedTask;
		}).ConfigureAwait(false);
	}
}
