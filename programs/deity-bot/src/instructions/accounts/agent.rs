use anchor_lang::prelude::*;

#[account]
#[derive(InitSpace)]
pub struct Agent {
    pub context: Pubkey,
    #[max_len(100)]
    pub name: String,
    pub happiness: u8,
    pub trust: u8,
}

impl Default for Agent {
    fn default() -> Self {
        Self {
            context: Pubkey::default(),
            name: "Deity".to_string(),
            happiness: 30,
            trust: 10,
        }
    }
}

impl Agent {
    pub fn seed() -> &'static [u8] {
        b"agent"
    }
}

// #[account]
// pub struct AgentCounter {
//     pub count: u32,
// }
