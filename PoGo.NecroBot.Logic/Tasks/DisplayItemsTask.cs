﻿using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class DisplayItemsTask
    {
        public static void Execute(Context ctx, StateMachine machine)
        {
            Logger.Write($"====== DisplayAllItems ======", LogLevel.Info, ConsoleColor.Yellow);
            var myItems = ctx.Inventory.GetItems().Result;

            int totalItemCount = 0;
            foreach (var item in myItems)
            {
                totalItemCount += item.Count;
                Logger.Write($"# Count {item.Count,3:##0} | {item.ItemId.ToString()}",
                    LogLevel.Info, ConsoleColor.Yellow);
            }
            int maxItemStorage = ctx.Inventory.GetMaxItemStorage().Result;
            Logger.Write($"Total number of Items in inventory: {totalItemCount,4:###0}/{maxItemStorage,4:###0}", LogLevel.Info, ConsoleColor.Yellow);
        }
    }
}
