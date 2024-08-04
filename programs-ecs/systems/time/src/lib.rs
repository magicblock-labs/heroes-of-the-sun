use bolt_lang::*;
use settlement::config;
use settlement::Settlement;

declare_id!("AyMpNWt8uw4nEHReXXEUhbDmgmQQbfG4HZjCSkupbEne");

#[system]
pub mod time {

    pub fn execute(ctx: Context<Components>, args: TimeArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.day += args.days;

        //process all buildings with allocated labour
        let labour_allocation = settlement.labour_allocation.to_vec();
        for building_index in labour_allocation {
            if building_index < 0 {
                continue;
            }
            let building_type = settlement.buildings[building_index as usize].id;

            match building_type {
                config::BuildingType::WoodCollector => {
                    settlement.treasury[0].amount += 10;
                }
                config::BuildingType::WaterCollector => {
                    settlement.treasury[1].amount += 10;
                }
                config::BuildingType::FoodCollector => {
                    settlement.treasury[2].amount += 10;
                }
                _ => {}
            }
        }

        //regen sources

        //deteriorate buildings

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
