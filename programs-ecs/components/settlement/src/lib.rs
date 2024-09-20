pub mod config;

use bolt_lang::*;
use config::BuildingType;

declare_id!("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ");

#[component_deserialize]
pub struct ResourceBalance {
    pub food: u16,
    pub water: u16,
    pub wood: u16,
}

#[component_deserialize]
pub struct Building {
    pub x: u8,
    pub y: u8,
    pub deterioration: u8, //0 is ok
    pub id: crate::config::BuildingType,
    pub level: u8,
    pub days_to_build: u8,
}

#[component]
pub struct Settlement {
    #[max_len(12, 5)]
    pub buildings: Vec<Building>,

    pub environment: ResourceBalance,
    pub treasury: ResourceBalance,

    pub faith: u8, //0..127
    pub time_units: u16,
    pub last_time_claim: i64,
    pub research: u32,

    #[max_len(12, 1)]
    pub labour_allocation: Vec<i8>, //index is labour unit index, value is building index from /buildings/ array; singed: use -1 for free slot
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
                x: 10,
                y: 10,
                deterioration: 0,
                id: BuildingType::TownHall,
                level: 1,
                days_to_build: 0,
            }],
            labour_allocation: vec![-1], //one labour comes as default from town hall
            environment: ResourceBalance {
                water: 200,
                food: 200,
                wood: 200,
            },
            treasury: ResourceBalance {
                water: 20,
                food: 40,
                wood: 200,
            },
            time_units: 10,
            faith: 50,
            last_time_claim: now,
            research: 0,
        })
    }
}
