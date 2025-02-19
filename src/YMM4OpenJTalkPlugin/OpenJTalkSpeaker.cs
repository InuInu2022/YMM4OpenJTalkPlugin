
using System.Buffers;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

using Epoxy;

using NAudio.Wave;

using SharpOpenJTalk;

using YmmeUtil.Ymm4;

using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4OpenJTalkPlugin;

public class OpenJTalkSpeaker : IVoiceSpeaker, IDisposable
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
	private bool _disposedValue;

	//IReadOnlyList<string> _presets;

	public OpenJTalkSpeaker(string voiceName)
	{
		SpeakerName = $"{voiceName}";
		var castData = OpenJTalkCastManager.GetCastData(voiceName);
		ID = $"{castData.Id}";
		//Resource = castData;
		SpeakerAuthor = castData.Author;
		SpeakerContentId = castData.ContentId;
		License = new OpenJTalkVoiceLicense(
			castData.Terms
		);
		_voiceName = voiceName;
		_styles = new(new Dictionary<string, double>(StringComparer.Ordinal));
		//_presets = [];
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
			await InitOpenJTalkAsync(_voiceName, parameter).ConfigureAwait(false);

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
				.Run(() => _jtalk.Synthesis(text, dumpAll: false))
				.ConfigureAwait(false);
			if (result)
			{
				ReadOnlySpan<byte> buf = [.. _jtalk.WavBuffer];

				// ノイズ部分を無音化
				var skipSamples = (int)(_jtalk.SamplingFrequency / 20.0);
				Span<byte> silence = skipSamples < 1024
					? stackalloc byte[skipSamples]
					: ArrayPool<byte>.Shared.Rent(skipSamples);
				silence.Clear();

				ReadOnlySpan<byte> modifiedBuf = [.. silence, .. buf[skipSamples..]];

				var rent = ArrayPool<byte>.Shared.Rent(modifiedBuf.Length);
				modifiedBuf.CopyTo(rent);
				ReadOnlyMemory<byte> readOnlyMemory = rent.AsMemory();

				if (skipSamples >= 1024)
				{
					ArrayPool<byte>.Shared.Return([.. silence]);
				}

#pragma warning disable MA0004 // Use Task.ConfigureAwait
				await using var waveFileWriter = new WaveFileWriter(
					filePath,
					new WaveFormat(48000, 16, 1));
#pragma warning restore MA0004 // Use Task.ConfigureAwait

				await using (waveFileWriter.ConfigureAwait(false))
				{
					await waveFileWriter
						.WriteAsync(readOnlyMemory)
						.ConfigureAwait(false);
				}

				ArrayPool<byte>.Shared.Return(rent, true);
			}
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
	static async ValueTask InitOpenJTalkAsync(
		string voiceId = "",
		IVoiceParameter? parameter = null
	)
	{
		var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
		var dic = Path.Combine(
			dir,
			"lib",
			"open_jtalk_dic_utf_8-1.11/"
		);

		var style = OpenJTalkCastManager.GetCastData(voiceId);
		var stylePath = "";
		if (parameter is OpenJTalkParameter param)
		{
			//TODO: 感情合成サポート
			//とりいそぎ一番値が大きいものを選択
			var index = param.ItemsCollection
				.Select((v, i) => (Index: i, v.Value))
				.OrderByDescending(x => x.Value)
				.First()
				.Index;
			stylePath = style.StylePaths.ElementAt(index).Value;
		}
		else
		{
			stylePath = style.StylePaths.First().Value;
		}
		var voice = Path.Combine(
			dir,
			"lib",
			"voices",
			$"{style.Id}",
			$"{stylePath}"
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

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{

			}
			_jtalk.Dispose();
			_disposedValue = true;
		}
	}

	~OpenJTalkSpeaker()
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
