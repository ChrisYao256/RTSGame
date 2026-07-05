using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Godot.HttpRequest;
using static System.Net.Mime.MediaTypeNames;

namespace RTSGame.Units;

public partial class TooltipRichTextLabel : RichTextLabel
{
	private static Dictionary<string, string> _imageAliases = new()
		{
				{ "::duration::", "[img=18x18]res://_Assets/Duration.png[/img]" },
				{ "::electricity::", "[img=18x18]res://_Assets/Electricity.png[/img]" },
				{ "::steel::", "[img=18x18]res://_Assets/Steel.png[/img]" },
				{ "::water::", "[img=18x18]res://_Assets/Water.png[/img]" },
				{ "::gas::", "[img=18x18]res://_Assets/Gas.png[/img]" },
		};

	public override void _Ready()
	{
		base._Ready();
		Text = ResolveImageAliases(Text);
		BbcodeEnabled = true;
		Godot.Collections.Array<Node> nodes = GetTree().Root.GetChildren();
		TooltipManager tooltipManager = GetTree().Root.GetNode<TooltipManager>("TdScene/TooltipManager");
		MetaHoverStarted += (meta) => tooltipManager.ShowTooltip(DecodeMetaString(meta.AsString()));
		MetaHoverEnded += (_) => tooltipManager.HideTooltip();
	}

	private static string ResolveImageAliases(string input)
	{
		string output = input;
		foreach (var pair in _imageAliases)
		{
			string shortTag = pair.Key;
			string fullTag = pair.Value;
			output = output.Replace(shortTag, fullTag);
		}
		return output;
	}

	public static string EncodeMetaString(string text, string topRightText)
	{
		text = ResolveImageAliases(text);
		text = text.Replace("[", "{").Replace("]", "}");
		text += $"|{ResolveImageAliases(topRightText).Replace("[", "{").Replace("]", "}")}|";
		return text;
	}

	public static (string, string) DecodeMetaString(string meta)
	{
		char symbol = '|';

		// Escape the symbol in case it's a regex reserved character (like $, *, +)
		string pattern = $"{Regex.Escape(symbol.ToString())}(.*?){Regex.Escape(symbol.ToString())}";
		Match match = Regex.Match(meta, pattern);

		if (match.Success)
		{
			string result = match.Groups[1].Value;
			string extractedResult = Regex.Replace(meta, pattern, "");
			return (extractedResult.Replace("{", "[").Replace("}", "]"), result.Replace("{", "[").Replace("}", "]"));
		}
		else
		{
			return (meta.Replace("{", "[").Replace("}", "]"), "");
		}
		
	}

	private Tween _flashTween;

	public void FlashRed()
	{
		if (_flashTween != null && _flashTween.IsRunning())
		{
			_flashTween.Kill();
		}

		// 2. Create a brand new tween
		_flashTween = CreateTween();

		// 3. Snap the color to bright red instantly
		Modulate = new Color(1, 0, 0, 1);

		// 4. Smoothly interpolate (fade) back to solid white over 1.0 seconds
		_flashTween.TweenProperty(
				this,
				"modulate",
				new Color(1, 1, 1, 1),
				1.0f // Duration in seconds
		).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}
}