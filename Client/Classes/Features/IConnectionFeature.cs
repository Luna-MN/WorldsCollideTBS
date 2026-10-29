using System.Collections.Generic;

namespace Packets.Classes.Features;

public interface IConnectionFeature : IFeature
{
    public List<CornerInfo> tiles { get; }
}