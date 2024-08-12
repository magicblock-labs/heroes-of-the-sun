use bolt_lang::*;

declare_id!("ECfKKquvf7PWgvCTAQiYkbDGVjaxhqAN4DFZCAjTUpwx");

fn get_level_multiplier(level: u8) -> u16 {
    return (2 as u16).pow(level as u32);
}

#[system]
pub mod wait {

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

        //todo move all rates to config

        //calc current storage capacity
        for building in settlement.buildings.to_vec() {
            match building.id {
                BuildingType::WaterStorage => {
                    water_storage += 10 * get_level_multiplier(building.level);
                }
                BuildingType::FoodStorage => {
                    food_storage += 25 * get_level_multiplier(building.level);
                }
                BuildingType::WoodStorage => {
                    wood_storage += 50 * get_level_multiplier(building.level);
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

            if settlement.buildings[building_index as usize].deterioration == u8::MAX {
                //building broken unallocated
                continue;
            }

            let building = settlement.buildings[building_index as usize];
            let building_type = building.id;

            match building_type {
                config::BuildingType::WaterCollector => {
                    //todo check if this code can be reused (across 3 different resources)
                    let mut collected = 0;

                    if water_storage > settlement.treasury.water {
                        collected = u16::min(
                            args.time * get_level_multiplier(building.level),
                            settlement.environment.water,
                        );
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
                        collected = u16::min(
                            args.time * get_level_multiplier(building.level),
                            settlement.environment.food,
                        );
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
                        collected = u16::min(
                            args.time * get_level_multiplier(building.level),
                            settlement.environment.wood,
                        );
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
            if building.deterioration < u8::MAX {
                building.deterioration += args.time as u8;
            }
        }

        //eat/drink
        if settlement.treasury.water == 0 || settlement.treasury.food == 0 {
            settlement.faith -= u8::min(settlement.faith, args.time as u8);
        }

        settlement.treasury.water -= u16::min(
            settlement.environment.water,
            args.time * (settlement.labour_allocation).len() as u16,
        );

        settlement.treasury.food -= u16::min(
            settlement.environment.water,
            args.time * (settlement.labour_allocation).len() as u16,
        );

        let mut i: usize = 0;
        //restore sacrificed labour
        for building_index in settlement.labour_allocation.to_vec() {
            if building_index < -1 {
                settlement.labour_allocation[i] += args.time as i8;
                if settlement.labour_allocation[i] > -1 {
                    settlement.labour_allocation[i] = -1;
                }
            }

            i += 1;
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
