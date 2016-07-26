﻿#region using directives

using System.Linq;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferDuplicatePokemonTask
    {
        public static void Execute(Context ctx, StateMachine machine)
        {
            var duplicatePokemons =
                ctx.Inventory.GetDuplicatePokemonToTransfer(ctx.LogicSettings.KeepPokemonsThatCanEvolve, ctx.LogicSettings.PrioritizeIvOverCp,
                    ctx.LogicSettings.PokemonsNotToTransfer).Result;

            var pokemonSettings = ctx.Inventory.GetPokemonSettings().Result;
            var pokemonFamilies = ctx.Inventory.GetPokemonFamilies().Result;

            int pokemonCount = 0;
            POGOProtos.Enums.PokemonId pokemonId = POGOProtos.Enums.PokemonId.Missingno;
            foreach (var duplicatePokemon in duplicatePokemons)
            {
                if (duplicatePokemon.PokemonId != pokemonId)
                {
                    pokemonId = duplicatePokemon.PokemonId;
                    pokemonCount = 1;
                }
                else
                {
                    pokemonCount++;
                }
                if ((pokemonCount < (ctx.LogicSettings.KeepMaxDuplicatePokemon - ctx.LogicSettings.KeepMinDuplicatePokemon)) && 
                        (duplicatePokemon.Cp >= ctx.LogicSettings.KeepMinCp || 
                         PokemonInfo.CalculatePokemonPerfection(duplicatePokemon) > ctx.LogicSettings.KeepMinIvPercentage))
                {
                    continue;
                }

                ctx.Client.Inventory.TransferPokemon(duplicatePokemon.Id).Wait();
                ctx.Inventory.DeletePokemonFromInvById(duplicatePokemon.Id);

                var bestPokemonOfType = ctx.LogicSettings.PrioritizeIvOverCp
                    ? ctx.Inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon).Result
                    : ctx.Inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon).Result;

                if (bestPokemonOfType == null)
                    bestPokemonOfType = duplicatePokemon;

                var setting = pokemonSettings.Single(q => q.PokemonId == duplicatePokemon.PokemonId);
                var family = pokemonFamilies.Single(q => q.FamilyId == setting.FamilyId);

                family.Candy++;

                machine.Fire(new TransferPokemonEvent
                {
                    Id = duplicatePokemon.PokemonId,
                    Perfection = PokemonInfo.CalculatePokemonPerfection(duplicatePokemon),
                    Cp = duplicatePokemon.Cp,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    FamilyCandies = family.Candy
                });
            }
        }
    }
}