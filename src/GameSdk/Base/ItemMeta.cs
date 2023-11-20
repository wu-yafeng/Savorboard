using GameSdk.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Base
{
    [GenerateSerializer]
    public record ItemMeta(int Id, ItemMeta.FeatureType Feature, int FeatureParam1, int FeatureParam2) : IMetaItem
    {
        public enum FeatureType
        {
            None = 0,
            AddBackpackExp = 1,
        }
    }
}
