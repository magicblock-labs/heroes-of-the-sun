use bolt_lang::*;

use crate::ResourceBalance;

//this enum matches the BUILDINGS_CONFIG array indexes (so we don't need to use a map (BuildingType=>BuildingConfig))
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

pub struct BuildingConfig {
    pub r#type: BuildingType,
    pub width: u8,
    pub height: u8,
    pub cost: u8,
    pub build_time: u8,
}

pub const MAP_WIDTH: u8 = 20;
pub const MAP_HEIGHT: u8 = 20;

pub const INITIAL_TIME_UNITS: u8 = 10;
pub const INITIAL_FAITH: u8 = 50;

pub const INITIAL_TREASURY: ResourceBalance = ResourceBalance {
    water: 20,
    food: 40,
    wood: 200,
};

pub const INITIAL_ENVIRONMENT: ResourceBalance = ResourceBalance {
    water: 200,
    food: 200,
    wood: 200,
};

pub const ENVIRONMENT_LIMITS: ResourceBalance = ResourceBalance {
    water: 1000,
    food: 1000,
    wood: 1000,
};

pub const BUILDINGS_CONFIG: [BuildingConfig; 8] = [
    BuildingConfig {
        r#type: BuildingType::TownHall,
        width: 4,
        height: 4,
        cost: 50,
        build_time: 3,
    },
    BuildingConfig {
        r#type: BuildingType::WaterCollector,
        width: 1,
        height: 1,
        cost: 10,
        build_time: 2,
    },
    BuildingConfig {
        r#type: BuildingType::FoodCollector,
        width: 2,
        height: 2,
        cost: 10,
        build_time: 1,
    },
    BuildingConfig {
        r#type: BuildingType::WoodCollector,
        width: 1,
        height: 3,
        cost: 5,
        build_time: 3,
    },
    BuildingConfig {
        r#type: BuildingType::WaterStorage,
        width: 2,
        height: 2,
        cost: 5,
        build_time: 2,
    },
    BuildingConfig {
        r#type: BuildingType::FoodStorage,
        width: 2,
        height: 2,
        cost: 8,
        build_time: 2,
    },
    BuildingConfig {
        r#type: BuildingType::WoodStorage,
        width: 2,
        height: 2,
        cost: 8,
        build_time: 4,
    },
    BuildingConfig {
        r#type: BuildingType::Altar,
        width: 3,
        height: 3,
        cost: 60,
        build_time: 5,
    },
];

pub const BASE_ENERGY_CAP: u8 = 10;
pub const ENERGY_CAP_FAITH_MULTIPLIER: f32 = 0.1;
pub const BASE_MINUTE_PER_ENERGY_UNIT: i64 = 20;

pub const ENERGY_REGEN_FAITH_MULTIPLIER: f32 = 0.1;

pub const BASE_DEATH_TIMEOUT: i8 = -10;
pub const DEATH_TIMEOUT_RESEARCH_MULTIPLIER: u8 = 1;
pub const SACRIFICE_FAITH_BOOST: u8 = 10;

pub const FAITH_TO_RUNWAY_LERP_PER_TURN: u8 = 1;

pub const BASE_DETERIORATION_CAP: u8 = 50;
pub const DETERIORATION_CAP_RESEARCH_MULTIPLIER: u8 = 5;

pub const WATER_STORAGE_PER_LEVEL: u16 = 10;
pub const FOOD_STORAGE_PER_LEVEL: u16 = 20;
pub const WOOD_STORAGE_PER_LEVEL: u16 = 50;

pub const STORAGE_CAPACITY_RESEARCH_MULTIPLIER: f32 = 0.1;

pub const RESEARCH_COST: u8 = 5;
pub const BITS_PER_RESEARCH: u8 = 2;
pub const RESEARCH_MASK: u8 = 0b11;

#[repr(u8)]
pub enum ResearchType {
    BuildingSpeed,
    BuildingCost,
    DeteriorationCap,
    Placeholder,
    StorageCapacity,
    ResourceCollectionSpeed,
    EnvironmentRegeneration,
    Mining,
    ExtraUnit,
    DeathTimeout,
    Consumption,
    Placeholder2,
    MaxEnergyCap,
    EnergyRegeneration,
    FaithBonus,
    Placeholder3,
}

pub const BUILDING_COST_RESEARCH_MULTIPLIER: f32 = 0.1;
pub const BUILDING_SPEED_RESEARCH_TURN_REDUCTION: u8 = 1;
pub const FAITH_BONUS_RESEARCH_MULTIPLIER: u8 = 1;
pub const MAX_ENERGY_CAP_RESEARCH_MULTIPLIER: u8 = 1;
pub const ENERGY_REGEN_RESEARCH_MULTIPLIER: u8 = 1;

pub fn get_research_level(value: u32, research_type: ResearchType) -> u8 {
    return get_research_level_u8(value, research_type as u8);
}

pub fn get_research_level_u8(value: u32, research_type: u8) -> u8 {
    let shift_by = BITS_PER_RESEARCH * (research_type as u8);
    return (value >> shift_by) as u8 & RESEARCH_MASK;
}
