use settlement::Balance;

pub struct BuildingConfig {
    pub width: u8,
    pub height: u8,
    pub cost: [Balance; 1],
}

pub const MAP_WIDTH: u8 = 20;
pub const MAP_HEIGHT: u8 = 20;
pub const PERFECT_STATE: u8 = 4;

pub const BUILDINGS_CONFIG: [BuildingConfig; 3] = [
    BuildingConfig {
        width: 4,
        height: 4,
        cost: [Balance {
            resource_type: 0,
            amount: 30,
        }],
    },
    BuildingConfig {
        width: 4,
        height: 4,
        cost: [Balance {
            resource_type: 0,
            amount: 60,
        }],
    },
    BuildingConfig {
        width: 4,
        height: 4,
        cost: [Balance {
            resource_type: 0,
            amount: 120,
        }],
    },
];
