use bolt_lang::*;

declare_id!("J2HTjpKDf317Q7Pg9kUVFDregE2Ld34P61M5m4XnVSh2");

#[system]
pub mod reset {
    use settlement::{
        config::{self, get_extraction_cap, BuildingType},
        Building, Settlement,
    };

    pub fn execute(ctx: Context<Components>, _args: ResetArgs) -> Result<Components> {
        let clock = Clock::get();
        let mut now = 0;

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        ctx.accounts.settlement.buildings = vec![Building {
            x: 8,
            y: 8,
            deterioration: 0,
            id: BuildingType::TownHall,
            level: 1,
            turns_to_build: 0,
        }];

        ctx.accounts.settlement.worker_assignment = vec![-1]; //one worker comes as default from town hall
        ctx.accounts.settlement.extraction = vec![get_extraction_cap(1)]; //TODO [CLEANUP] replace this with hashmap to not store useless extraction value for in this case townhall
        ctx.accounts.settlement.treasury = config::INITIAL_TREASURY;
        ctx.accounts.settlement.environment = config::INITIAL_ENVIRONMENT;
        ctx.accounts.settlement.time_units = config::INITIAL_TIME_UNITS;
        ctx.accounts.settlement.faith = config::INITIAL_FAITH;
        ctx.accounts.settlement.last_time_claim = now;
        ctx.accounts.settlement.research = 0;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct ResetArgs {}
}
