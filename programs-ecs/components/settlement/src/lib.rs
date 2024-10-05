pub mod config;

use bolt_lang::*;
use config::{get_extraction_cap, BuildingType};

declare_id!("B2h45ZJwpiuD9jBY7Dfjky7AmEzdzGsty4qWQxjX9ycv");

#[component_deserialize]
pub struct ResourceBalance {
    pub food: u16,
    pub water: u16,
    pub wood: u16,
    pub stone: u16,
    pub gold: u16,
}

#[component_deserialize]
pub struct EnvironmentState {
    pub food: u16,
    pub wood: u16,
}

#[component_deserialize]
pub struct Building {
    pub x: u8,
    pub y: u8,
    pub deterioration: u8, //0 is ok
    pub id: BuildingType,
    pub level: u8,
    pub turns_to_build: u8,
}

#[component]
pub struct Settlement {
    #[max_len(20, 6)]
    pub buildings: Vec<Building>,

    pub environment: EnvironmentState,
    pub treasury: ResourceBalance,

    //TODO [CLEANUP] review if it makes sense to be part of Building struct
    #[max_len(20, 2)]
    pub extraction: Vec<u16>,

    pub faith: u8,
    pub time_units: u8,
    pub last_time_claim: i64,
    pub research: u32,

    #[max_len(30, 1)]
    pub worker_assignment: Vec<i8>, //index is worker unit index, value is building index from /buildings/ array; singed: use -1 for free slot
}

impl Default for Settlement {
    fn default() -> Self {
        let clock = Clock::get();
        let mut now = 0;

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        Self::new(SettlementInit {
            buildings: vec![Building {
                x: 8,
                y: 8,
                deterioration: 0,
                id: BuildingType::TownHall,
                level: 1,
                turns_to_build: 0,
            }],
            worker_assignment: vec![-1], //one worker comes as default from town hall
            extraction: vec![get_extraction_cap(1)], //TODO [CLEANUP] replace this with hashmap to not store useless extraction value for in this case townhall
            treasury: config::INITIAL_TREASURY,
            environment: config::INITIAL_ENVIRONMENT,
            time_units: config::INITIAL_TIME_UNITS,
            faith: config::INITIAL_FAITH,
            last_time_claim: now,
            research: 0,
        })
    }
}
