using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;


[HarmonyPatch]
internal static class CardRewardViewerPatch
{
    
    private static List<List<CardModel>> GroupedCards;
    
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
        rewardsLabel.Hide();
        __instance.AddChild(rewardsLabel);
        __instance.CallDeferred(Node.MethodName.AddChild, cardRewardContainer);
        GroupedCards = GetGroupedCardRewards(__instance);
        Populate(cardRewardContainer, rewardsLabel);
    }

    private static void Populate(Control node, MegaRichTextLabel label)
    {
        foreach (var group in GroupedCards)
        {
            if (group.Count == 0)
                continue;
            label.Show();
            GridContainer groupContainer = new GridContainer();
            if (group.Count > 5)
                groupContainer.Columns = 2;
            node.AddChild(groupContainer);
            foreach (var card in group)
            {
                var entry = NDeckHistoryEntry.Create(card, 1);
                groupContainer.CallDeferred(Node.MethodName.AddChild, entry);
                long num = (long) entry.Connect(NDeckHistoryEntry.SignalName.Clicked, Callable.From<NDeckHistoryEntry>(new Action<NDeckHistoryEntry>(ShowEntry)));
            }
        }
    }

    private static void ShowEntry(NDeckHistoryEntry entry)
    {
        var allCards = GroupedCards
            .SelectMany(group => group)
            .ToList();
        NGame.Instance?.GetInspectCardScreen().Open(allCards, allCards.IndexOf(entry.Card));
    }
    
    private static List<List<CardModel>> GetGroupedCardRewards(
        NMultiplayerPlayerExpandedState state)
    {
        ulong netId = Traverse.Create(state).Field("_player").GetValue<Player>().NetId;
        if (CardRewardsMap.Instance.Map.ContainsKey(netId))
        {
            return CardRewardsMap.Instance.Map[netId];
        } 
        return [];
    }
}