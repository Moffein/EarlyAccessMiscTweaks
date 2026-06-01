using BepInEx;
using EarlyAccessMiscTweaks.Tweaks;
using System;
using System.Security.Permissions;
using System.Security;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace EarlyAccessMiscTweaks
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.EarlyAccessMiscTweaks", "EarlyAccessMiscTweaks", "1.0.0")]
    public class EarlyAccessMiscTweaksPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new ArtificerTweaks();
            new DroneBuffs();
        }
    }
}
