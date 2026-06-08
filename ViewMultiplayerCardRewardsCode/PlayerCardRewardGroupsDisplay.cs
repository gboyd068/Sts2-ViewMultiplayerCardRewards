using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

public static class PlayerCardRewardGroupsDisplay
{
    public static List<List<CardModel>> groupedCards;

    public static void Populate(Control node, MegaRichTextLabel label)
    {
        foreach (var group in groupedCards)
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
        var allCards = groupedCards
            .SelectMany(group => group)
            .ToList();
        NGame.Instance?.GetInspectCardScreen().Open(allCards, allCards.IndexOf(entry.Card));
    }
    
    public static List<List<CardModel>> GetGroupedCardRewards(
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