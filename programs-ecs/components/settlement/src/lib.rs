pub mod config;

use bolt_lang::*;

declare_id!("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ");

#[component_deserialize]
pub struct Balance {
    pub amount: u16,
    pub resource_type: u8,
}

#[component_deserialize]
pub struct Source {
    pub balance: Balance,
    pub regeneration: u8,
    pub capacity: u16,
}

#[component_deserialize]
pub struct Building {
    pub x: u8,
    pub y: u8,
    pub state: u8,
    pub id: crate::config::BuildingType,
    pub level: u8,
}

#[component]
pub struct Settlement {
    #[max_len(12, 5)]
    pub buildings: Vec<Building>,

    #[max_len(3, 8)]
    pub sources: Vec<Source>,

    pub day: u16,
    pub faith: u8, //0..100
    #[max_len(3, 4)]
    pub treasury: Vec<Balance>,

    #[max_len(12, 1)]
    pub labour_allocation: Vec<i8>, //index is labour unit index, value is building index from /buildings/ array; singed: use -1 for free slot
}

impl Default for Settlement {
    fn default() -> Self {
        Self::new(SettlementInit {
            buildings: vec![],
            labour_allocation: vec![-1],
            sources: vec![
                Source {
                    balance: Balance {
                        resource_type: 0,
                        amount: 100,
                    },
                    regeneration: 4,
                    capacity: 200,
                },
                Source {
                    balance: Balance {
                        resource_type: 1,
                        amount: 20,
                    },
                    regeneration: 2,
                    capacity: 30,
                },
                Source {
                    balance: Balance {
                        resource_type: 2,
                        amount: 10,
                    },
                    regeneration: 1,
                    capacity: 20,
                },
            ],
            day: 0,
            faith: 50,
            treasury: vec![
                Balance {
                    resource_type: 0,
                    amount: 1000,
                },
                Balance {
                    resource_type: 1,
                    amount: 2,
                },
            ],
        })
    }
}
