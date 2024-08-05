mod errors;

use bolt_lang::*;
use settlement::{config::BuildingType, Settlement};

declare_id!("FrTthTtkfEWa2zZt4YEHGbL9Hz8hpsSW1hsHHnJXPRd4");

#[system]
pub mod upgrade {
    use settlement::config::BUILDINGS_CONFIG;

    pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if args.index as usize >= settlement.buildings.len() {
            return err!(errors::BuildError::BuildingIndexOutOfRange);
        }

        let building = settlement.buildings[args.index as usize];
        let building_config = &BUILDINGS_CONFIG[building.id as usize];

        //todo multiply cost by level??
        if settlement.treasury.wood < building_config.cost as u16 {
            return err!(errors::BuildError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= building_config.cost as u16;
        }

        //all checks passed
        settlement.buildings[args.index as usize].level += 1;

        if matches!(building.id, BuildingType::TownHall) {
            settlement.labour_allocation.push(-1);
        }
        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        index: u8,
    }
}
