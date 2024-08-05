use bolt_lang::*;
use settlement::config;
use settlement::Settlement;

declare_id!("AyMpNWt8uw4nEHReXXEUhbDmgmQQbfG4HZjCSkupbEne");

#[system]
pub mod time {
    use std::u16;

    pub fn execute(ctx: Context<Components>, args: TimeArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.day += args.days;

        //process all buildings with allocated labour
        let labour_allocation = settlement.labour_allocation.to_vec();
        for building_index in labour_allocation {
            if building_index < 0 {
                //labour unallocated
                continue;
            }
            let building_type = settlement.buildings[building_index as usize].id;

            match building_type {
                config::BuildingType::WaterCollector => {
                    //check if this code can be reused?
                    let collected = u16::min(args.days, settlement.environment.water);

                    if collected > 0 {
                        settlement.environment.water += collected;
                        settlement.treasury.water += collected;
                    }
                }
                config::BuildingType::FoodCollector => {
                    let collected = u16::min(args.days, settlement.environment.food);

                    if collected > 0 {
                        settlement.environment.food += collected;
                        settlement.treasury.food += collected;
                    }
                }
                config::BuildingType::WoodCollector => {
                    let collected = u16::min(args.days, settlement.environment.wood);

                    if collected > 0 {
                        settlement.environment.wood -= collected;
                        settlement.treasury.wood += collected; //* faith +technology + env capacity */
                    }
                }
                _ => {}
            }
        }

        //regen sources

        //deteriorate buildings
        for building in &mut settlement.buildings {
            building.deterioration += args.days as u8;
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct TimeArgs {
        days: u16,
    }
}
