﻿using CyberCAT.Core.Classes.NodeRepresentations;

namespace CyberCAT.Core.Classes.Mapping.TierSystem
{
    [RealName("gameGlobalTierSaveData")]
    public class GameGlobalTierSaveData : GenericUnknownStruct.BaseClassEntry
    {
        [RealName("subtype")]
        public DumpedEnums.gameGlobalTierSubtype Subtype { get; set; }

        [RealName("data")]
        [RealType("gameSceneTierData", IsHandle = true)]
        public uint Data { get; set; }
    }
}