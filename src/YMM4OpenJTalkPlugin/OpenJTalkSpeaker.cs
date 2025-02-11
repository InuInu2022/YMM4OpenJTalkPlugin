
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;

using Epoxy;

using NAudio.Wave;

using SharpOpenJTalk;

using YmmeUtil.Ymm4;

using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public class OpenJTalkSpeaker : IVoiceSpeaker
{
	public string EngineName => "OpenJTalk";
	public string SpeakerName { get; init; }
	public string API => "OpenJTalk";
	public string ID { get; init; }
	public bool IsVoiceDataCachingRequired => false;
	public SupportedTextFormat Format => SupportedTextFormat.Text;
	public IVoiceLicense? License { get; init; }
	public IVoiceResource? Resource { get; init; }
	public string? SpeakerAuthor { get; init; }
	public string? SpeakerContentId { get; init; }
	public string? EngineAuthor
		=> "Dept. of Computer Science, Nagoya Institute of Technology";
	public string? EngineContentId { get; }

	static readonly SemaphoreSlim Semaphore = new(1);

	readonly string _voiceName;
	static readonly OpenJTalkAPI _jtalk = new();
	ReadOnlyDictionary<string, double> _styles;
	IReadOnlyList<string> _presets;

	public OpenJTalkSpeaker(string voiceName, string id)
	{
		SpeakerName = $"{voiceName}";
		ID = $"YMM4OpenJTalk_{id}";
		var castData = OpenJTalkCastManager.GetCastData(id);
		SpeakerAuthor = castData.Author;
		SpeakerContentId = castData.ContentId;
		License = new OpenJTalkVoiceLicense(
			castData.TermUrl
		);
		_voiceName = id;
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

		await UIThread.InvokeAsync(() =>
		{
			TaskbarUtil.StartIndeterminate();
			return ValueTask.CompletedTask;
		}).ConfigureAwait(false);

		try
		{
			await InitOpenJTalkAsync().ConfigureAwait(false);

			if (parameter is OpenJTalkParameter param)
			{
				_jtalk.Speed = param.Speed;
				_jtalk.Volume = param.Volume;
				_jtalk.HarfTone = param.Pitch;
				_jtalk.Alpha = param.Alpha;
				_jtalk.GVWeightLF0 = param.Intonation;
				_jtalk.MSDThreshold = param.MSDThreshold;
				_jtalk.Beta = param.Beta;
				_jtalk.GVWeightSpectrum = param.GVWeightSpectrum;
			}
			_jtalk.FramePeriod = 240;
			_jtalk.SamplingFrequency = 48000;

			var result = await Task
				.Run(() => _jtalk.Synthesis(text))
				.ConfigureAwait(false);
			if (result)
			{
				var buf = _jtalk.WavBuffer.AsReadOnly();

				// ノイズ部分をカット
				const int skipSamples = (int)(4800 / 2.0);
				var trimmedBuf = buf.Skip(skipSamples).ToArray();

				#pragma warning disable MA0004 // Use Task.ConfigureAwait
				await using var waveFileWriter = new WaveFileWriter(
					filePath,
					new WaveFormat(48000, 16, 1));
				#pragma warning restore MA0004 // Use Task.ConfigureAwait

				await using (waveFileWriter.ConfigureAwait(false))
				{
					await waveFileWriter
						.WriteAsync(
							trimmedBuf)
						.ConfigureAwait(false);
				}
			}
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
		catch (Exception ex)
		{
			await Console.Error
				.WriteLineAsync($"ERROR! {ex.Message}")
				.ConfigureAwait(false);
			await UIThread.InvokeAsync(() =>
			{
				TaskbarUtil.ShowError();
				WindowUtil.FocusBack();
				return ValueTask.CompletedTask;
			}).ConfigureAwait(false);
		}
		finally
		{
			Semaphore.Release();

			await UIThread.InvokeAsync(() =>
			{
				TaskbarUtil.FinishIndeterminate();
				WindowUtil.FocusBack();
				return ValueTask.CompletedTask;
			}).ConfigureAwait(false);
		}
		return new OpenJTalkPronounce();
	}

	public IVoiceParameter CreateVoiceParameter()
	{
		if (
			!OpenJTalkSettings.Default.SpeakersStyles.TryGetValue(_voiceName, out var savedStyles)
		)
		{
			return new OpenJTalkParameter();
		}
		_styles = savedStyles.AsReadOnly();

		return new OpenJTalkParameter
		{
			Voice = _voiceName,
			ItemsCollection = _styles
				.Select(v => new OpenJTalkStyleParameter()
				{
					DisplayName = v.Key,
					Value = v.Value,
					Description = $"Style: {v.Key}",
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
		if (currentParameter is not OpenJTalkParameter vsParam)
		{
			return CreateVoiceParameter();
		}

		//声質切替で固有Styleが切り替わらないので強制再読み込みを掛ける
		var isSame = string.Equals(vsParam.Voice, _voiceName, StringComparison.Ordinal);
		return isSame ? currentParameter : CreateVoiceParameter();
	}

	/// <summary>
	/// initialize openjtalk
	/// </summary>
	/// <seealso cref="DisposeOpenJTalkAsync"/>
	static async ValueTask InitOpenJTalkAsync()
	{
		var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
		var dic = Path.Combine(
			dir,
			"lib",
			"open_jtalk_dic_utf_8-1.11/"
		);

		var voice = Path.Combine(
			dir,
			"lib",
			"voices",
			"tohoku-f01",
			"tohoku-f01-neutral.htsvoice"
		);

		var userdic = Path.Combine(
			dir,
			"lib",
			"userdic",
			"user.dic"
		);
		userdic = File.Exists(userdic) ? userdic : null;
		_ = await Task.Run(() => _jtalk.Initialize(dic, voice, userdic))
			.ConfigureAwait(false);
	}

	static async ValueTask DisposeOpenJTalkAsync()
	{
		await Task.Run(_jtalk.Dispose).ConfigureAwait(false);
	}
}
