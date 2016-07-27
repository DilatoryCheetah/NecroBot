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
    public class DisplayPokemonStatsTask
    {
        public static async Task Execute(Context ctx, StateMachine machine)
        {
            Logger.Write("====== DisplayHighestsCP ======", LogLevel.Info, ConsoleColor.Yellow);
            var highestsPokemonCp = await ctx.Inventory.GetHighestsCp(20);
            foreach (var pokemon in highestsPokemonCp)
                Logger.Write(
                    $"# CP {pokemon.Cp.ToString().PadLeft(4, ' ')}/{PokemonInfo.CalculateMaxCp(pokemon).ToString().PadLeft(4, ' ')} | ({PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}% perfect)\t| Lvl {PokemonInfo.GetLevel(pokemon).ToString("00")}\t NAME: '{pokemon.PokemonId}'",
                    LogLevel.Info, ConsoleColor.Yellow);
            Logger.Write("====== DisplayHighestsPerfect ======", LogLevel.Info, ConsoleColor.Yellow);
            var highestsPokemonPerfect = await ctx.Inventory.GetHighestsPerfect(20);
            foreach (var pokemon in highestsPokemonPerfect)
            {
                Logger.Write(
                    $"# CP {pokemon.Cp.ToString().PadLeft(4, ' ')}/{PokemonInfo.CalculateMaxCp(pokemon).ToString().PadLeft(4, ' ')} | ({PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}% perfect)\t| Lvl {PokemonInfo.GetLevel(pokemon).ToString("00")}\t NAME: '{pokemon.PokemonId}'",
                    LogLevel.Info, ConsoleColor.Yellow);
            }
            Logger.Write($"====== DisplayAllPokemon ======", LogLevel.Info, ConsoleColor.Yellow);
            var myPokemons = await ctx.Inventory.GetPokemons();
            var pokemons = myPokemons.OrderBy(x => x.PokemonId.ToString()).ThenByDescending(PokemonInfo.CalculatePokemonPerfection).ThenByDescending(x => x.Cp);

            foreach (var pokemon in pokemons)
            {
                Logger.Write($"# {pokemon.PokemonId.ToString().PadRight(15, ' ')} | Lvl {PokemonInfo.GetLevel(pokemon),2:#0} | CP {pokemon.Cp,4:###0}/{PokemonInfo.CalculateMaxCp(pokemon),4:###0} | IV {PokemonInfo.CalculatePokemonPerfection(pokemon),6:##0.00}% [{pokemon.IndividualAttack,2:#0}/{pokemon.IndividualDefense,2:#0}/{pokemon.IndividualStamina,2:#0}] | {pokemon.Nickname}",
                    LogLevel.Info, ConsoleColor.Yellow);
            }
            int maxPokemonStorage = await ctx.Inventory.GetMaxPokemonStorage();
            Logger.Write($"Total number of Pokemon in inventory: {pokemons.Count(),4:###0}/{maxPokemonStorage,4:###0}", LogLevel.Info, ConsoleColor.Yellow);
        }
    }
}
