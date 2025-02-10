using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;

using Epoxy;

using YmmeUtil.Common;
using YmmeUtil.Ymm4;

namespace YMM4OpenJTalkPlugin.ViewModel;

[ViewModel]
public class TalkSettingViewModel
{
	public string? PluginVersion { get; }
	public string UpdateMessage { get; set; } = "Update checkボタンを押してください";

	public bool IsPreloading { get;set;}
	public bool IsPreloadButtonEnabled { get; set; } = true;
	public bool HasUpdate { get; set; }
	public bool IsUpdateCheckEnabled { get; set; } = true;
	public bool IsDownloadable { get; set; }

	public Command PreloadVoice { get; set; }
	public Command UpdateCheck { get; set; }
	public Command Download { get; set; }
	public Command OpenGithub { get; init; }
	public Command OpenSourceForge { get; init; }

	private readonly UpdateChecker checker;

	public TalkSettingViewModel()
	{
		PluginVersion = AssemblyUtil.GetVersionString(typeof(OpenJTalkPlugin));

		checker = UpdateChecker
			.Build("InuInu2022", "YMM4OpenJTalkPlugin");

		//PreloadVoice = Command.Factory.Create(PreloadAsync);

		UpdateCheck = Command.Factory.Create(async () =>
		{
			IsUpdateCheckEnabled = false;
			TaskbarUtil.StartIndeterminate();

			HasUpdate = await checker
				.IsAvailableAsync(typeof(TalkSettingViewModel))
				.ConfigureAwait(true);
			IsDownloadable = HasUpdate;
			UpdateMessage = await GetUpdateMessageAsync().ConfigureAwait(true);

			IsUpdateCheckEnabled = true;
			TaskbarUtil.FinishIndeterminate();
		});

		Download = Command.Factory.Create(async () =>
		{
			try
			{
				var result = await checker.GetDownloadUrlAsync(
					"YMM4OpenJTalkPlugin.",
					"https://github.com/InuInu2022/YMM4OpenJTalkPlugin/releases")
					.ConfigureAwait(false);
				await OpenUrlAsync(result)
					.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				await Console.Error.WriteLineAsync(e.Message).ConfigureAwait(false);
			}
		});


		OpenGithub = Command.Factory.Create(async () =>
			await OpenUrlAsync("https://github.com/InuInu2022/YMM4OpenJTalkPlugin")
				.ConfigureAwait(false)
		);

		OpenSourceForge = Command.Factory.Create(async () =>
			await OpenUrlAsync("https://open-jtalk.sourceforge.net/")
				.ConfigureAwait(false)
		);
	}

	static async Task<Process> OpenUrlAsync(string openUrl)
	{
		return await Task.Run(()
			=> Process.Start(new ProcessStartInfo()
			{
				FileName = openUrl,
				UseShellExecute = true,
			}) ?? new()
		).ConfigureAwait(false);
	}

	async ValueTask<string> GetUpdateMessageAsync(){
		return HasUpdate
			? $"プラグインの更新があります {await checker.GetRepositoryVersionAsync().ConfigureAwait(false)}"
			: "プラグインは最新です";
	}
}
