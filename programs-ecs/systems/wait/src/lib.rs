use bolt_lang::*;

declare_id!("ECfKKquvf7PWgvCTAQiYkbDGVjaxhqAN4DFZCAjTUpwx");

fn get_level_multiplier(level: u8) -> u16 {
    return (2 as u16).pow(level as u32);
}

#[system]
pub mod wait {

    use std::u16;

    use settlement::{
        Settlement,
        {self, config::BuildingType, config::ENVIRONMENT_LIMITS},
    };

    pub fn execute(ctx: Context<Components>, args: WaitArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.day += args.time;

        let mut water_storage: u16 = 0;
        let mut food_storage: u16 = 0;
        let mut wood_storage: u16 = 0;

        //todo move all rates to config

        //calc current storage capacity for all resources
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

        //wells generate water without labour asigned
        for building in settlement.buildings.to_vec() {
            match building.id {
                BuildingType::WaterCollector => {
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
                _ => {}
            }
        }

        //process all buildings with allocated labour
        let labour_allocation = settlement.labour_allocation.to_vec();
        let mut alive_labour: u16 = 0;
        for building_index in labour_allocation {
            if building_index > -1 {
                alive_labour += 1;
            }

            if building_index < 0 {
                //labour unallocated
                continue;
            }

            if settlement.buildings[building_index as usize].deterioration == u8::MAX {
                //allocated building broken
                continue;
            }

            let mut building = settlement.buildings[building_index as usize];

            if (building.days_to_build > 0) {
                building.days_to_build -= u8::min(args.time as u8, building.days_to_build);
            }

            let building_type = building.id;

            match building_type {
                BuildingType::FoodCollector => {
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
                BuildingType::WoodCollector => {
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
            ENVIRONMENT_LIMITS.water - settlement.environment.water,
        );
        settlement.environment.food += u16::min(
            args.time,
            ENVIRONMENT_LIMITS.water - settlement.environment.food,
        );
        settlement.environment.wood += u16::min(
            args.time,
            ENVIRONMENT_LIMITS.water - settlement.environment.wood,
        );

        //deteriorate buildings
        for building in &mut settlement.buildings {
            if building.deterioration < u8::MAX {
                building.deterioration += args.time as u8;
            }
        }

        //eat/drink
        if settlement.treasury.food == 0 {
            settlement.faith -= u8::min(settlement.faith, args.time as u8);
        }

        if settlement.treasury.water < alive_labour {
            //kill one
            for i in 0..settlement.labour_allocation.len() {
                if (settlement.labour_allocation[i]) > -1 {
                    settlement.labour_allocation[i] = -10;

                    alive_labour -= 1;
                    break;
                }
            }
        }

        settlement.treasury.water -= u16::min(settlement.treasury.water, args.time * alive_labour);

        settlement.treasury.food -= u16::min(settlement.treasury.water, args.time * alive_labour);

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
