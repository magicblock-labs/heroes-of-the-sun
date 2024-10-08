mod errors;

use bolt_lang::*;
use settlement::{config::BuildingType, Settlement};

declare_id!("J3evfUppPdgjTzWhhAhuhKBVM23UU8iCU9j9r7sTHCTB");

#[system]
pub mod upgrade {
    use settlement::config::{get_construction_cost, get_extraction_cap, BUILDINGS_CONFIG};

    pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        let index = args.index;

        if index >= settlement.buildings.len() {
            return err!(errors::UpgradeError::BuildingIndexOutOfRange);
        }

        let building = settlement.buildings[index];
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
            || settlement.treasury.gold < build_cost.gold
        {
            return err!(errors::UpgradeError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= build_cost.wood;
            settlement.treasury.water -= build_cost.water;
            settlement.treasury.food -= build_cost.food;
            settlement.treasury.stone -= build_cost.stone;
            settlement.treasury.gold -= build_cost.gold;
        }

        //all checks passed
        settlement.buildings[index].level += 1;
        settlement.buildings[index].extraction += get_extraction_cap(building.level);

        if matches!(building.id, BuildingType::TownHall) {
            settlement.worker_assignment.push(-1);
        }
        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        index: usize,
    }
}
