use bolt_lang::*;

use crate::{EnvironmentState, ResourceBalance};

//this enum matches the BUILDINGS_CONFIG array indexes (so we don't need to use a map (BuildingType=>BuildingConfig))
#[component_deserialize]
pub enum BuildingType {
    TownHall = 0,
    Altar = 1,
    Research = 2,
    FoodCollector = 3,
    FoodStorage = 4,
    WoodCollector = 5,
    WoodStorage = 6,
    WaterCollector = 7,
    WaterStorage = 8,
    StoneCollector = 9,
    StoneStorage = 10,
}

pub struct BuildingConfig {
    pub id: BuildingType,
    pub width: u8,
    pub height: u8,
    pub cost_tier: u8,
    pub build_time_tier: u8,
}

pub const MAP_WIDTH: u8 = 20;
pub const MAP_HEIGHT: u8 = 20;

pub const INITIAL_TIME_UNITS: u8 = 50;
pub const INITIAL_FAITH: u8 = 50;
pub const CHUNK_SIZE: u8 = 24;

pub const INITIAL_TREASURY: ResourceBalance = ResourceBalance {
    water: 40,
    food: 80,
    wood: 200,
    stone: 10,
};

pub const EXCHANGE_RATES: ResourceBalance = ResourceBalance {
    water: 10,
    food: 10,
    wood: 6,
    stone: 3,
};

pub const INITIAL_ENVIRONMENT: EnvironmentState = EnvironmentState {
    food: 200,
    wood: 200,
};

pub const ENVIRONMENT_MAX: EnvironmentState = EnvironmentState {
    food: 1000,
    wood: 1000,
};

pub const BUILDINGS_CONFIG: [BuildingConfig; 11] = [
    BuildingConfig {
        id: BuildingType::TownHall,
        width: 4,
        height: 4,
        cost_tier: 5,
        build_time_tier: 4,
    },
    BuildingConfig {
        id: BuildingType::Altar,
        width: 3,
        height: 3,
        cost_tier: 3,
        build_time_tier: 5,
    },
    BuildingConfig {
        id: BuildingType::Research,
        width: 3,
        height: 3,
        cost_tier: 3,
        build_time_tier: 5,
    },
    BuildingConfig {
        id: BuildingType::FoodCollector,
        width: 2,
        height: 2,
        cost_tier: 1,
        build_time_tier: 2,
    },
    BuildingConfig {
        id: BuildingType::FoodStorage,
        width: 3,
        height: 3,
        cost_tier: 2,
        build_time_tier: 4,
    },
    BuildingConfig {
        id: BuildingType::WoodCollector,
        width: 2,
        height: 2,
        cost_tier: 1,
        build_time_tier: 2,
    },
    BuildingConfig {
        id: BuildingType::WoodStorage,
        width: 3,
        height: 3,
        cost_tier: 2,
        build_time_tier: 4,
    },
    BuildingConfig {
        id: BuildingType::WaterCollector,
        width: 2,
        height: 2,
        cost_tier: 1,
        build_time_tier: 2,
    },
    BuildingConfig {
        id: BuildingType::WaterStorage,
        width: 3,
        height: 3,
        cost_tier: 2,
        build_time_tier: 4,
    },
    BuildingConfig {
        id: BuildingType::StoneCollector,
        width: 3,
        height: 3,
        cost_tier: 1,
        build_time_tier: 2,
    },
    BuildingConfig {
        id: BuildingType::StoneStorage,
        width: 3,
        height: 3,
        cost_tier: 2,
        build_time_tier: 4,
    },
];

pub const BASE_ENERGY_CAP: u8 = 30;
pub const ENERGY_CAP_FAITH_MULTIPLIER: f32 = 0.1;
pub const BASE_MINUTE_PER_ENERGY_UNIT: i64 = 10;

pub const ENERGY_REGEN_FAITH_MULTIPLIER: f32 = 0.05;

pub const BASE_DEATH_TIMEOUT: i8 = -10;
pub const DEATH_TIMEOUT_RESEARCH_MULTIPLIER: u8 = 1;
pub const SACRIFICE_FAITH_BOOST: u8 = 10;

pub const FAITH_TO_RUNWAY_LERP_PER_TURN: u8 = 1;

pub const BASE_DETERIORATION_CAP: u8 = 50;
pub const DETERIORATION_CAP_RESEARCH_MULTIPLIER: u8 = 5;

pub const TOWNHALL_STORAGE_PER_LEVEL: u16 = 10;
pub const WATER_STORAGE_PER_LEVEL: u16 = 10;
pub const FOOD_STORAGE_PER_LEVEL: u16 = 20;
pub const WOOD_STORAGE_PER_LEVEL: u16 = 50;
pub const STONE_STORAGE_PER_LEVEL: u16 = 15;
pub const GOLD_STORAGE_PER_LEVEL: u16 = 5;

pub const STORAGE_CAPACITY_RESEARCH_MULTIPLIER: f32 = 0.1;

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
pub const ENERGY_REGENERATION_RESEARCH_MULTIPLIER: u8 = 1;

pub fn get_research_level(value: u32, research_type: ResearchType) -> u8 {
    return get_research_level_u8(value, research_type as u8);
}

pub fn get_research_level_u8(value: u32, research_type: u8) -> u8 {
    let shift_by = BITS_PER_RESEARCH * (research_type as u8);
    return (value >> shift_by) as u8 & RESEARCH_MASK;
}

pub fn get_storage_level_multiplier(level: u8) -> u16 {
    return (2 as u16).pow(level as u32);
}

pub fn get_collection_level_multiplier(level: u8) -> u16 {
    return (2 as u16).pow(level as u32);
}

pub fn get_extraction_cap(level: u8) -> u16 {
    return 10 * (2 as u16).pow(level as u32);
}

pub const BASE_RESEARCH_COST: u16 = 5;
pub fn get_research_cost(_research_type: u8, level: u8) -> u16 {
    return BASE_RESEARCH_COST * (2 as u16).pow(level as u32);
}

pub fn get_regeneration_rate(research: u32) -> u16 {
    return 3 + get_research_level(research, ResearchType::EnvironmentRegeneration) as u16;
}

fn calculate_cost(research: u32, tier: u8, level: u8, level_offset: u8, multiplier: f32) -> u16 {
    if level_offset >= level {
        return 0;
    }

    let base_cost = 2;
    let level_multiplier: f32 = 1.5;
    let cost_research = get_research_level(research, ResearchType::BuildingCost);
    let cost = ((base_cost * tier) as f32 * (level_multiplier).powf((level - level_offset) as f32))
        * (1.0 - BUILDING_COST_RESEARCH_MULTIPLIER * (cost_research as f32))
        * multiplier;

    return cost.ceil() as u16;
}

pub fn get_construction_cost(
    research: u32,
    tier: u8,
    level: u8,
    multiplier: f32,
) -> ResourceBalance {
    return ResourceBalance {
        food: 0,
        water: 0,
        wood: calculate_cost(research, tier, level, 0, multiplier),
        stone: calculate_cost(research, tier, level, 4, multiplier),
    };
}

pub fn get_build_time(research: u32, tier: u8, level: u8) -> u8 {
    let level_multiplier: f32 = 1.2;
    let base_cost = (tier as f32 * (level_multiplier).powf(level as f32)) as u8;
    return base_cost
        - u8::min(
            BUILDING_SPEED_RESEARCH_TURN_REDUCTION
                * get_research_level(research, ResearchType::BuildingSpeed),
            base_cost,
        );
}

#[repr(u8)]
pub enum QuestType {
    Build,
    Upgrade,
    Store,
    Research,
    Faith,
}

#[repr(u8)]
pub enum Resource {
    Food,
    Water,
    Wood,
    Stone,
}

pub struct QuestConfig {
    pub id: u32,
    pub quest_type: QuestType,
    pub target_type: u8,
    pub target_value: u8,
    pub reward_type: u8,
    pub reward_value: u16,
}

pub const QUESTS_CONFIG: [QuestConfig; 14] = [
    QuestConfig {
        id: 0,
        quest_type: QuestType::Build,
        target_type: BuildingType::FoodStorage as u8,
        target_value: 1,
        reward_type: Resource::Wood as u8,
        reward_value: 10,
    },
    QuestConfig {
        id: 1,
        quest_type: QuestType::Build,
        target_type: BuildingType::FoodCollector as u8,
        target_value: 1,
        reward_type: Resource::Water as u8,
        reward_value: 10,
    },
    QuestConfig {
        id: 2,
        quest_type: QuestType::Build,
        target_type: BuildingType::WaterStorage as u8,
        target_value: 1,
        reward_type: Resource::Water as u8,
        reward_value: 10,
    },
    QuestConfig {
        id: 3,
        quest_type: QuestType::Build,
        target_type: BuildingType::WaterCollector as u8,
        target_value: 1,
        reward_type: Resource::Water as u8,
        reward_value: 10,
    },
    QuestConfig {
        id: 4,
        quest_type: QuestType::Upgrade,
        target_type: BuildingType::TownHall as u8,
        target_value: 2,
        reward_type: Resource::Wood as u8,
        reward_value: 100,
    },
    QuestConfig {
        id: 5,
        quest_type: QuestType::Upgrade,
        target_type: BuildingType::WoodCollector as u8,
        target_value: 2,
        reward_type: Resource::Water as u8,
        reward_value: 20,
    },
    QuestConfig {
        id: 6,
        quest_type: QuestType::Upgrade,
        target_type: BuildingType::TownHall as u8,
        target_value: 3,
        reward_type: Resource::Stone as u8,
        reward_value: 10,
    },
    QuestConfig {
        id: 7,
        quest_type: QuestType::Store,
        target_type: Resource::Food as u8,
        target_value: 30,
        reward_type: Resource::Stone as u8,
        reward_value: 5,
    },
    QuestConfig {
        id: 8,
        quest_type: QuestType::Store,
        target_type: Resource::Wood as u8,
        target_value: 50,
        reward_type: Resource::Stone as u8,
        reward_value: 5,
    },
    QuestConfig {
        id: 9,
        quest_type: QuestType::Store,
        target_type: Resource::Stone as u8,
        target_value: 30,
        reward_type: Resource::Stone as u8,
        reward_value: 20,
    },
    QuestConfig {
        id: 10,
        quest_type: QuestType::Research,
        target_type: ResearchType::BuildingCost as u8,
        target_value: 1,
        reward_type: Resource::Stone as u8,
        reward_value: 5,
    },
    QuestConfig {
        id: 11,
        quest_type: QuestType::Research,
        target_type: ResearchType::Consumption as u8,
        target_value: 1,
        reward_type: Resource::Stone as u8,
        reward_value: 5,
    },
    QuestConfig {
        id: 12,
        quest_type: QuestType::Faith,
        target_type: 0,
        target_value: 30,
        reward_type: Resource::Stone as u8,
        reward_value: 15,
    },
    QuestConfig {
        id: 13,
        quest_type: QuestType::Faith,
        target_type: 0,
        target_value: 60,
        reward_type: Resource::Stone as u8,
        reward_value: 30,
    },
];
