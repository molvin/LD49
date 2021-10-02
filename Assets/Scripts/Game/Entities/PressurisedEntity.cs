
public class PressurisedEnitity : Entity
{
    public ResourceType Type;
    public float Pressure;

    public override void Clear()
    {
        Type = ResourceType.None;
        Pressure = 0.0f;
    }
    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        throw new System.NotImplementedException();
    }
}