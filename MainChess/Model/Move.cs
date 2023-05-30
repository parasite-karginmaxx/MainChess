namespace MainChess.Model;

public readonly struct Move
{
    public Position Source { get; init; }
    public Position Destination { get; init; }
    public Type? PromotionType { get; init; }
    public Move(Position source, Position destination, Type? promotionType = null)
    {
        Source = source;
        Destination = destination;
        PromotionType = promotionType;
    }
    public override string ToString() => Source.ToString() + " - " + Destination.ToString();
}