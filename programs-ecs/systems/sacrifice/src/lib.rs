mod errors;
use bolt_lang::*;

declare_id!("85CUG48uuz6UYrvWYuDuK5Z79Q7rTGCP4y4CU933A7jd");

#[system]
pub mod sacrifice {
    use settlement::{
        config::{get_research_level, ResearchType},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: SacrificeArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if settlement.labour_allocation.len() <= args.index as usize {
            return err!(errors::SacrificeError::LabourIndexOutOfRange);
        }

        if settlement.labour_allocation[args.index as usize] < -1 {
            return err!(errors::SacrificeError::NotRestoredYet);
        }

        settlement.labour_allocation[args.index as usize] =
            -10 + get_research_level(settlement.research, ResearchType::DeathTimeout) as i8;
        settlement.faith += 10;
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
