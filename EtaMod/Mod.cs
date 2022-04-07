using DistantWorlds2;
using HarmonyLib;
using JetBrains.Annotations;

namespace EtaMod;

[PublicAPI]
public class Mod
{
    public Mod(DWGame game)
        => new Harmony(nameof(EtaMod)).PatchAll();
}
