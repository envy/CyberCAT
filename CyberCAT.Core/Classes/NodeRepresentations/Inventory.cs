﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberCAT.Core.Classes.NodeRepresentations
{
    [JsonObject]
    public class Inventory
    {
        [JsonObject]
        public class SubInventory
        {
            public ulong InventoryId { get; set; }
            public uint NumberOfItems { get; set; }
            public ItemData.NextItemEntry[] ItemHeaders { get; set; }
            public ItemData[] Items { get; set; }

            public override string ToString()
            {
                return $"[{InventoryId:X}] {NumberOfItems} items";
            }
        }
        public uint NumberOfInventories { get; set; }

        public SubInventory[] SubInventories { get; set; }
    }
}
