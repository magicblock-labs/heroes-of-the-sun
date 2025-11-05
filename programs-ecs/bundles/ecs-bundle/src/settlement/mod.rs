pub mod config;

use bolt_lang::*;
use config::{get_extraction_cap, BuildingType};

pub use crate::ecs_bundle::{Settlement, SettlementInit};

declare_id!("5bKBE1HgusXC5jVVjpk4CvxUM8UGnVPQyvGt7cB6Jk7W");

#[component_deserialize]
pub struct ResourceBalance {
    pub food: u16,
    pub water: u16,
    pub wood: u16,
    pub stone: u16,
}

impl Default for ResourceBalance {
    fn default() -> Self {
        ResourceBalance {
            food: 1,
            water: 1,
            wood: 1,
            stone: 1,
        }
    }
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
    pub extraction: u16, //mines only
}

impl Default for Settlement {
    fn default() -> Self {
        let clock = Clock::get();
        let mut now = 0;

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        Self::new(SettlementInit {
            owner: Pubkey::default(),
            buildings: vec![Building {
                x: 8,
                y: 8,
                deterioration: 0,
                id: BuildingType::TownHall,
                level: 1,
                turns_to_build: 0,
                extraction: get_extraction_cap(1),
            }],
            worker_assignment: vec![-1], //one worker comes as default from town hall
            treasury: config::INITIAL_TREASURY,
            environment: config::INITIAL_ENVIRONMENT,
            time_units: config::INITIAL_TIME_UNITS,
            faith: config::INITIAL_FAITH,
            last_time_claim: now,
            research: 0,
            quest_claim_status: 0,
        })
    }
}
