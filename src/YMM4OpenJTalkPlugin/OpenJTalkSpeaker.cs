
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Epoxy;

using YMM4OpenJTalkPlugin.ViewModel;

using YmmeUtil.Ymm4;

using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public class OpenJTalkSpeaker : IVoiceSpeaker
{
	public string EngineName => "OpenJTalk";
	public string SpeakerName { get; }
	public string API => "OpenJTalk";
	public string ID { get; }
	public bool IsVoiceDataCachingRequired => true;
	public SupportedTextFormat Format => SupportedTextFormat.Text;
	public IVoiceLicense? License { get; }
	//TODO: cast data json
	public IVoiceResource? Resource { get; init; }
	public string? SpeakerAuthor { get; }
	public string? SpeakerContentId { get; }
	public string? EngineAuthor { get; } = "Dept. of Computer Science, Nagoya Institute of Technology";
	public string? EngineContentId { get; }

	static readonly SemaphoreSlim Semaphore = new(1);
	//static readonly ITalkAutoService _service = new TalkServiceProvider()
	//	.GetService<ITalkAutoService>();

	readonly string _voiceName;
	ReadOnlyDictionary<string, double> _styles;
	IReadOnlyList<string> _presets;

	public OpenJTalkSpeaker(string voiceName)
	{
		SpeakerName = $"{voiceName}";
		ID = $"YMM4OpenJTalk{voiceName}";
		var castData = OpenJTalkCastManager.GetCastData(voiceName);
		SpeakerAuthor = castData.Author;
		SpeakerContentId = castData.ContentId;
		License = new OpenJTalkVoiceLicense(
			castData.TermUrl
		);
		_voiceName = voiceName;
		_styles = new(new Dictionary<string, double>(StringComparer.Ordinal));
		_presets = [];
	}

	public async Task<string> ConvertKanjiToYomiAsync(string text, IVoiceParameter voiceParameter)
	{
		return await Task.FromResult(text)
			.ConfigureAwait(false);
	}

	public async Task<IVoicePronounce?> CreateVoiceAsync(
		string text,
		IVoicePronounce? pronounce,
		IVoiceParameter? parameter,
		string filePath
	)
	{
		await Semaphore.WaitAsync().ConfigureAwait(false);

		await UIThread.InvokeAsync(()=>{
			TaskbarUtil.StartIndeterminate();
			return ValueTask.CompletedTask;
		}).ConfigureAwait(false);

		try
		{
			/*
			Console.WriteLine($"from ymm4 path: {filePath}");
			var sw = System.Diagnostics.Stopwatch.StartNew();
			await _service
				.SetCastAsync(SpeakerName)
				.ConfigureAwait(false);
			sw.Stop();
			Console.WriteLine($"set cast time: {sw.Elapsed.TotalSeconds}");
			sw.Restart();
			if(parameter is OpenJTalkParameter vstParam)
			{
				await _service.SetGlobalParamsAsync(
					new Dictionary<string,double>(StringComparer.Ordinal)
					{
						{nameof(vstParam.Speed), vstParam.Speed},
						{nameof(vstParam.Volume), vstParam.Volume},
						{nameof(vstParam.Pitch), vstParam.Pitch},
						{nameof(vstParam.Alpha), vstParam.Alpha},
						{"Into.", vstParam.Intonation},
						{"Hus.", vstParam.Husky},
					}
				).ConfigureAwait(false);

				await _service.SetStylesAsync(
					SpeakerName,
					vstParam.ItemsCollection
						.ToDictionary(
							x => x.DisplayName,
							x => x.Value,
							StringComparer.Ordinal)
				).ConfigureAwait(false);
			}
			sw.Stop();
			sw.Restart();
			var result = await _service
				.OutputWaveToFileAsync(text, filePath)
				.ConfigureAwait(false);
			Console.WriteLine($"output time: {sw.Elapsed.TotalSeconds}");
			if(!result){
				await Console.Error
					.WriteLineAsync($"ERROR! {nameof(CreateVoiceAsync)} : {text}")
					.ConfigureAwait(false);
				await UIThread.InvokeAsync(()=>{
					TaskbarUtil.ShowError();
					return ValueTask.CompletedTask;
				}).ConfigureAwait(false);
			}
			*/
		}
		catch(Exception ex)
		{
			await Console.Error
				.WriteLineAsync($"ERROR! {ex.Message}")
				.ConfigureAwait(false);
			await UIThread.InvokeAsync(()=>{
				TaskbarUtil.ShowError();
				WindowUtil.FocusBack();
				return ValueTask.CompletedTask;
			}).ConfigureAwait(false);
		}
		finally
		{
			Semaphore.Release();

			await UIThread.InvokeAsync(()=>{
				TaskbarUtil.FinishIndeterminate();
				WindowUtil.FocusBack();
				return ValueTask.CompletedTask;
			}).ConfigureAwait(false);
		}
		return new OpenJTalkPronounce();
	}

	public IVoiceParameter CreateVoiceParameter()
	{
		if(
			!OpenJTalkSettings.Default.SpeakersStyles.TryGetValue(_voiceName, out var savedStyles)
		){
			return new OpenJTalkParameter();
		}
		/*
		var hasPresets = OpenJTalkSettings.Default.SpeakersPresets
			.TryGetValue(_voiceName, out var savedPresets);
		_styles = savedStyles.AsReadOnly();
		if(hasPresets) _presets = savedPresets!.AsReadOnly();
		*/

		return new OpenJTalkParameter
		{
			Voice = _voiceName,
			ItemsCollection = _styles
				.Select(v => new OpenJTalkStyleParameter(){
					DisplayName=v.Key,
					Value=v.Value,
					Description=$"Style: {v.Key}",
				})
				.ToImmutableList(),
			//Preset = [.._presets],
		};
	}

	public bool IsMatch(string api, string id)
	{
		return string.Equals(api, API, StringComparison.Ordinal)
			&& string.Equals(id, ID, StringComparison.Ordinal);
	}

	public IVoiceParameter MigrateParameter(IVoiceParameter currentParameter)
	{
		if(currentParameter is not OpenJTalkParameter vsParam){
			return CreateVoiceParameter();
		}

		//声質切替で固有Styleが切り替わらないので強制再読み込みを掛ける
		var isSame = string.Equals(vsParam.Voice, _voiceName, StringComparison.Ordinal);
		return isSame ? currentParameter : CreateVoiceParameter();
	}
}
