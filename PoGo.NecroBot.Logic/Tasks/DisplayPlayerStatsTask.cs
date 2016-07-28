using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class DisplayPlayerStatsTask
    {
        public static async Task Execute(ISession session)
        {
            Logger.Write($"====== DisplayPlayerStats ======", LogLevel.Info, ConsoleColor.Yellow);
            var myPlayerStats = await session.Inventory.GetPlayerStats();

            foreach (var playerStat in myPlayerStats)
            {
                var properties = playerStat.GetType().GetProperties();
                foreach (var property in properties)
                {
                    Logger.Write($"{property.Name}: {property.GetValue(playerStat, null)}", LogLevel.Info, ConsoleColor.Yellow);
                }
            }
        }
    }
}
