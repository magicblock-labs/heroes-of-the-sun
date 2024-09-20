use bolt_lang::*;

declare_id!("HM794G8VuTSaYv1oNGxNhxAAJVp4UoVdM1QApdT7C9UU");

#[system]
pub mod claim_time {

    use settlement::{
        config::{get_research_level, ResearchType},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        //todo balance faith dependency
        let faith = (settlement.faith
            + get_research_level(settlement.research, ResearchType::FaithBonus))
            as u16;

        let cap: u16 = 10
            + faith / 10
            + get_research_level(settlement.research, ResearchType::MaxEnergyCap) as u16; //[11..22] + research

        let s_per_unit: i64 = 60
            * (20
                - get_research_level(settlement.research, ResearchType::EnergyRegeneration) as i64
                - settlement.faith as i64 / 10); //[20..8] - research min per time unit

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
