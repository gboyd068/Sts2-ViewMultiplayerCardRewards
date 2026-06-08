using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Abstracts;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace BaseLib.Patches.Hooks;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeRewardsOffered), MethodType.Async)]
class BeforeRewardOfferedPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> BeforeOffer(ILGenerator generator, IEnumerable<CodeInstruction> instructions,
        MethodBase original)
    {
        return AsyncMethodCall.Create(generator, instructions, original,
            AccessTools.Method(typeof(BeforeRewardOfferedPatch), nameof(BeforeRewardsOfferedHooks)),
            beforeState: original);
    }

    private static async Task BeforeRewardsOfferedHooks(IRunState runState,
        Player player,
        IReadOnlyList<Reward> rewards)
    { 
        CustomMessageWrapper.Send(new RewardsOfferedMessage(player, rewards));
    }
}