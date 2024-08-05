use bolt_lang::*;

//this enum matches the BUILDINGS_CONFIG array indexes (so we don't need to use a map (BuildingType=>BuildingConfig))
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

pub struct BuildingConfig {
    pub r#type: BuildingType,
    pub width: u8,
    pub height: u8,
    pub cost: u8,
}

pub const MAP_WIDTH: u8 = 20;
pub const MAP_HEIGHT: u8 = 20;

pub const BUILDINGS_CONFIG: [BuildingConfig; 8] = [
    BuildingConfig {
        r#type: BuildingType::TownHall,
        width: 4,
        height: 4,
        cost: 50,
    },
    BuildingConfig {
        r#type: BuildingType::WaterCollector,
        width: 1,
        height: 1,
        cost: 10,
    },
    BuildingConfig {
        r#type: BuildingType::FoodCollector,
        width: 2,
        height: 2,
        cost: 10,
    },
    BuildingConfig {
        r#type: BuildingType::WoodCollector,
        width: 1,
        height: 3,
        cost: 5,
    },
    BuildingConfig {
        r#type: BuildingType::WaterStorage,
        width: 2,
        height: 2,
        cost: 5,
    },
    BuildingConfig {
        r#type: BuildingType::FoodStorage,
        width: 2,
        height: 2,
        cost: 8,
    },
    BuildingConfig {
        r#type: BuildingType::WoodStorage,
        width: 2,
        height: 2,
        cost: 8,
    },
    BuildingConfig {
        r#type: BuildingType::Altar,
        width: 3,
        height: 3,
        cost: 60,
    },
];
