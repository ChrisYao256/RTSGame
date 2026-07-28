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

				{ "::projectile::", "[img=18x18]res://_Assets/ProjectileWeapon.png[/img]" },
				{ "::laser::", "[img=18x18]res://_Assets/LaserWeapon.png[/img]" },
				{ "::flame::", "[img=18x18]res://_Assets/FlameWeapon.png[/img]" },
				{ "::scanner::", "[img=18x18]res://_Assets/ScannerWeapon.png[/img]" },
				{ "::ballistic::", "[img=18x18]res://_Assets/BallisticWeapon.png[/img]" },
				{ "::electric::", "[img=18x18]res://_Assets/ElectricWeapon.png[/img]" },

				{	"::miniboss::",  "[img=18x18]res://_Assets/MiniBoss.png[/img]"},
				{ "::lateminiboss::",  "[img=18x18]res://_Assets/LateMiniBoss.png[/img]"},
				{ "::midminiboss::",  "[img=18x18]res://_Assets/MidMiniBoss.png[/img]"},
				{ "::finalboss::",  "[img=18x18]res://_Assets/FinalBoss.png[/img]"},
		};

	private bool _isUpdatingText = false;

	public new string Text
	{
		get => GetText();
		set
		{
			// If we are currently inside our custom text formatting logic,
			// just assign the raw text and stop to break the loop!
			if (_isUpdatingText)
			{
				SetText(value);
				return;
			}

			try
			{
				_isUpdatingText = true;

				// 1. Set the initial text
				SetText(value);

				// 2. Process custom modifications (e.g., auto-formatting BBCode, bolding links)
				OnTextChanged(value);
			}
			finally
			{
				// Always release the lock when done
				_isUpdatingText = false;
			}
		}
	}

	private void OnTextChanged(string newText)
	{
		// Your custom logic runs automatically whenever text is assigned!
		Text = ResolveImageAliases(Text);
		Text = SetBoldText(Text);
	}

	public override void _Ready()
	{
		base._Ready();
		MetaUnderlined = false;
		Text = ResolveImageAliases(Text);
		Text = SetBoldText(Text);
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

	private static string SetBoldText(string input)
	{
		string pattern = @"(\[url[=\]].*?\])(.*?)(\[/url\])";
		return Regex.Replace(input, pattern, "$1[i]$2[/i]$3");
	}

	/// <summary>
	/// Replaces [ with { so that [color], [b], [img], etc. can be placed in [url]. In other words, use this if tooltip contains colors, image, or even another tooltip.
	/// </summary>
	/// <returns></returns>
	public static string EncodeMetaString(string text, string topRightText)
	{
		text = ResolveImageAliases(text);
		text = text.Replace("[", "{").Replace("]", "}");
		text += $"|{ResolveImageAliases(topRightText).Replace("[", "{").Replace("]", "}")}|";
		text = text.Replace("'", "’");
		return text;
	}

	/// <summary>
	/// Replaces { with [, reverting EncodeMetaString. Should not be called outside of TooltipRichTextLabel. 
	/// </summary>
	/// <returns></returns>
	private static (string, string) DecodeMetaString(string meta)
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