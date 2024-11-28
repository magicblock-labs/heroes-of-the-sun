mod errors;
use bolt_lang::*;

declare_id!("6JwZJNAtkciXVGenFSoa99VBNcxyb2W8mvzcMK1vTWKs");

#[system]
pub mod sacrifice {
    use settlement::{
        config::{self, get_research_level, ResearchType},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: SacrificeArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if settlement.worker_assignment.len() <= args.index as usize {
            return err!(errors::SacrificeError::LabourIndexOutOfRange);
        }

        if settlement.worker_assignment[args.index as usize] < -1 {
            return err!(errors::SacrificeError::NotRestoredYet);
        }

        settlement.worker_assignment[args.index as usize] = config::BASE_DEATH_TIMEOUT
            + (config::DEATH_TIMEOUT_RESEARCH_MULTIPLIER
                * get_research_level(settlement.research, ResearchType::DeathTimeout))
                as i8;
        settlement.faith += config::SACRIFICE_FAITH_BOOST;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct SacrificeArgs {
        index: u8,
    }
}
