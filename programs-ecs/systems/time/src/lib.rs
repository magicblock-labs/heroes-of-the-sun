use bolt_lang::*;
use settlement::Settlement;

declare_id!("AyMpNWt8uw4nEHReXXEUhbDmgmQQbfG4HZjCSkupbEne");

#[system]
pub mod time {

    pub fn execute(ctx: Context<Components>, _args_p: Vec<u8>) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.day += 1;

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }
}
