
using System.ComponentModel.DataAnnotations;

using YukkuriMovieMaker.ItemEditor;

namespace YMM4OpenJTalkPlugin.View;

[AttributeUsage(AttributeTargets.Property)]
public sealed class OpenJTalkStyleDisplayAttribute : CustomDisplayAttributeBase
{
	public override string? GetDescription(object instance)
	{
		if (instance is not OpenJTalkStyleParameter styleParam) return null;
		return GetParamProp(styleParam, nameof(OpenJTalkStyleParameter.Description));
	}

	static string? GetParamProp(
		OpenJTalkStyleParameter styleParam,
		string propName)
	{
		var temp = styleParam
			.GetType()
			.GetProperty(propName)?
			.GetValue(styleParam);
		return temp is string propValue ? propValue : null;
	}

	public override string? GetGroupName(object instance) => null;

	public override string? GetName(object instance)
	{
		if (instance is not OpenJTalkStyleParameter styleParam) return null;

		return GetParamProp(styleParam, nameof(OpenJTalkStyleParameter.DisplayName));
	}

	public override bool? GetAutoGenerateField(object instance) => false;

	public override bool? GetAutoGenerateFilter(object instance) => true;

	public override int? GetOrder(object instance) => 0;
}
