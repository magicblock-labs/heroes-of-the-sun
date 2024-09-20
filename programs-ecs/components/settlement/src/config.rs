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

pub fn get_research_level(value: u32, research_type: ResearchType) -> u8 {
    return get_research_level_u8(value, research_type as u8);
}

pub fn get_research_level_u8(value: u32, research_type: u8) -> u8 {
    let shift_by = BITS_PER_RESEARCH * (research_type as u8);
    return (value >> shift_by) as u8 & RESEARCH_MASK;
}
