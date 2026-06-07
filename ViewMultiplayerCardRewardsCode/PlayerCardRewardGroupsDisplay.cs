using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using ViewMultiplayerRewards.ViewMultiplayerRewardsCode;

public static class PlayerCardRewardGroupsDisplay
{
    public static List<List<CardModel>> groupedCards;

    public static void Populate(Control node)
    {
        foreach (var group in groupedCards)
        {
            if (group.Count == 0)
                continue;
            VBoxContainer groupContainer = new VBoxContainer();
            node.AddChild(groupContainer);
            foreach (var card in group)
            {
                var entry = NDeckHistoryEntry.Create(card, 1);
                groupContainer.CallDeferred(Node.MethodName.AddChild, entry);
                MainFile.Logger.LogMessage(LogLevel.Info, card.Title,0);
                long num = (long) entry.Connect(NDeckHistoryEntry.SignalName.Clicked, Callable.From<NDeckHistoryEntry>(new Action<NDeckHistoryEntry>(ShowEntry)));
            }
        }
    }

    private static void ShowEntry(NDeckHistoryEntry entry)
    {
        var allCards = groupedCards
            .SelectMany(group => group)
            .ToList();
        NGame.Instance.GetInspectCardScreen().Open(allCards, allCards.IndexOf(entry.Card));
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

    public static void UpdateNavigation(Control container)
    {
        for (int index = 0; index < container.GetChildCount(); ++index)
        {
            NDeckHistoryEntry child = container.GetChild<NDeckHistoryEntry>(index);
            child.FocusNeighborLeft = index > 0 ? container.GetChild<NDeckHistoryEntry>(index - 1).GetPath() : container.GetChild<NDeckHistoryEntry>(index).GetPath();
            child.FocusNeighborRight = index < container.GetChildCount() - 1 ? container.GetChild<NDeckHistoryEntry>(index + 1).GetPath() : container.GetChild<NDeckHistoryEntry>(index).GetPath();
        }
    }
}