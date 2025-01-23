use std::str::FromStr;

use bolt_lang::*;

declare_id!("9RfzWgEBYQAM64a46V3dGRPKYsVY8a7YvZszWPMxvBfk");

#[component(delegate)]
pub struct SmartObjectDeity {
    pub system: Pubkey,
}

impl Default for SmartObjectDeity {
    fn default() -> Self {
        let system_program_id =
            Pubkey::from_str("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB").unwrap();

        Self::new(SmartObjectDeityInit {
            system: system_program_id,
        })
    }
}
