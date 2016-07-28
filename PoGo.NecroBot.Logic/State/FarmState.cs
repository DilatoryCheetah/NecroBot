#region using directives

using System;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Tasks;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public class FarmState : IState
    {
        public async Task<IState> Execute(ISession session)
        {
            await RenamePokemonTask.Execute(session);

            if (session.LogicSettings.EvolveAllPokemonAboveIv || session.LogicSettings.EvolveAllPokemonWithEnoughCandy)
            {
                await EvolvePokemonTask.Execute(session);
            }

            await RenamePokemonTask.Execute(session);

            if (session.LogicSettings.TransferDuplicatePokemon)
            {
                await TransferDuplicatePokemonTask.Execute(session);
            }

            await RecycleItemsTask.Execute(session);

            await DisplayPokemonStatsTask.Execute(session);

            await DisplayItemsTask.Execute(session);

            await DisplayPlayerStatsTask.Execute(session);

            if (session.LogicSettings.ExecuteFarming)
            {
                if (session.LogicSettings.UseEggIncubators)
                {
                    await UseIncubatorsTask.Execute(session);
                }

                if (session.LogicSettings.UseGpxPathing)
                {
                    await FarmPokestopsGpxTask.Execute(session);
                }
                else
                {
                    await FarmPokestopsTask.Execute(session);
                }
            }
            else
                Environment.Exit(0);


            return this;
        }
    }
}