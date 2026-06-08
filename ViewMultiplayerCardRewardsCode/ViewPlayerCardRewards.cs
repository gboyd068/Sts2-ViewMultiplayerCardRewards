using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;


[HarmonyPatch]
internal static class CardRewardViewerPatch
{
    [HarmonyPatch(typeof(NMultiplayerPlayerExpandedState), "_Ready")]
    [HarmonyPostfix]
    private static void AfterReady(NMultiplayerPlayerExpandedState __instance)
    {
        Control cardContainerAbove = Traverse.Create(__instance).Field("_cardContainer").GetValue<Control>();
        MegaRichTextLabel label = Traverse.Create(__instance).Field("_cardsHeader").GetValue<MegaRichTextLabel>();
        MegaRichTextLabel rewardsLabel = label.Duplicate() as MegaRichTextLabel;
        rewardsLabel.Text = "[gold]Current Card Rewards[/gold]";
        var cardRewardContainer = new HBoxContainer();
        cardRewardContainer.Size = new Vector2(600, 600);
        cardRewardContainer.GlobalPosition = cardContainerAbove.GlobalPosition + new Vector2(300, 900);
        rewardsLabel.GlobalPosition = cardRewardContainer.GlobalPosition + new Vector2(-40, -50);
        __instance.AddChild(rewardsLabel);
        __instance.CallDeferred(Node.MethodName.AddChild, cardRewardContainer);
        PlayerCardRewardGroupsDisplay.groupedCards = PlayerCardRewardGroupsDisplay.GetGroupedCardRewards(__instance);
        PlayerCardRewardGroupsDisplay.Populate(cardRewardContainer);
    }
}