use bolt_lang::*;
use std::str::FromStr;
pub use crate::ecs_bundle::{SmartObjectTokenLauncher, SmartObjectTokenLauncherInit};
use crate::settlement::ResourceBalance;

impl Default for SmartObjectTokenLauncher {
    fn default() -> Self {
        let system_program_id =
            Pubkey::from_str("DUW1KczxcpeTEY7j9nkvcuAdWGNWoadTeDBKN5Z9xhst").unwrap();

        Self::new(SmartObjectTokenLauncherInit {
            system: system_program_id,
            mint: Pubkey::default(),
            recipe: ResourceBalance {
                water: 0,
                food: 0,
                wood: 0,
                stone: 0,
            },
        })
    }
}
