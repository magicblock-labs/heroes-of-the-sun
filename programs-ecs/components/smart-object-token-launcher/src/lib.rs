use std::str::FromStr;

use bolt_lang::*;
use ecs_bundle::settlement::ResourceBalance;

declare_id!("8va4yKEBACkT49C9wo94gS8ZaTdUrq2ipLgZvSNxWbd3");

#[component(delegate)]
pub struct SmartObjectTokenLauncher {
    pub system: Pubkey,
    pub mint: Pubkey,
    pub recipe: ResourceBalance,
}

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
