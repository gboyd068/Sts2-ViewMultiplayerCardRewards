// A basic example of adding an image+label to all cards.
// For actual implementation you would be recommended to use a predefined scene instead.

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using ViewMultiplayerRewards.ViewMultiplayerRewardsCode;


[HarmonyPatch]
internal static class CardRewardViewerPatch
{
    [HarmonyPatch(typeof(NMultiplayerPlayerExpandedState), "_Ready")]
    [HarmonyPostfix]
    private static void AfterReady(NMultiplayerPlayerExpandedState __instance)
    {
        Control cardContainerAbove = Traverse.Create(__instance).Field("_cardContainer").GetValue<Control>();
        MegaRichTextLabel label = Traverse.Create(__instance).Field("_cardsHeader").GetValue<MegaRichTextLabel>();
        
        MainFile.Logger.LogMessage(LogLevel.Info, label.Text, 0);
        var cardRewardContainer = new HBoxContainer();
        cardRewardContainer.Size = new Vector2(600, 600);
        cardRewardContainer.GlobalPosition = cardContainerAbove.GlobalPosition + new Vector2(300, 900);
        //var rewardsLabel = new RichTextLabel() {Text="[gold]Current Card Rewards[/gold]", BbcodeEnabled = true, Size = new Vector2(200, 200)};
        //cardRewardContainer.AddChild(rewardsLabel);
        __instance.CallDeferred(Node.MethodName.AddChild, cardRewardContainer);
        PlayerCardRewardGroupsDisplay.groupedCards = PlayerCardRewardGroupsDisplay.GetGroupedCardRewards(__instance);
        PlayerCardRewardGroupsDisplay.Populate(cardRewardContainer);
    }
}
