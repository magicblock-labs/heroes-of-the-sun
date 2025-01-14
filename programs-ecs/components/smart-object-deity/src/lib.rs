use std::str::FromStr;

use bolt_lang::*;

declare_id!("9RfzWgEBYQAM64a46V3dGRPKYsVY8a7YvZszWPMxvBfk");

pub const COOLDOWN: i64 = 60;

#[component(delegate)]
pub struct SmartObjectDeity {
    pub next_interaction_time: i64,
    pub system: Pubkey,
}

impl Default for SmartObjectDeity {
    fn default() -> Self {
        let clock = Clock::get();
        let mut now = 0;
        let system_program_id =
            Pubkey::from_str("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB").unwrap();

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        Self::new(SmartObjectDeityInit {
            next_interaction_time: now,
            system: system_program_id,
        })
    }
}
