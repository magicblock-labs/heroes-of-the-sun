use bolt_lang::*;

declare_id!("HM794G8VuTSaYv1oNGxNhxAAJVp4UoVdM1QApdT7C9UU");

#[system]
pub mod claim_time {

    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        //todo balance faith dependency
        let cap: u16 = 10 + settlement.faith as u16 / 10; //max 22
        let s_per_unit: i64 = 60 * (20 - settlement.faith as i64 / 10); //20..8 min per time unit

        let now = Clock::get()?.unix_timestamp;

        let time_passed: i64 = now - settlement.last_time_claim;
        let claimable = time_passed / s_per_unit as i64;

        if claimable > 0 {
            settlement.time_units += u16::min(claimable as u16, cap - settlement.time_units);
        }

        if settlement.time_units == cap {
            settlement.last_time_claim = now;
        } else {
            settlement.last_time_claim += claimable * s_per_unit;
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct EmptyArgs {
        //can this be avoided?
    }
}
