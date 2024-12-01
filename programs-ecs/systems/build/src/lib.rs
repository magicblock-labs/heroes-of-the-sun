mod errors;

use bolt_lang::*;
use settlement::{
    config::{
        get_build_time, get_construction_cost, get_extraction_cap, BuildingConfig,
        BUILDINGS_CONFIG, MAP_HEIGHT, MAP_WIDTH,
    },
    Building, Settlement,
};
use std::u8;

declare_id!("fkiWK1Wn6ouGcHb3icX4XGKynef5MpsTQ478ZMdgB1g");

//move to settlement trait?
fn fits(settlement: &mut Account<Settlement>, x: u8, y: u8, new_config: &BuildingConfig) -> bool {
    for existing_building in &settlement.buildings {
        let existing_config = &BUILDINGS_CONFIG[existing_building.id as usize];

        if x < existing_building.x + existing_config.width
            && existing_building.x < x + new_config.width
            && y < existing_building.y + existing_config.height
            && existing_building.y < y + new_config.height
        {
            msg!("collided!");
            msg!(
                "new: x {}, y {}, w {}, h {}",
                x,
                y,
                new_config.width,
                new_config.height
            );

            msg!(
                "existing: x {}, y {}, w {}, h {}",
                existing_building.x,
                existing_building.y,
                existing_config.width,
                existing_config.height
            );

            return false;
        }
    }

    return true;
}

#[system]
pub mod build {

    pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
        if args.config_index as usize >= BUILDINGS_CONFIG.len() {
            return err!(errors::BuildError::ConfigIndexOutOfRange);
        }

        let new_building_config = &BUILDINGS_CONFIG[args.config_index as usize];

        //check map bounds
        if args.x + new_building_config.width >= MAP_WIDTH {
            return err!(errors::BuildError::OutOfBounds);
        }

        if args.y + new_building_config.height >= MAP_HEIGHT {
            return err!(errors::BuildError::OutOfBounds);
        }

        let settlement = &mut ctx.accounts.settlement;
        if !fits(settlement, args.x, args.y, new_building_config) {
            return err!(errors::BuildError::WontFit);
        }

        let build_cost =
            get_construction_cost(settlement.research, new_building_config.cost_tier, 1, 1.0);

        if settlement.treasury.wood < build_cost.wood
            || settlement.treasury.water < build_cost.water
            || settlement.treasury.food < build_cost.food
            || settlement.treasury.stone < build_cost.stone
        {
            return err!(errors::BuildError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= build_cost.wood;
            settlement.treasury.water -= build_cost.water;
            settlement.treasury.food -= build_cost.food;
            settlement.treasury.stone -= build_cost.stone;
        }

        let new_building = Building {
            x: args.x,
            y: args.y,
            id: new_building_config.id,
            deterioration: 0,
            level: 0,
            turns_to_build: get_build_time(
                settlement.research,
                new_building_config.build_time_tier,
                1,
            ),
            extraction: get_extraction_cap(0),
        };

        settlement.buildings.push(new_building);

        if args.worker_index >= 0 {
            if settlement.worker_assignment.len() as i16 <= args.worker_index {
                return err!(errors::BuildError::SuppliedWorkerIndexOutOfBounds);
            }
            settlement.worker_assignment[args.worker_index as usize] =
                (settlement.buildings.len() - 1) as i8;
        }

        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        config_index: u8,
        worker_index: i16,
        x: u8,
        y: u8,
    }
}
