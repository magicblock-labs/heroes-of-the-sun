mod errors;

use bolt_lang::*;
use settlement::Settlement;

declare_id!("J3evfUppPdgjTzWhhAhuhKBVM23UU8iCU9j9r7sTHCTB");

#[system]
pub mod upgrade {
    use settlement::config::{get_build_time, get_construction_cost, BUILDINGS_CONFIG};

    pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        let index = args.index;

        if index >= settlement.buildings.len() {
            return err!(errors::UpgradeError::BuildingIndexOutOfRange);
        }

        let building = settlement.buildings[index];

        if building.turns_to_build > 0 {
            return err!(errors::UpgradeError::UnderConstruction);
        }

        if index > 0 && building.level >= settlement.buildings[0].level {
            return err!(errors::UpgradeError::TownHallLevelReached);
        }

        let building_config = &BUILDINGS_CONFIG[building.id as usize];

        let build_cost = get_construction_cost(
            settlement.research,
            building_config.cost_tier,
            building.level + 1,
            1.0,
        );

        if settlement.treasury.wood < build_cost.wood
            || settlement.treasury.water < build_cost.water
            || settlement.treasury.food < build_cost.food
            || settlement.treasury.stone < build_cost.stone
        {
            return err!(errors::UpgradeError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= build_cost.wood;
            settlement.treasury.water -= build_cost.water;
            settlement.treasury.food -= build_cost.food;
            settlement.treasury.stone -= build_cost.stone;
        }

        if args.worker_index >= 0 {
            if settlement.worker_assignment.len() as i16 <= args.worker_index {
                return err!(errors::UpgradeError::SuppliedWorkerIndexOutOfBounds);
            }
            settlement.worker_assignment[args.worker_index as usize] = (index) as i8;
        }

        //all checks passed
        settlement.buildings[index].turns_to_build += get_build_time(
            settlement.research,
            building_config.build_time_tier,
            building.level,
        );

        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        index: usize,
        worker_index: i16,
    }
}
