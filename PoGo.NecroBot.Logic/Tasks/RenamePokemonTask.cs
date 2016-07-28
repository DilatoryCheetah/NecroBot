#region using directives

using System;
using System.Globalization;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(ISession session)
        {
            await session.Inventory.RefreshCachedInventory();

            var pokemons = await session.Inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                var pokemonName = pokemon.PokemonId.ToString();
                if (pokemonName.Length > 10 - perfection.ToString(CultureInfo.InvariantCulture).Length)
                {
                    pokemonName = pokemonName.Substring(0, 10 - perfection.ToString(CultureInfo.InvariantCulture).Length);
                }
                var newNickname = $"{pokemonName}_{perfection}";

                if (pokemon.PokemonId == POGOProtos.Enums.PokemonId.Eevee)
                {
                     if (pokemon.Nickname != "Rainer")
                    {
                        // Always rename Eevee to Rainer to force an evolution to Vaporeon
                        var result = await session.Client.Inventory.NicknamePokemon(pokemon.Id, "Rainer");

                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message = $"Pokemon {pokemon.PokemonId} ({pokemon.Id}) renamed from {pokemon.Nickname} to Rainer."
                        });
                    }
                }
                else if (perfection > session.LogicSettings.KeepMinIvPercentage && newNickname != pokemon.Nickname &&
                    session.LogicSettings.RenameAboveIv)
                {
                    await session.Client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                    session.EventDispatcher.Send(new NoticeEvent
                    {
                        Message = session.Translation.GetTranslation(Common.TranslationString.PokemonRename, pokemon.PokemonId, pokemon.Id, pokemon.Nickname, newNickname)
                    });
                }
                else if (newNickname == pokemon.Nickname && !session.LogicSettings.RenameAboveIv)
                {
                    await session.Client.Inventory.NicknamePokemon(pokemon.Id, pokemon.PokemonId.ToString());

                    session.EventDispatcher.Send(new NoticeEvent
                    {
                        Message = session.Translation.GetTranslation(Common.TranslationString.PokemonRename, pokemon.PokemonId, pokemon.Id, pokemon.Nickname, pokemon.PokemonId)
                    });
                }
            }

            await session.Inventory.RefreshCachedInventory();
        }
    }
}