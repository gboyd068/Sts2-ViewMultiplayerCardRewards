

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
    public List<CardModel> Cards;
    
    public RewardsOfferedMessage() { }
        
    public RewardsOfferedMessage(Player player, IEnumerable<CardModel> cards)
    {
        this.ShouldBroadcast = true;
        this.player = player;
        this.Cards = cards.ToList();

    }

    public void Serialize(PacketWriter writer)
    {
        writer.Write(player.ToSerializable());
        WriteCardsList(writer, Cards);
    }
    
    public void Deserialize(PacketReader reader)
    {
        player = Player.FromSerializable(reader.Read<SerializablePlayer>());
        Cards = ReadCardsList(reader);
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

        if (!IsAlreadyPresentInMap(Cards))
        {
            rewards.Add(Cards);
        }
        
    }

    public bool ShouldBroadcast { get; } = true;
    
    public static void WriteCardsList(
        PacketWriter writer,
        List<CardModel> data)
    {
        var serializable = data
            .Select(c => c.ToSerializable()).ToList();
        
        writer.WriteInt(serializable.Count);

        foreach (var item in serializable)
        {
            writer.Write(item);
        }
    }
    
    public static List<CardModel> ReadCardsList(PacketReader reader)
    {
        
        int count = reader.ReadInt();
        var group = new List<SerializableCard>(count);
        for (int j = 0; j < count; j++)
        {
            group.Add(reader.Read<SerializableCard>());
        }

        return group
            .Select(CardModel.FromSerializable)
            .ToList();
    }
}


public partial class CardRewardsMap : Node
{
    public Dictionary<ulong, List<List<CardModel>>> Map = new Dictionary<ulong, List<List<CardModel>>?>();
    
    public static CardRewardsMap Instance { get; set; } = new CardRewardsMap();
}