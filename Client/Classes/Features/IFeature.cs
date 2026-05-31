public interface IFeature
{
    public void Set(WorldInfo worldInfo, FeatureArgs args);
    public void Generate(int seed);
    public void Destroy();
    public void Update();
}