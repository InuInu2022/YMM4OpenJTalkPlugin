using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using YMM4OpenJTalkPlugin.View;

using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public partial class OpenJTalkStyleParameter : VoiceParameterBase
{
	double _value;
	string _displayName = string.Empty;
	string _description = string.Empty;

	[JsonProperty]
	public string DisplayName { get => _displayName; init => Set(ref _displayName, value); }
	[JsonProperty]
	public string Description { get => _description; init => Set(ref _description, value); }

	[OpenJTalkStyleDisplay]
	[TextBoxSlider("F2", "", -1, 2, Delay = -1)]
	[Range(0, 1)]
	[DefaultValue(0.0)]
	[JsonProperty]
	public double Value { get => _value; set => Set(ref _value, value);}
}