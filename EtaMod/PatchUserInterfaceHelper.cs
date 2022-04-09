using System.Diagnostics.CodeAnalysis;
using DistantWorlds2;
using DistantWorlds.UI;
using DistantWorlds.Types;
using HarmonyLib;
using JetBrains.Annotations;
using Xenko.Graphics;
using Xenko.Core.Mathematics;

namespace EtaMod;

[PublicAPI]
[HarmonyPatch(typeof(UserInterfaceHelper))]

public class PatchUserInterfaceHelper
{

	// SelectionList Fleet Mouse-Hover
	[HarmonyPostfix]
	[HarmonyPatch(nameof(UserInterfaceHelper), "DrawFleetSummaryDetailed")]
	public static void DrawFleetSummaryDetailed(SpriteBatch spriteBatch, RectangleF rectangle, float margin, float gap, SpriteFont headerFont, SpriteFont normalFont, SpriteFont boldFont, Color textColor, Color shadowColor, Color fillColor, Galaxy galaxy, Empire empire, Fleet fleet, float __result)
	{
		float leftX = rectangle.X + margin;
		float pointY = rectangle.Y + margin;
		float totalLineSpacing = headerFont.GetTotalLineSpacing(headerFont.Size);
		float totalLineSpacing2 = normalFont.GetTotalLineSpacing(normalFont.Size);
		if (fleet != null && fleet.LeadShip != null)
		{
			Empire empire2 = fleet.GetEmpire();
			if (empire2 != null)
			{
				float lineSpacing = totalLineSpacing + totalLineSpacing2;
				float pointX = leftX + (lineSpacing + gap);
				Color primaryColor = empire2.PrimaryColor;
				DrawingHelper.DrawStringDropShadow(spriteBatch, fleet.Name, headerFont, primaryColor, new Vector2(pointX, pointY));

				Vector2 vector = headerFont.MeasureString(fleet.Name);
				var sEta = PatchTextHelper.DrawEta(galaxy, fleet.LeadShip, fleet.Mission, checkCD: true);
				DrawingHelper.DrawStringDropShadow(spriteBatch, sEta, UserInterfaceHelper.FontSmall, Color.Gold, new Vector2(pointX + vector.X + gap, pointY));
			}
		}
	}

	// SelectionList Ship Mouse-Hover
	[HarmonyPrefix]
	[HarmonyPatch(typeof(UserInterfaceHelper), "DrawShipSummary", new Type[] { typeof(SpriteBatch), typeof(RectangleF), typeof(float), typeof(float), typeof(SpriteFont), typeof(SpriteFont), typeof(SpriteFont), typeof(Color), typeof(Color), typeof(Galaxy), typeof(Empire), typeof(Ship) })]
	public static bool DrawShipSummary(SpriteBatch spriteBatch, RectangleF rectangle, float margin, float gap, SpriteFont headerFont, SpriteFont normalFont, SpriteFont boldFont, Color textColor, Color fillColor, Galaxy galaxy, Empire empire, Ship ship, ref float __result)
	{
		float num = rectangle.X + margin;
		float num2 = rectangle.Y + margin;
		float totalLineSpacing = headerFont.GetTotalLineSpacing(headerFont.Size);
		float totalLineSpacing2 = normalFont.GetTotalLineSpacing(normalFont.Size);
		float totalWidth = rectangle.Width - (margin * 2f);
		float num4 = rectangle.Right - margin;
		float num5a = totalLineSpacing2 * 0.15f;
		if (ship != null)
		{
			Texture image = ship.GetImage();
			Empire empire2 = ship.GetEmpire();
			if (empire2 != null)
			{
				if (fillColor != Color.Transparent)
				{
					DrawingHelper.FillRectangle(spriteBatch, rectangle, fillColor);
				}
				float num5 = totalLineSpacing + totalLineSpacing2;
				if (image != null && !image.IsDisposed)
				{
					RectangleF destination;
					destination = new(num, num2, num5, num5);
					DrawingHelper.DrawTextureUndistorted(spriteBatch, image, destination);
				}
				float num6 = num + (num5 + gap);
				Color primaryColor = empire2.PrimaryColor;
				DrawingHelper.DrawStringDropShadow(spriteBatch, ship.Name, headerFont, primaryColor, new Vector2(num6, num2 + num5a));
				float num7 = totalLineSpacing * 1.6666666f;
				if (empire2.Flag != null && !empire2.Flag.IsDisposed)
				{
					RectangleF destination2;
					destination2 = new(num4 - num7, num2, num7, num7);
					DrawingHelper.DrawTextureUndistorted(spriteBatch, empire2.Flag, destination2);
				}

				string text1 = string.Empty;
				Design design = ship.GetDesign();
				if (design != null)
					text1 = design.Name;
				else
					text1 = TextHelper.ResolveDescription(ship.Role);
				if (ship.Fleet != null)
					text1 += "  (" + ship.Fleet.Name + ")";
				DrawingHelper.DrawStringDropShadow(spriteBatch, text1, UserInterfaceHelper.FontSmall, primaryColor, new Vector2(num6, num2 + totalLineSpacing));
				
				string text = TextHelper.ResolveMissionDescription(galaxy, empire, ship, ship.Mission);
				Vector2 vector = normalFont.MeasureString(text);
				float maximumWidth = UserInterfaceHelper.CalculateScaledValue(378f);
				int alternateColorRowCount = 0;
				var alternateRowColor = UserInterfaceController.HoverInfo.AlternateRowColor;
				var shadowColor = UserInterfaceController.HoverInfo.ShadowColorResolved;
				num2 += vector.Y + num5a;

				if (vector.X > maximumWidth)
				{
					List<string> lines;
					vector = DrawingHelper.MeasureStringDropShadowWordWrapWithSize(text, normalFont, maximumWidth, out lines);
					FillBackgroundAlternateColor(spriteBatch, ref alternateColorRowCount, num, num2 + totalLineSpacing, maximumWidth, lineHeight: vector.Y, lineSeparatorHeight: num5a, alternateRowColor);
					DrawingHelper.DrawStringDropShadowWordWrapLines(spriteBatch, lines, normalFont, textColor, shadowColor, num, num2 + totalLineSpacing, maximumWidth, 0);
					num2 += vector.Y + num5a * 2f;
				}
				else
				{
					if (string.IsNullOrEmpty(text))
					{
						text = "(" + TextResolver.GetText("No mission") + ")";
						if (ship.CheckForIncomingFuelTanker(galaxy, empire))
						{
							text = "(" + TextResolver.GetText("Waiting for fuel tanker") + ")";
						}
					}
					DrawingHelper.DrawStringDropShadow(spriteBatch, text, normalFont, textColor, new Vector2(num, num2 + totalLineSpacing));
					num2 += totalLineSpacing2;
				}

				num2 += totalLineSpacing;
				num2 += margin;
				float firstColumnWidth = totalWidth * 0.5f;
				FormattedTextGrid formattedTextGrid = new FormattedTextGrid();
				formattedTextGrid.Initialize(2, 8, totalWidth, firstColumnWidth, totalLineSpacing2 + margin);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Role"), textColor, normalFont, null, TextAlignment.Right), 0, 0);
				formattedTextGrid.SetText(new FormattedText(TextHelper.ResolveDescription(ship.Role), textColor, boldFont, null, TextAlignment.Center), 1, 0);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Size"), textColor, normalFont, null, TextAlignment.Right), 0, 1);
				formattedTextGrid.SetText(new FormattedText(ship.GetShipHull().Size.ToString(Constants.NumberFormat), textColor, boldFont, null, TextAlignment.Center), 1, 1);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Strength"), textColor, normalFont, null, TextAlignment.Right), 0, 2);
				formattedTextGrid.SetText(new FormattedText(ship.CalculateStrengthWithFighters().ToString("0"), textColor, boldFont, null, TextAlignment.Center), 1, 2);
				formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Strength"], new Vector2(totalLineSpacing2, totalLineSpacing2), true), 1, 2);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Troop Attack Strength"), textColor, normalFont, null, TextAlignment.Right), 0, 3);
				formattedTextGrid.SetText(new FormattedText(ship.Troops.GetTotalAttackStrength(false).ToString(Constants.NumberFormat), textColor, boldFont, null, TextAlignment.Center), 1, 3);
				formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Troops"], new Vector2(totalLineSpacing2, totalLineSpacing2), true), 1, 3);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Health"), textColor, normalFont, null, TextAlignment.Right), 0, 4);
				formattedTextGrid.SetText(new FormattedText(ship.Health.ToString("0%"), textColor, boldFont, null, TextAlignment.Center), 1, 4);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Fleet"), textColor, normalFont, null, TextAlignment.Right), 0, 5);
				string text2 = "(" + TextResolver.GetText("None") + ")";
				if (ship.Fleet != null)
				{
					text2 = ship.Fleet.Name;
				}
				formattedTextGrid.SetText(new FormattedText(text2, textColor, boldFont, null, (TextAlignment)1), 1, 5);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Maintenance Cost"), textColor, normalFont, null, (TextAlignment)2), 0, 6);
				formattedTextGrid.SetText(new FormattedText(string.Format(TextResolver.GetText("X credits"), ship.GetMaintenanceCost().ToString(Constants.NumberFormat)), textColor, boldFont, null, (TextAlignment)1), 1, 6);
				formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Money"], new Vector2(totalLineSpacing2, totalLineSpacing2), true), 1, 6);
				formattedTextGrid.SetText(new FormattedText(TextResolver.GetText("Fuel"), textColor, normalFont, null, (TextAlignment)2), 0, 7);
				formattedTextGrid.SetText(new FormattedText(ship.Fuel.ToString("0"), textColor, boldFont, null, (TextAlignment)1), 1, 7);
				formattedTextGrid.Draw(spriteBatch, new Vector2(num, num2));
				num2 += formattedTextGrid.Size.Y;
				num2 += margin;
			}
		}
		__result = num2 - rectangle.Top;
		return false;
	}

	private static bool FillBackgroundAlternateColor(SpriteBatch spriteBatch, ref int alternateColorRowCount, float x, float y, float width, float lineHeight, float lineSeparatorHeight, Color alternateBackColor)
	{
		alternateColorRowCount++;
		if (alternateColorRowCount % 2 == 1)
		{
			y -= lineSeparatorHeight * 0.5f;
			RectangleF rectangle;
			rectangle = new(x, y, width, lineHeight + lineSeparatorHeight);
			DrawingHelper.FillRectangle(spriteBatch, rectangle, alternateBackColor);
			return true;
		}
		return false;
	}
}

