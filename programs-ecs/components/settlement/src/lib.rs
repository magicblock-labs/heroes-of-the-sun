use bolt_lang::*;

declare_id!("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ");

#[component_deserialize]
pub struct Balance {
    pub amount: u16,
    pub resource_type: u8,
}

#[component_deserialize]
pub struct Source {
    pub x: u8,
    pub y: u8,
    pub resource: Balance,
}

#[component_deserialize]
pub struct Building {
    pub x: u8,
    pub y: u8,
    pub state: u8,
    pub id: u8,
    pub level: u8,
}

#[component]
pub struct Settlement {
    #[max_len(12, 5)]
    pub buildings: Vec<Building>,

    #[max_len(12, 5)]
    pub sources: Vec<Source>,

    pub day: u16,
    #[max_len(3, 6)]
    pub treasury: Vec<Balance>,
}

impl Default for Settlement {
    fn default() -> Self {
        Self::new(SettlementInit {
            buildings: vec![],
            sources: vec![Source {
                x: 0,
                y: 0,
                resource: Balance {
                    resource_type: 0,
                    amount: 100,
                },
            },Source {
                x: 0,
                y: 10,
                resource: Balance {
                    resource_type: 1,
                    amount: 50,
                },
            },Source {
                x: 10,
                y: 0,
                resource: Balance {
                    resource_type: 1,
                    amount: 50,
                },
            }],
            day: 0,
            treasury: vec![Balance {
                resource_type: 0,
                amount: 1000,
            }],
        })
    }
}
