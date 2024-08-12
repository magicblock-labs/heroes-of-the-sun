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
    pub deterioration: u8, //0 is ok, 127 will destroy
    pub id: crate::config::BuildingType,
    pub level: u8,
    pub timestamp: u16,
}

#[component]
pub struct Settlement {
    #[max_len(12, 5)]
    pub buildings: Vec<Building>,

    pub environment: ResourceBalance,
    pub treasury: ResourceBalance,

    pub day: u16,
    pub faith: u8, //0..127

    #[max_len(12, 1)]
    pub labour_allocation: Vec<i8>, //index is labour unit index, value is building index from /buildings/ array; singed: use -1 for free slot
}

impl Default for Settlement {
    fn default() -> Self {
        Self::new(SettlementInit {
            buildings: vec![Building {
                x: 10,
                y: 10,
                deterioration: 0,
                id: BuildingType::TownHall,
                level: 1,
                timestamp: 0,
            }],
            labour_allocation: vec![-1], //one labour comes as default from town hall
            environment: ResourceBalance {
                water: 200,
                food: 40,
                wood: 1000,
            },
            treasury: ResourceBalance {
                water: 20,
                food: 40,
                wood: 100,
            },
            day: 0,
            faith: 50,
        })
    }
}
