using DistantWorlds.Types;
using HarmonyLib;
using JetBrains.Annotations;
using Xenko.Core.Mathematics;

namespace EtaMod;

[PublicAPI]
[HarmonyPatch(typeof(TextHelper))]
public class PatchTextHelper
{

    // Draw ETA for single ship
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TextHelper), "ResolveMissionDescription", new Type[] { typeof(Galaxy), typeof(Empire), typeof(Ship), typeof(ShipMission) })]
    public static void ResolveMissionDescription(Galaxy galaxy, Empire empire, Ship ship, ShipMission mission, ref string __result)
    {
        __result += DrawEta(galaxy, ship, mission, checkCD: true);
    }

    public static string DrawEta(Galaxy galaxy, Ship ship, ShipMission mission, bool checkCD = false)
    {
        string result = string.Empty;

        if (ship != null && mission != null && galaxy != null)
        {
            if (checkCD)
            {
                var countdown = ship.HyperDriveCountdown;

                // draw hyper-drive countdown
                if (countdown > 0f)
                {
                    string cd = " (" + TextResolver.GetText("Jumping") + ": " + countdown.ToString("0") + ")";

                    if (ship.EnemyHyperDenyActive)
                        cd = " (" + TextResolver.GetText("HyperDeny") + " " + TextResolver.GetText("Active").ToLowerInvariant() + ")";

                    result = cd;
                }
            }

            // draw ETA
            if (ship.IsHyperjumping() && ship.GetSpeed() > 0f)
            {
                ShipCommand shipCommand = mission.ResolveCurrentCommand();
                if (!shipCommand.IsEmpty)
                {
                    var currentTarget = mission.GetCurrentTarget(galaxy);
                    if (currentTarget != null)
                    {
                        Point targetPos = new(currentTarget.GalaxyX, currentTarget.GalaxyY);
                        Point shipPos = new(ship.GalaxyX, ship.GalaxyY);
                        double distance = 0f;
                        double arrival = 0f;
                        var progress = (int)mission.LocationPathProgress;
                        bool bIsLastWpt = false;
                        if (mission.LocationIdPath != null)
                        {
                            var LocatationIdPath = mission.LocationIdPath.ToArray();

                            if (mission.LocationIdPath != null && progress < LocatationIdPath.Length)
                            {
                                Location? lastLocationStop = null;
                                for (int i = (int)progress; i < LocatationIdPath.Length; i++)
                                {
                                    int id = LocatationIdPath[i];
                                    Location byId = galaxy.Locations.GetById(id);
                                    if (byId != null)
                                    {
                                        targetPos = new(byId.GalaxyX, byId.GalaxyY);
                                        bIsLastWpt = i == LocatationIdPath.Length - 1;
                                        if (bIsLastWpt)
                                        {
                                            int galaxyX;
                                            int galaxyY;
                                            mission.ResolveCommandGalaxyCoordinates(out galaxyX, out galaxyY);
                                            if (galaxyX >= 0 && galaxyY >= 0)
                                                targetPos = new(galaxyX, galaxyY);
                                            else
                                                targetPos = new(currentTarget.GalaxyX, currentTarget.GalaxyY);
                                        }

                                        if (lastLocationStop != null && lastLocationStop != byId)
                                            distance = Math.Sqrt((double)Calculations.CalculateDistanceSquared(lastLocationStop.GalaxyX, lastLocationStop.GalaxyY, targetPos.X, targetPos.Y));
                                        else
                                            distance = Math.Sqrt((double)Calculations.CalculateDistanceSquared(shipPos.X, shipPos.Y, targetPos.X, targetPos.Y));

                                        arrival += Math.Truncate(distance / (double)ship.GetSpeed());

                                        lastLocationStop = byId;
                                        shipPos = targetPos;
                                    }
                                }
                                goto lDrawEta;

                            }
                        }
                        if (mission.PrimaryTargetType == ShipMissionTargetType.GalaxyCoordinates)
                        {
                            targetPos = new(mission.PrimaryGalaxyX, mission.PrimaryGalaxyY);
                        }

                        shipPos = new Point(ship.GalaxyX, ship.GalaxyY);
                        distance = Math.Sqrt((double)Calculations.CalculateDistanceSquared(shipPos.X, shipPos.Y, targetPos.X, targetPos.Y));
                        arrival = Math.Truncate(distance / (double)ship.GetSpeed());

                        lDrawEta:
            
                        TimeSpan t = TimeSpan.FromSeconds(arrival);

                        var eta = t.ToString(@"mm\:ss");
                        if (t.Days > 0)
                            eta = t.ToString(@"d.hh\:mm\:ss");
                        else if (t.Hours > 0)
                            eta = t.ToString(@"hh\:mm\:ss");

                        if (eta.Equals("00:00") && currentTarget.IsHyperjumping() && currentTarget.GetSpeed() > 0f)
                            return "";

                        //var text = " ETA Wpt" + (LocatationIdPath.Length - 1 - progress).ToString() + ": " + eta;
                        //if (bIsLastWpt)
                        //    text = " ETA: " + eta;

                        var text = " (ETA: " + eta + ")";
                        result += text;

                    }
                }
            }
        }

        return result;
    }
}
