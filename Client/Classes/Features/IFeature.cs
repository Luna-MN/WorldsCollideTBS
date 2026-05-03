public interface IFeature
{
    public void Set(WorldInfo worldInfo, FeatureArgs args);
    public void Generate();
    public void Destroy();
    public void Update();
}