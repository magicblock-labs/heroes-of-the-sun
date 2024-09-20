mod errors;

use bolt_lang::*;
use settlement::{
    config::{BuildingConfig, BUILDINGS_CONFIG, MAP_HEIGHT, MAP_WIDTH},
    Building, Settlement,
};

declare_id!("Fgc4uSFUPnhUpwUu7z4siYiBtnkxrwroYVQ2csDo3Q7P");

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
    use std::u8;

    use settlement::config::{get_research_level, ResearchType};

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

        let cost_research = get_research_level(settlement.research, ResearchType::BuildingCost);
        let cost =
            ((new_building_config.cost as f32) * (1.0 - 0.1 * (cost_research as f32))).ceil();

        if settlement.treasury.wood < cost as u16 {
            return err!(errors::BuildError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= cost as u16;
        }

        let build_time = new_building_config.build_time
            - u8::min(
                get_research_level(settlement.research, ResearchType::BuildingSpeed),
                new_building_config.build_time,
            );

        let new_building = Building {
            x: args.x,
            y: args.y,
            id: new_building_config.r#type,
            deterioration: 0,
            level: 1,
            days_to_build: build_time,
        };

        settlement.buildings.push(new_building);

        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct BuildArgs {
        config_index: u8,
        x: u8,
        y: u8,
    }
}
