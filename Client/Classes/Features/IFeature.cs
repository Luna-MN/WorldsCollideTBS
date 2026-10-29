public interface IFeature
{
    public void Set(WorldInfo worldInfo, FeatureArgs args);
    public FeatureType FeatureType { get; }
    public void Generate(int seed);
    public void Destroy();
    public void Update();
}

public enum FeatureType
{
    TopTile,
    Connection
}