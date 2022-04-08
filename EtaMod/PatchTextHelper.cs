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
        if (ship != null)
            __result += DrawEta(galaxy, ship, mission, checkCD: true);
    }

    public static string DrawEta(Galaxy galaxy, Ship ship, ShipMission mission, bool checkCD = false)
    {
        string result = string.Empty;

        if (ship != null && mission != null && mission.Type != ShipMissionType.Undefined && galaxy != null)
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
                Point shipPos = new(ship.GalaxyX, ship.GalaxyY);
                ShipCommand shipCommand = mission.ResolveCurrentCommand();
                if (!shipCommand.IsEmpty)
                {
                    var currentTarget = mission.GetCurrentTarget(galaxy);
                    if (currentTarget != null)
                    {
                        Point targetPos = new(currentTarget.GalaxyX, currentTarget.GalaxyY);
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
                                result += getEtaAsString(shipPos, targetPos, ship.GetSpeed(), arrival);
                                goto lExit;
                            }
                        }
                        if (mission.PrimaryTargetType == ShipMissionTargetType.GalaxyCoordinates)
                            targetPos = new(mission.PrimaryGalaxyX, mission.PrimaryGalaxyY);
                    
                        result += getEtaAsString(shipPos, targetPos, ship.GetSpeed());
                        goto lExit;
                    }
                    else
                    {
                        StellarObject? stellarObject = null;
                        StellarObject? stellarObject2 = null;
                        Fleet? fleet = null;
                        Fleet? fleet2 = null;
                        Fleet? missionTargetFleet = null;
                        int galaxyX;
                        int galaxyY;
                        var missionTarget = mission.DetermineMissionTarget(galaxy, shipCommand, out galaxyX, out galaxyY, out missionTargetFleet, ref stellarObject, ref stellarObject2, ref fleet, ref fleet2);
                        if (missionTargetFleet != null)
                        {
                            Ship targetShip = missionTargetFleet.LeadShip;
                            if (targetShip != null)
                            {
                                Point targetPos = new(targetShip.GalaxyX, targetShip.GalaxyY);
                                result += getEtaAsString(shipPos, targetPos, ship.GetSpeed());
                            }
                        }
                        else if (galaxyX > 0 && galaxyY > 0)
                        {
                            Point targetPos = new(galaxyX, galaxyY);
                            result += getEtaAsString(shipPos, targetPos, ship.GetSpeed());
                        }
                    }
                }
            }
        }
    lExit:
        return result;
    }

    private static string getEtaAsString(Point shipPos, Point targetPos, double shipSpeed, double arrival = 0f)
    {
        if (arrival > 0f)
            goto calcTimeSpan;

        var distance = Math.Sqrt((double)Calculations.CalculateDistanceSquared(shipPos.X, shipPos.Y, targetPos.X, targetPos.Y));
        arrival = Math.Truncate(distance / shipSpeed);

    calcTimeSpan:
        TimeSpan t = TimeSpan.FromSeconds(arrival);

        var eta = t.ToString(@"mm\:ss");
        if (t.Days > 0)
            eta = t.ToString(@"d.hh\:mm\:ss");
        else if (t.Hours > 0)
            eta = t.ToString(@"hh\:mm\:ss");

        var text = " (ETA: " + eta + ")";
        return text;
    }
}
