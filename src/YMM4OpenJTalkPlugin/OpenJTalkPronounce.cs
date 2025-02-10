using System.ComponentModel.DataAnnotations;

using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Voice;
using YukkuriMovieMaker.UndoRedo;

namespace YMM4OpenJTalkPlugin;

public partial class OpenJTalkPronounce : UndoRedoable, IVoicePronounce
{
	public OpenJTalkPronounce()
	{
	}

	public void BeginEdit()
	{

	}

	public ValueTask EndEditAsync()
	{
		return ValueTask.CompletedTask;
	}

}
