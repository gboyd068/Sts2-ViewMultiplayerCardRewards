

using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

class RewardsOfferedMessage : ICustomMessage
{
    public Player player;
    public IEnumerable<CardModel> Cards;
    
    public RewardsOfferedMessage() { }
        
    public RewardsOfferedMessage(Player player, IEnumerable<CardModel> cards)
    {
        this.ShouldBroadcast = true;
        this.player = player;
        this.Cards = cards;

    }

    public void Serialize(PacketWriter writer)
    {
        writer.Write(player.ToSerializable());
        WriteNestedList(writer, [Cards.ToList()]);
    }
    
    public void Deserialize(PacketReader reader)
    {
        player = Player.FromSerializable(reader.Read<SerializablePlayer>());
        var groupedCards = ReadNestedList(reader);
        Cards = groupedCards.FirstOrDefault([]);
    }

    private bool IsAlreadyPresentInMap(List<CardModel> cards)
    {
        if (CardRewardsMap.Instance.Map.TryGetValue(player.NetId, out List<List<CardModel>> mapEntry))
        {
            var titlesCollections = mapEntry.Select(cs => cs.Select(c => c.Title));
            var cardTitles = cards.Select(c => c.Title);
            if (titlesCollections.Any(titles => titles.SequenceEqual(cardTitles)))
            {
                return true;
            }
        }
        return false;
    }
    
    public void HandleMessage(ulong senderId)
    {
        if (!CardRewardsMap.Instance.Map.TryGetValue(player.NetId, out var rewards))
        {
            rewards = new List<List<CardModel>>();
            CardRewardsMap.Instance.Map[player.NetId] = rewards;
        }

        if (!IsAlreadyPresentInMap(Cards.ToList()))
        {
            rewards.Add(Cards.ToList());
        }
        
    }

    public bool ShouldBroadcast { get; } = true;
    
    public static void WriteNestedList(
        PacketWriter writer,
        List<List<CardModel>> data)
    {
        var serializable = data
            .Select(g => g.Select(c => c.ToSerializable()).ToList())
            .ToList();
        
        writer.WriteInt(data.Count);

        foreach (var group in serializable)
        {
            writer.WriteInt(group.Count);

            foreach (var item in group)
                writer.Write(item);
        }
    }
    
    public static List<List<CardModel>> ReadNestedList(PacketReader reader)
    {
        int outer = reader.ReadInt();
        var result = new List<List<SerializableCard>>(outer);

        for (int i = 0; i < outer; i++)
        {
            int inner = reader.ReadInt();
            var group = new List<SerializableCard>(inner);

            for (int j = 0; j < inner; j++)
            {
                group.Add(reader.Read<SerializableCard>());
            }

            result.Add(group);
        }
        
        return result
            .Select(group =>
                group
                    .Select(CardModel.FromSerializable)
                    .ToList()
            )
            .ToList();
    }
}


public partial class CardRewardsMap : Node
{
    public Dictionary<ulong, List<List<CardModel>>> Map = new Dictionary<ulong, List<List<CardModel>>?>();
    
    public static CardRewardsMap Instance { get; set; } = new CardRewardsMap();
}