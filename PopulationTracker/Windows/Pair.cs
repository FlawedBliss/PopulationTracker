namespace PopulationTracker.Windows;

public record Pair<TK, TV>
{
    public required TK Key { get; init; }
    public required TV Value { get; init; }

}
