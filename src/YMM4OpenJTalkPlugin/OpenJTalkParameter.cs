using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Voice;

using YMM4OpenJTalkPlugin.View;
using System.Diagnostics;

namespace YMM4OpenJTalkPlugin;

public partial class OpenJTalkParameter : VoiceParameterBase
{
	double _speed = 1.0;
	double _volume = 1.0;
	double _pitch;
	double _alpha = 0.55;
	double _intonation = 1.0;
	double _msdThreshold = 0.5;
	double _beta = 0.0;
	double _gvWeightSpectrum = 1.0;
	//double _husky;

	ImmutableList<OpenJTalkStyleParameter> _styles = [];
	string _voice = "";

	//ImmutableList<string>? _preset;

	public string Voice
	{
		get => _voice;
		set => Set(ref _voice, value);
	}

	#region synthesis_options

	#endregion

	[Display(Name = "話速", Description = "話速を調整")]
	[TextBoxSlider("F2", "", 0.2, 5, Delay = -1)]
	[Range(0.2, 5)]
	[DefaultValue(1.0)]
	public double Speed
	{
		get => _speed;
		set => Set(ref _speed, value);
	}

	[Display(Name = "大きさ", Description = "ボリュームを調整")]
	[TextBoxSlider("F2", "", 0, 50, Delay = -1)]
	[Range(0, 50)]
	[DefaultValue(1.0)]
	public double Volume
	{
		get => _volume;
		set => Set(ref _volume, value);
	}

	[Display(Name = "高さ", Description = "ピッチの高さを調整")]
	[TextBoxSlider("F2", "", -5, 10, Delay = -1)]
	[Range(-5, 10)]
	[DefaultValue(0.0)]
	public double Pitch
	{
		get => _pitch;
		set => Set(ref _pitch, value);
	}

	[Display(Name = "声質詳細", Description = "声質の詳細を調整")]
	[TextBoxSlider("F2", "", 0.0, 1.0, Delay = -1)]
	[Range(0.0, 1.0)]
	[DefaultValue(0.55)]
	public double Alpha
	{
		get => _alpha;
		set => Set(ref _alpha, value);
	}

	[Display(Name = "抑揚", Description = "抑揚を調整")]
	[TextBoxSlider("F2", "", 0.0, 2, Delay = -1)]
	[Range(0, 2)]
	[DefaultValue(1.0)]
	public double Intonation
	{
		get => _intonation;
		set => Set(ref _intonation, value);
	}

	const string DescMSDThreshold =
		"""
		無声音（休止や息の部分）と有声音の境界を決めるしきい値を調整
		高め 0.7～0.9  にすると無音が多くなり、はっきりした発音になるが、不自然になる可能性
		低め 0.2～0.4  にすると滑らかな発音になるが、無音部分が減って不明瞭になる可能性
		""";
	[Display(GroupName = "詳細",Name = "無声音閾値", Description = DescMSDThreshold)]
	[TextBoxSlider("F2", "", 0.0, 1.0, Delay = -1)]
	[Range(0, 1)]
	[DefaultValue(0.5)]
	public double MSDThreshold
	{
		get => _msdThreshold;
		set => Set(ref _msdThreshold, value);
	}

	const string DescBeta =
		"""
		状態間遷移確率の補正調整
		値を大きくすると音のつながりがより自然になるが、破綻しやすい
		推奨(0.0 ～ 0.3)
		""";

	[Display(GroupName = "詳細", Name = "遷移平滑度", Description = DescBeta)]
	[TextBoxSlider("F2", "", 0.0, 1.0, Delay = -1)]
	[Range(0, 1)]
	[DefaultValue(0.0)]
	public double Beta
	{
		get => _beta;
		set => Set(ref _beta, value);
	}

	const string DescGVWeightSpectrum =
		"""
		音声のスペクトルの変化を平均的に大きくすることで音質の変化やイントネーションの多様性を調整
		高め 1.5 ～ 2.0  にすると音がより自然で豊かな抑揚を持つが、ノイズや不安定な発音が発生することもある
		低め 0.5 ～ 0.8  にすると安定した合成音になるが、単調な音になりやすい
		""";

	[Display(GroupName = "詳細", Name = "スペクトル変動重み", Description = DescGVWeightSpectrum)]
	[TextBoxSlider("F2", "", 0, 2.0, Delay = -1)]
	[Range(0, 2)]
	[DefaultValue(1.0)]
	public double GVWeightSpectrum
	{
		get => _gvWeightSpectrum;
		set => Set(ref _gvWeightSpectrum, value);
	}

	/*
	[Display(Name = "Hus.", Description = "声質の擦れ具合を調整")]
	[TextBoxSlider("F2", "", -20, 20, Delay = -1)]
	[Range(-20, 20)]
	[DefaultValue(0.0)]
	public double Husky
	{
		get => _husky;
		set => Set(ref _husky, value);
	}
	*/

	[Display(AutoGenerateField = true)]
	[JsonProperty]
	public ImmutableList<OpenJTalkStyleParameter> ItemsCollection
	{
		get => _styles;
		set
		{
			UnsubscribeFromItems(_styles);
			try
			{
				Set(ref _styles, value);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.StackTrace);
			}

			SubscribeToItems(_styles);
		}
	}

	 // 個々のアイテムの PropertyChanged イベントに登録
    void SubscribeToItems(ImmutableList<OpenJTalkStyleParameter> newItems)
    {
        foreach (var item in newItems)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }
    }

	// 個々のアイテムの PropertyChanged イベントの登録解除
	void UnsubscribeFromItems(ImmutableList<OpenJTalkStyleParameter> oldItems)
    {
        foreach (var item in oldItems)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }
    }

	void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged($"{nameof(ItemsCollection)}.{e.PropertyName}");
	}
}
