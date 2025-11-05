use bolt_lang::*;
pub use crate::ecs_bundle::{SmartObjectDeity, SmartObjectDeityInit};
use std::str::FromStr;

impl Default for SmartObjectDeity {
    fn default() -> Self {
        let system_program_id =
            Pubkey::from_str("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB").unwrap();

        Self::new(SmartObjectDeityInit {
            system: system_program_id,
        })
    }
}
