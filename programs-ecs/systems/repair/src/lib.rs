mod errors;
use bolt_lang::*;

declare_id!("4MA6KhwEUsLbZJqJK9rqwVjdZgdxy7vbebuD2MeLKm5j");

#[system]
pub mod repair {
    use settlement::{
        config::{self, get_research_level, ResearchType, BUILDINGS_CONFIG},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: RepairArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if args.index as usize >= settlement.buildings.len() {
            return err!(errors::RepairError::BuildingIndexOutOfRange);
        }

        let building = settlement.buildings[args.index as usize];
        let building_config = &BUILDINGS_CONFIG[building.id as usize];

        //TODO [BALANCE] multiply cost by level??

        let max_deterioration = config::BASE_DETERIORATION_CAP
            + config::DETERIORATION_CAP_RESEARCH_MULTIPLIER
                * get_research_level(settlement.research, ResearchType::DeteriorationCap);

        let repair_cost = (building_config.cost
            * (settlement.buildings[args.index as usize].deterioration / max_deterioration))
            as u16;

        if settlement.treasury.wood < repair_cost {
            return err!(errors::RepairError::NotEnoughResources);
        }

        settlement.treasury.wood -= repair_cost;
        //all checks passed
        settlement.buildings[args.index as usize].deterioration = 0;

        Ok((ctx.accounts))
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct RepairArgs {
        index: u8,
    }
}
