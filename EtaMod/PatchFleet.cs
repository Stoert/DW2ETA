using System.Diagnostics.CodeAnalysis;
using DistantWorlds.Types;
using Xenko.Graphics;
using Xenko.Core.Mathematics;
using HarmonyLib;
using JetBrains.Annotations;

namespace EtaMod;

[PublicAPI]
[HarmonyPatch(typeof(Fleet))]
[SuppressMessage("ReSharper", "InconsistentNaming")]

public class PatchFleet
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Fleet.DrawFleetSummaryDetail))]
    public static bool DrawFleetSummaryDetail(SpriteBatch spriteBatch, Galaxy galaxy, Empire empire, Fleet fleet, float lineHeight, float lineHeightMajor, Vector2 position, float x, float y, float width, float textWidth, Vector2 nameSize, float gap, float smallGap, float largeGap, float extraGapRight, Color fontColor, Color shadowColor, SpriteFont font, SpriteFont boldFont, ref HoverTipList hoverTips)
    {
        float num = y + lineHeightMajor;
        CharacterList characters = fleet.Characters;
        float num2 = 0f;
        if (characters.Count > 0)
        {
            float num3 = lineHeight * 1.25f;
            float num4 = position.X + width - (num3 + extraGapRight);
            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                if (character != null && (character.PrimaryRole == CharacterRole.FleetAdmiral || character.PrimaryRole == CharacterRole.TroopGeneral) && character.GetImage() != null && !character.GetImage().IsDisposed)
                {
                    RectangleF rectangleF = new RectangleF(num4 - num2, num - (num3 - lineHeight), num3, num3);
                    DrawingHelper.DrawTextureUndistorted(spriteBatch, character.GetImage(), rectangleF);
                    num2 += rectangleF.Width;
                    num2 += smallGap;
                }
            }
        }

        string missionDesc = TextHelper.ResolveMissionDescription(galaxy, empire, null, fleet.Mission);
        Vector2 vector;
        DrawingHelper.DrawStringDropShadow(spriteBatch, missionDesc, font, fontColor, shadowColor, new Vector2(x, num), new Vector2(textWidth, lineHeight), 0, out vector);

        x += vector.X;

        if (fleet.LeadShip != null && fleet.Mission != null && fleet.Mission.Type != ShipMissionType.Undefined)
        {
            var countDown = PatchTextHelper.DrawEta(galaxy, fleet.LeadShip, fleet.LeadShip.Mission, checkCD: true);

            // draw hyper drive countdown
            if (!String.IsNullOrEmpty(countDown))
                DrawingHelper.DrawStringDropShadow(spriteBatch, countDown, UserInterfaceHelper.FontSmall, Color.Gold, new Vector2(x, num));
            // draw ETA
            if (fleet.LeadShip.IsHyperjumping() && fleet.LeadShip.GetSpeed() > 0f)
            {
                var eta = PatchTextHelper.DrawEta(galaxy, fleet.LeadShip, fleet.LeadShip.Mission);
                if (!String.IsNullOrEmpty(eta))
                    DrawingHelper.DrawStringDropShadow(spriteBatch, eta, UserInterfaceHelper.FontSmall, Color.Gold, new Vector2(x, num));
            }
        }

        float num5 = Math.Min(textWidth * 0.6f, textWidth - (largeGap + nameSize.X + extraGapRight));
        float num6 = position.X + width - (num5 + extraGapRight);
        FormattedTextGrid formattedTextGrid = new FormattedTextGrid();
        formattedTextGrid.SetTransparentBackground();
        formattedTextGrid.PaddingHorizontal = 0f;
        float item = 0.333f * (num5 - lineHeight);
        List<float> columnWidths = new List<float>
            {
                item,
                item,
                item,
                lineHeight
            };
        formattedTextGrid.Initialize(4, 1, columnWidths, lineHeight);
        formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Fleet"], new Vector2(lineHeight, lineHeight), true), 0, 0);
        FormattedText text2 = new FormattedText(fleet.Ships.Count.ToString("0"), fontColor, boldFont, null, TextAlignment.Center);
        formattedTextGrid.SetText(text2, 0, 0);
        formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Strength"], new Vector2(lineHeight, lineHeight), true), 1, 0);
        FormattedText text3 = new FormattedText(fleet.TotalStrength.ToString(Constants.NumberFormat), fontColor, boldFont, null, TextAlignment.Center);
        formattedTextGrid.SetText(text3, 1, 0);
        int num7 = fleet.CountTroops();
        float num8 = fleet.CalculateTroopStrengthAttack(false);
        formattedTextGrid.SetImage(new FormattedImage(UserInterfaceHelper.IconImages["Troops"], new Vector2(lineHeight, lineHeight), true), 2, 0);
        string.Concat(string.Format(TextResolver.GetText("X Troops"), num7.ToString("0")) + "\n", string.Format(TextResolver.GetText("X Attack Strength"), num8.ToString(Constants.NumberFormat)));
        FormattedText text4 = new FormattedText(num8.ToString(Constants.NumberFormat), fontColor, boldFont, null, TextAlignment.Center);
        formattedTextGrid.SetText(text4, 2, 0);
        Color tintColor;
        Sprite imageSprite = fleet.ResolveRoleSprite(out tintColor);
        formattedTextGrid.SetImage(new FormattedImage(imageSprite, new Vector2(lineHeight, lineHeight), true, tintColor), 3, 0);
        string text5 = TextHelper.ResolveDescription(fleet.Role);
        float num9 = num6 + formattedTextGrid.ColumnWidths[0] + formattedTextGrid.ColumnWidths[1] + formattedTextGrid.ColumnWidths[2];
        HoverTip hoverTip = new HoverTip(string.Empty, text5, new RectangleF(num9, y + smallGap, formattedTextGrid.ColumnWidths[3], lineHeight), num5, Side.Bottom);
        FormattedText text6 = new FormattedText(string.Empty, fontColor, boldFont, hoverTip, TextAlignment.Center);
        formattedTextGrid.SetText(text6, 3, 0);
        formattedTextGrid.Draw(spriteBatch, new Vector2(num6, y + smallGap), ref hoverTips);

        return false;
    }

}

