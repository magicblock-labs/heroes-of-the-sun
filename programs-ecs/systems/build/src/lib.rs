mod config;
mod errors;

use bolt_lang::*;
use config::BuildingConfig;
use settlement::{Balance, Settlement};

declare_id!("Fgc4uSFUPnhUpwUu7z4siYiBtnkxrwroYVQ2csDo3Q7P");

fn overlap_check(
    settlement: &mut Account<Settlement>,
    x: u8,
    y: u8,
    new_config: &BuildingConfig,
) -> bool {
    for existing_building in &settlement.buildings {
        let existing_config = &config::BUILDINGS_CONFIG[existing_building.id as usize];

        if (x < existing_building.x + existing_config.width
            || x + new_config.width >= existing_building.x)
            && (y < existing_building.y + existing_config.height
                || y + new_config.height >= existing_building.y)
        {
            return false;
        }
    }

    return true;
}

pub fn subtract_resource(settlement: &mut Account<Settlement>, cost: Vec<Balance>) -> bool {
    for cost_elem in cost {
        for elem in &mut settlement.treasury {
            if elem.resource_type == cost_elem.resource_type {
                if elem.amount >= cost_elem.amount {
                    elem.amount -= cost_elem.amount;
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
    return false;
}

#[system]
pub mod build {

    use settlement::Building;

    pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
        let new_building_config = &config::BUILDINGS_CONFIG[args.id as usize];

        //check map bounds
        if args.x + new_building_config.width >= config::MAP_WIDTH {
            return err!(errors::BuildError::OutOfBounds);
        }

        if args.y + new_building_config.height >= config::MAP_HEIGHT {
            return err!(errors::BuildError::OutOfBounds);
        }

        let settlement = &mut ctx.accounts.settlement;
        if !overlap_check(settlement, args.x, args.y, new_building_config) {
            return err!(errors::BuildError::WontFit);
        }

        if !subtract_resource(settlement, Vec::from(new_building_config.cost)) {
            return err!(errors::BuildError::NotEnoughResources);
        }

        let new_building = Building {
            x: args.x,
            y: args.y,
            id: args.id,
            state: config::PERFECT_STATE,
            level: 1,
        };

        ctx.accounts.settlement.buildings.push(new_building);
        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        id: u8,
        x: u8,
        y: u8,
    }
}
