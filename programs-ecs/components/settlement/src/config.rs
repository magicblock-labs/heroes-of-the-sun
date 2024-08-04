use bolt_lang::*;

use crate::Balance;

pub struct BuildingConfig {
    pub width: u8,
    pub height: u8,
    pub cost: [Balance; 1],
}

pub const MAP_WIDTH: u8 = 20;
pub const MAP_HEIGHT: u8 = 20;
pub const PERFECT_STATE: u8 = 4;

//this enum matches the BUILDINGS_CONFIG array indexes
#[repr(u8)]
#[component_deserialize]
pub enum BuildingType {
    TownHall = 0,
    WaterCollector = 1,
    FoodCollector = 2,
    WoodCollector = 3,
    WaterStorage = 4,
    FoodStorage = 5,
    WoodStorage = 6,
    Altar = 7,
}

pub const BUILDINGS_CONFIG: [BuildingConfig; 8] = [
    BuildingConfig {
        width: 4,
        height: 4,
        cost: [Balance {
            resource_type: 0,
            amount: 80,
        }],
    },
    BuildingConfig {
        width: 1,
        height: 1,
        cost: [Balance {
            resource_type: 0,
            amount: 30,
        }],
    },
    BuildingConfig {
        width: 2,
        height: 2,
        cost: [Balance {
            resource_type: 0,
            amount: 40,
        }],
    },
    BuildingConfig {
        width: 1,
        height: 3,
        cost: [Balance {
            resource_type: 0,
            amount: 10,
        }],
    },
    BuildingConfig {
        width: 2,
        height: 2,
        cost: [Balance {
            resource_type: 0,
            amount: 40,
        }],
    },
    BuildingConfig {
        width: 2,
        height: 2,
        cost: [Balance {
            resource_type: 0,
            amount: 40,
        }],
    },
    BuildingConfig {
        width: 2,
        height: 2,
        cost: [Balance {
            resource_type: 0,
            amount: 40,
        }],
    },
    BuildingConfig {
        width: 3,
        height: 3,
        cost: [Balance {
            resource_type: 0,
            amount: 140,
        }],
    },
];
