use bolt_lang::*;
use std::u16;

use settlement::{
    self,
    config::{
        self, get_collection_level_multiplier, get_extraction_cap, get_research_level,
        get_storage_level_multiplier, BuildingType, ResearchType, ENVIRONMENT_MAX,
    },
    Settlement,
};

declare_id!("9F6qiZPUWN3bCnr5uVBwSmEDf8QcAFHNSVDH8L7AkZe4");

#[system]
pub mod wait {

    pub fn execute(ctx: Context<Components>, args: WaitArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        let time_to_wait = u16::min(args.time, settlement.time_units as u16);
        settlement.time_units -= time_to_wait as u8;

        let mut water_storage: u16 = 0;
        let mut food_storage: u16 = 0;
        let mut wood_storage: u16 = 0;
        let mut stone_storage: u16 = 0;

        //calc current storage capacity for all resources
        for building in settlement.buildings.to_vec() {
            if building.turns_to_build > 0 {
                continue;
            }

            match building.id {
                BuildingType::TownHall => {
                    water_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
                    food_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
                }
                BuildingType::WaterStorage => {
                    water_storage += config::WATER_STORAGE_PER_LEVEL
                        * get_storage_level_multiplier(building.level);
                }
                BuildingType::FoodStorage => {
                    food_storage += config::FOOD_STORAGE_PER_LEVEL
                        * get_storage_level_multiplier(building.level);
                }
                BuildingType::WoodStorage => {
                    wood_storage += config::WOOD_STORAGE_PER_LEVEL
                        * get_storage_level_multiplier(building.level);
                }
                BuildingType::StoneStorage => {
                    stone_storage += config::STONE_STORAGE_PER_LEVEL
                        * get_storage_level_multiplier(building.level);
                }
                _ => {}
            }
        }

        let storage_research =
            get_research_level(settlement.research, ResearchType::StorageCapacity);
        let storage_multiplier =
            1.0 + config::STORAGE_CAPACITY_RESEARCH_MULTIPLIER * storage_research as f32;
        if storage_multiplier > 1.0 {
            water_storage = (water_storage as f32 * storage_multiplier).floor() as u16;
            food_storage = (food_storage as f32 * storage_multiplier).floor() as u16;
            wood_storage = (wood_storage as f32 * storage_multiplier).floor() as u16;
            stone_storage = (stone_storage as f32 * storage_multiplier).floor() as u16;
        }

        //wells generate water without worker assigned
        for building in settlement.buildings.to_vec() {
            if building.turns_to_build > 0 {
                continue;
            }
            match building.id {
                BuildingType::WaterCollector => {
                    let mut collected = 0;

                    if water_storage > settlement.treasury.water {
                        collected = time_to_wait * get_collection_level_multiplier(building.level)
                            + get_research_level(
                                settlement.research,
                                ResearchType::ResourceCollectionSpeed,
                            ) as u16;
                        collected = u16::min(collected, water_storage - settlement.treasury.water);
                    }

                    if collected > 0 {
                        settlement.treasury.water += collected;
                    }
                }
                _ => {}
            }
        }

        //process all buildings with allocated worker
        let mut alive_workers: u16 = 0;
        for worker_index in 0..settlement.worker_assignment.len() {
            let building_index = settlement.worker_assignment[worker_index];

            if building_index >= -1 {
                alive_workers += 1;
            }

            if building_index < 0 {
                //worker unallocated
                continue;
            }

            let building_index_usize = building_index as usize;
            let building = settlement.buildings[building_index_usize];

            let max_deterioration = config::BASE_DETERIORATION_CAP
                + config::DETERIORATION_CAP_RESEARCH_MULTIPLIER
                    * get_research_level(settlement.research, ResearchType::DeteriorationCap);

            if building.deterioration >= max_deterioration {
                //allocated building broken
                settlement.worker_assignment[worker_index] = -1;
                continue;
            }

            if building.turns_to_build > 0 {
                settlement.buildings[building_index_usize].turns_to_build -=
                    u8::min(time_to_wait as u8, building.turns_to_build);

                if settlement.buildings[building_index_usize].turns_to_build <= 0 {
                    settlement.buildings[building_index_usize].level += 1;

                    settlement.buildings[building_index_usize].extraction +=
                        get_extraction_cap(building.level);
                    if matches!(building.id, BuildingType::TownHall) {
                        settlement.worker_assignment.push(-1);
                    }

                    //finished building anything but food/wood collectors - release workers
                    match settlement.buildings[building_index_usize].id {
                        BuildingType::FoodCollector => {}
                        BuildingType::WoodCollector => {}
                        BuildingType::StoneCollector => {}
                        _ => settlement.worker_assignment[worker_index] = -1,
                    }
                } else {
                    continue;
                }
            }

            let building_type = building.id;

            //TODO [CLEANUP] check if this code can be reused (across 3 different resources) (e.g. using array to store resources)
            match building_type {
                BuildingType::FoodCollector => {
                    let mut collected = 0;

                    if food_storage > settlement.treasury.food {
                        collected = u16::min(
                            time_to_wait * get_collection_level_multiplier(building.level)
                                + get_research_level(
                                    settlement.research,
                                    ResearchType::ResourceCollectionSpeed,
                                ) as u16,
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

                    if wood_storage > settlement.treasury.wood {
                        collected = u16::min(
                            time_to_wait * get_collection_level_multiplier(building.level)
                                + get_research_level(
                                    settlement.research,
                                    ResearchType::ResourceCollectionSpeed,
                                ) as u16,
                            settlement.environment.wood,
                        );
                        collected = u16::min(collected, wood_storage - settlement.treasury.wood);
                    }
                    if collected > 0 {
                        settlement.environment.wood -= collected;
                        settlement.treasury.wood += collected;
                    }
                }
                BuildingType::StoneCollector => {
                    let mut collected = 0;

                    if stone_storage > settlement.treasury.stone {
                        collected = u16::min(
                            time_to_wait * get_collection_level_multiplier(building.level)
                                + get_research_level(
                                    settlement.research,
                                    ResearchType::ResourceCollectionSpeed,
                                ) as u16,
                            building.extraction,
                        );
                        collected = u16::min(collected, stone_storage - settlement.treasury.stone);
                    }
                    if collected > 0 {
                        settlement.buildings[building_index_usize].extraction -= collected;
                        settlement.treasury.stone += collected;
                    }
                }

                _ => {}
            }
        }

        let regeneration_rate = config::get_regeneration_rate(settlement.research);

        //regeneration sources in environment
        settlement.environment.food += u16::min(
            time_to_wait * regeneration_rate,
            ENVIRONMENT_MAX.food - settlement.environment.food,
        );
        settlement.environment.wood += u16::min(
            time_to_wait * regeneration_rate,
            ENVIRONMENT_MAX.wood - settlement.environment.wood,
        );

        //deteriorate buildings
        for building in &mut settlement.buildings {
            if building.turns_to_build == 0 && building.deterioration < u8::MAX {
                building.deterioration += time_to_wait as u8;
            }
        }

        if settlement.treasury.water < alive_workers || settlement.treasury.food < alive_workers {
            //kill one
            for i in 0..settlement.worker_assignment.len() {
                if (settlement.worker_assignment[i]) >= -1 {
                    settlement.worker_assignment[i] = config::BASE_DEATH_TIMEOUT
                        + (config::DEATH_TIMEOUT_RESEARCH_MULTIPLIER
                            * get_research_level(settlement.research, ResearchType::DeathTimeout))
                            as i8;

                    alive_workers -= 1;
                    break;
                }
            }
        }

        let consumption_rate: u16 = (alive_workers
            - u16::min(
                alive_workers,
                get_research_level(settlement.research, ResearchType::Consumption) as u16,
            ))
            * time_to_wait;

        settlement.treasury.water -= u16::min(settlement.treasury.water, consumption_rate);
        settlement.treasury.food -= u16::min(settlement.treasury.food, consumption_rate);

        let mut i: usize = 0;

        //restore sacrificed worker
        for building_index in settlement.worker_assignment.to_vec() {
            if building_index < -1 {
                settlement.worker_assignment[i] += time_to_wait as i8;
                if settlement.worker_assignment[i] > -1 {
                    settlement.worker_assignment[i] = -1;
                }
            }

            i += 1;
        }

        //calc faith as a lerp to 'runway'
        let mut runway = 0;
        if alive_workers > 0 {
            runway = u16::min(settlement.treasury.food, settlement.treasury.water)
                / alive_workers as u16;
        }

        msg!("runway {}", runway);

        if settlement.faith >= config::FAITH_TO_RUNWAY_LERP_PER_TURN
            && runway < settlement.faith as u16
        {
            settlement.faith -= config::FAITH_TO_RUNWAY_LERP_PER_TURN;
        } else if settlement.faith < u8::MAX - config::FAITH_TO_RUNWAY_LERP_PER_TURN
            && runway > settlement.faith as u16
        {
            settlement.faith += config::FAITH_TO_RUNWAY_LERP_PER_TURN;
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
