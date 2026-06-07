

using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

class RewardsOfferedMessage : ICustomMessage
{
    public Player player;
    public List<List<CardModel>> groupedCards;
    
    public RewardsOfferedMessage() { }
        
    public RewardsOfferedMessage(Player player, IReadOnlyList<Reward> rewards)
    {
        this.ShouldBroadcast = true;
        this.player = player;
        this.groupedCards = rewards
            .OfType<CardReward>()
            .Select(r => r.Cards
                .ToList())
            .ToList();
        
    }

    public void Serialize(PacketWriter writer)
    {
        writer.Write(player.ToSerializable());
        WriteNestedList(writer, groupedCards);
    }
    
    public void Deserialize(PacketReader reader)
    {
        player = Player.FromSerializable(reader.Read<SerializablePlayer>());
        groupedCards = ReadNestedList(reader);
    }

    public void HandleMessage(ulong senderId)
    {
        CardRewardsMap.Instance.Map[senderId] = groupedCards;
    }

    public bool ShouldBroadcast { get; }
    
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
    public Dictionary<ulong, List<List<CardModel>>> Map = new Dictionary<ulong, List<List<CardModel>>>();
    
    public static CardRewardsMap Instance { get; set; } = new CardRewardsMap();
}