#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchNearbyPokemonsTask
    {
        public static async Task Execute(ISession session)
        {
            Logger.Write(session.Translation.GetTranslation(Common.TranslationString.LookingForPokemon), LogLevel.Debug);

            var pokemons = await GetNearbyPokemons(session);
            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);

            foreach (var pokemon in pokemons)
            {
                var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 3000 : 500);

                var encounter = await session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (session.LogicSettings.WalkingSpeedInKilometerPerHour >= 1000)
                {
                    if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                    {
                        var maxCp = PokemonInfo.CalculateMaxCp(encounter.WildPokemon.PokemonData);
                        var perfection = PokemonInfo.CalculatePokemonPerfection(encounter.WildPokemon.PokemonData);
                        var level = PokemonInfo.GetLevel(encounter.WildPokemon.PokemonData);

                        Logger.Write(
                            $"# CP {encounter.WildPokemon.PokemonData.Cp,4:###0}/{maxCp,4:###0} | " +
                            $"IV: {perfection,6:##0.00}% [{encounter.WildPokemon.PokemonData.IndividualAttack,2:#0}/{encounter.WildPokemon.PokemonData.IndividualDefense,2:#0}/{encounter.WildPokemon.PokemonData.IndividualStamina,2:#0}] | " + 
                            $"Lvl {level,2:#0} | Name: {encounter.WildPokemon.PokemonData.PokemonId.ToString().PadRight(13, ' ')} | " + 
                            $"Location: {encounter.WildPokemon.Latitude}, {encounter.WildPokemon.Longitude}",
                            LogLevel.Info, System.ConsoleColor.DarkYellow);
                        continue;
                    }
                }

                var normalBallsCount = await session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemPokeBall);
                var greatBallsCount = await session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemGreatBall);
                var ultraBallsCount = await session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemUltraBall);
                var masterBallsCount = await session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemMasterBall);

                if (normalBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount == 0)
                    return;
                    
                if (session.LogicSettings.UsePokemonToNotCatchFilter &&
                    session.LogicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId) &&
                    pokeBallsCount < 50)
                {
                    Logger.Write(session.Translation.GetTranslation(Common.TranslationString.PokemonSkipped, pokemon.PokemonId));
                    continue;
                }

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    await CatchPokemonTask.Execute(session, encounter, pokemon);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (session.LogicSettings.TransferDuplicatePokemon)
                    {
                        session.EventDispatcher.Send(new WarnEvent {Message = session.Translation.GetTranslation(Common.TranslationString.InvFullTransferring)});
                        await TransferDuplicatePokemonTask.Execute(session);
                    }
                    else
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(Common.TranslationString.InvFullTransferManually)
                        });
                }
                else
                {
                    session.EventDispatcher.Send(new WarnEvent {Message = session.Translation.GetTranslation(Common.TranslationString.EncounterProblem, encounter.Status)});
                }

                // If pokemon is not last pokemon in list, create delay between catches, else keep moving.
                if (!Equals(pokemons.ElementAtOrDefault(pokemons.Count() - 1), pokemon))
                {
                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch);
                }
            }
        }

        private static async Task<IOrderedEnumerable<MapPokemon>> GetNearbyPokemons(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons)
                .OrderBy(
                    i =>
                        LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude, session.Client.CurrentLongitude,
                            i.Latitude, i.Longitude));

            return pokemons;
        }
    }
}
