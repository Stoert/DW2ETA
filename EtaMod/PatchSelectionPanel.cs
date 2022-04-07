using System.Diagnostics.CodeAnalysis;
using DistantWorlds2;
using DistantWorlds.UI;
using DistantWorlds.Types;

using Xenko.Core.Mathematics;
using Xenko.Graphics;
using HarmonyLib;
using JetBrains.Annotations;

namespace EtaMod;

[PublicAPI]
[HarmonyPatch(typeof(SelectionPanel))]

public class PatchSelectionPanel
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SelectionPanel), "DrawGraphValue", new Type[] { typeof(SpriteBatch), typeof(string), typeof(SpriteFont), typeof(SpriteFont), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(Color), typeof(Color), typeof(Color), typeof(string) })]

    static bool DrawGraphValue(SelectionPanel __instance, SpriteBatch spriteBatch, string label, SpriteFont labelFont, SpriteFont valueFont, float x, float y, float gap, float labelWidth, float graphWidth, float graphHeight, float valueWidth, float value, float maximum, Color graphColor1, Color graphColor2, Color graphBackColor, string valueSuffix)
    {
		if (label.Equals(TextResolver.GetText("Speed").ToUpperInvariant()))
		{
			var _ship = CustomOrbRenderer.GetSelectedObject();
			if (_ship != null && _ship is Ship)
			{
				Ship ship = (Ship)_ship;

				string text = maximum.ToString(Constants.NumberFormat);
				// value = ship.Speed
				// maximum = ship.Summary.TopSpeed
				maximum = Math.Max(maximum, value);
				float totalLineSpacing = labelFont.GetTotalLineSpacing(labelFont.Size);
				float num = 0.5f * (graphHeight - totalLineSpacing);
				Vector2 position = new Vector2(x, y + num);
				Vector2 maximumSize = new Vector2(labelWidth, graphHeight);

				// draw label SPEED
				if (!string.IsNullOrEmpty(label))
				{
					DrawingHelper.DrawStringDropShadow(spriteBatch, label, labelFont, __instance.ForeColorResolved, __instance.ShadowColorResolved, position, maximumSize, TextAlignment.Right);
				}
				float num2 = x + labelWidth + gap;
				float width = graphWidth * (Math.Max(0f, value) / Math.Max(1f, maximum));
				RectangleF rectangle = new RectangleF(num2, y, graphWidth, graphHeight);
				DrawingHelper.FillRectangle(spriteBatch, rectangle, graphBackColor);
				RectangleF rectangleF = new RectangleF(num2, y, width, graphHeight);
				DrawingHelper.FillRectangleGradient(spriteBatch, rectangleF, graphColor1, graphColor2, false);
				DrawingHelper.DrawTextureTiled(spriteBatch, DrawingHelper.RasterizedLines, rectangleF, Vector2.Zero, new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 4));
				float totalLineSpacing2 = valueFont.GetTotalLineSpacing(valueFont.Size);
				float num3 = 0.5f * (graphHeight - totalLineSpacing2);
				position = new Vector2(num2, y + num3);
				maximumSize = new Vector2(graphWidth, __instance.LineHeight);
				string text2 = value.ToString(Constants.NumberFormat);

				if (!string.IsNullOrEmpty(valueSuffix))
				{
					text2 += GG_Mod.PatchTextHelper.DrawEta(ship.GetGalaxy(), ship, ship.Mission, checkCD: true);
				}
				else
				{
					// draw ETA
					if (ship.Mission != null && ship.IsHyperjumping() && ship.GetSpeed() > 0f)
						text2 += GG_Mod.PatchTextHelper.DrawEta(ship.GetGalaxy(), ship, ship.Mission);
				}

				DrawingHelper.DrawStringDropShadow(spriteBatch, text2, valueFont, __instance.ForeColorResolved, __instance.ShadowColorResolved, position, maximumSize, TextAlignment.Center);
				position = new Vector2(num2 + graphWidth + gap, y + num3);
				DrawingHelper.DrawStringDropShadow(spriteBatch, text, valueFont, __instance.ForeColorResolved, position);

				return false;	
			}
		}

		return true;
    }
}
