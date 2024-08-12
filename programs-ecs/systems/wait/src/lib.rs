use bolt_lang::*;

declare_id!("ECfKKquvf7PWgvCTAQiYkbDGVjaxhqAN4DFZCAjTUpwx");

#[system]
pub mod wait {
    use core::time;
    use std::u16;

    use settlement::{
        config::{self, BuildingType},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: WaitArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.day += args.time;

        let mut water_storage: u16 = 0;
        let mut food_storage: u16 = 0;
        let mut wood_storage: u16 = 0;

        //calc current storage capacity
        for building in settlement.buildings.to_vec() {
            match building.id {
                BuildingType::WaterStorage => {
                    water_storage += 10 * (2 as u16).pow(building.level as u32);
                }
                BuildingType::FoodStorage => {
                    food_storage += 25 * (2 as u16).pow(building.level as u32);
                }
                BuildingType::WoodStorage => {
                    wood_storage += 50 * (2 as u16).pow(building.level as u32);
                }
                _ => {}
            }
        }

        msg!("water_storage {} ", water_storage);

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
                    let mut collected = 0;

                    if water_storage > settlement.treasury.water {
                        collected = u16::min(args.time, settlement.environment.water);
                        collected = u16::min(collected, water_storage - settlement.treasury.water);
                    }

                    if collected > 0 {
                        settlement.environment.water -= collected;
                        settlement.treasury.water += collected;
                    }
                }
                config::BuildingType::FoodCollector => {
                    let mut collected = 0;

                    if food_storage > settlement.treasury.food {
                        collected = u16::min(args.time, settlement.environment.food);
                        collected = u16::min(collected, food_storage - settlement.treasury.food);
                    }

                    if collected > 0 {
                        settlement.environment.food -= collected;
                        settlement.treasury.food += collected;
                    }
                }
                config::BuildingType::WoodCollector => {
                    let mut collected = 0;

                    if food_storage > settlement.treasury.wood {
                        collected = u16::min(args.time, settlement.environment.wood);
                        collected = u16::min(collected, wood_storage - settlement.treasury.wood);
                    }
                    if collected > 0 {
                        settlement.environment.wood -= collected;
                        settlement.treasury.wood += collected; //* faith +technology + env capacity */
                    }
                }
                _ => {}
            }
        }

        //regen sources in environment
        settlement.environment.water += u16::min(
            args.time,
            config::ENVIRONMENT_LIMITS.water - settlement.environment.water,
        );
        settlement.environment.food += u16::min(
            args.time,
            config::ENVIRONMENT_LIMITS.water - settlement.environment.food,
        );
        settlement.environment.wood += u16::min(
            args.time,
            config::ENVIRONMENT_LIMITS.water - settlement.environment.wood,
        );

        //deteriorate buildings
        for building in &mut settlement.buildings {
            building.deterioration += args.time as u8;
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct WaitArgs {
        time: u16,
    }
}
