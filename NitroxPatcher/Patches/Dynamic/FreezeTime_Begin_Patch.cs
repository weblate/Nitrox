using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UWE;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FreezeTime_Begin_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => FreezeTime.Begin(default));

        public static bool Prefix(FreezeTime.Id id)
        {
            return id == FreezeTime.Id.FeedbackPanel;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
