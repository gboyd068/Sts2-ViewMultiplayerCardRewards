using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Abstracts;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace BaseLib.Patches.Hooks;

[HarmonyPatch(
    typeof(CardFactory),
    nameof(CardFactory.CreateForReward),
    new[]
    {
        typeof(Player),
        typeof(int),
        typeof(CardCreationOptions)
    })]
class BeforeRewardOfferedPatch
{
    [HarmonyPostfix]
    public static IEnumerable<CardCreationResult> CardCreationHook(
        IEnumerable<CardCreationResult> __result,
        Player player,
        int cardCount,
        CardCreationOptions options)
    {
        var cards = __result.Select(r => r.Card);
        CustomMessageWrapper.Send(new RewardsOfferedMessage(player, cards));
        return __result;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeRoomEntered), MethodType.Async)]
class BeforeRoomEnteredPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> BeforeRoom(ILGenerator generator, IEnumerable<CodeInstruction> instructions,
        MethodBase original)
    {
        return AsyncMethodCall.Create(generator, instructions, original,
            AccessTools.Method(typeof(BeforeRoomEnteredPatch), nameof(BeforeRoomEnteredHooks)),
            beforeState: original);
    }

    private static async Task BeforeRoomEnteredHooks(IRunState runState, AbstractRoom room)
    {
        CardRewardsMap.Instance.Map.Clear();
    }
}