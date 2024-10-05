use bolt_lang::*;

declare_id!("HFx2weMbr8CrAEAPfPtgw9zzgHgUFzSz7qiTyhTHGSF");

const SECONDS_IN_MINUTE: i64 = 60;

#[system]
pub mod claim_time {

    use settlement::{
        config::{self, get_research_level, ResearchType},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        //todo balance faith dependency
        let faith = (settlement.faith
            + config::FAITH_BONUS_RESEARCH_MULTIPLIER
                * get_research_level(settlement.research, ResearchType::FaithBonus))
            as u16;

        msg!("faith {}", faith);

        let cap: u8 = config::BASE_ENERGY_CAP
            + (faith as f32 * config::ENERGY_CAP_FAITH_MULTIPLIER) as u8
            + (config::MAX_ENERGY_CAP_RESEARCH_MULTIPLIER
                * get_research_level(settlement.research, ResearchType::MaxEnergyCap)); //[11..22] + research

        msg!("cap {}", cap);

        let s_per_unit: i64 = SECONDS_IN_MINUTE
            * (config::BASE_MINUTE_PER_ENERGY_UNIT
                - (config::ENERGY_REGEN_RESEARCH_MULTIPLIER
                    * get_research_level(settlement.research, ResearchType::EnergyRegeneration))
                    as i64
                - (faith as f32 * config::ENERGY_REGEN_FAITH_MULTIPLIER) as i64); //[20..8] - research min per time unit

        msg!("s_per_unit {}", s_per_unit);

        let now = Clock::get()?.unix_timestamp;

        let time_passed: i64 = now - settlement.last_time_claim;
        msg!(
            "now, last_time_claim, time_passed {} {} {}",
            now,
            settlement.last_time_claim,
            time_passed
        );

        let mut claimable = 0;

        if cap > settlement.time_units {
            claimable = u8::min(
                (time_passed / s_per_unit as i64) as u8,
                cap - settlement.time_units,
            );

            msg!("claimable {}", claimable);

            if claimable > 0 {
                settlement.time_units += claimable;
                settlement.last_time_claim += claimable as i64 * s_per_unit;
            }
        } else {
            settlement.last_time_claim = now;
        }

        msg!(
            "new time_units&last_time_claim {} {}",
            settlement.time_units,
            settlement.last_time_claim
        );

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
