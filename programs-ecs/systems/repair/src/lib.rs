mod errors;
use bolt_lang::*;

declare_id!("4MA6KhwEUsLbZJqJK9rqwVjdZgdxy7vbebuD2MeLKm5j");

#[system]
pub mod repair {
    use settlement::{
        config::{self, get_construction_cost, get_research_level, ResearchType, BUILDINGS_CONFIG},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: RepairArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if args.index as usize >= settlement.buildings.len() {
            return err!(errors::RepairError::BuildingIndexOutOfRange);
        }

        let building = settlement.buildings[args.index as usize];
        let building_config = &BUILDINGS_CONFIG[building.id as usize];

        let max_deterioration = config::BASE_DETERIORATION_CAP
            + config::DETERIORATION_CAP_RESEARCH_MULTIPLIER
                * get_research_level(settlement.research, ResearchType::DeteriorationCap);

        let build_cost = get_construction_cost(
            settlement.research,
            building_config.cost_tier,
            building.level,
            building.deterioration as f32 / max_deterioration as f32,
        );

        if settlement.treasury.wood < build_cost.wood
            || settlement.treasury.water < build_cost.water
            || settlement.treasury.food < build_cost.food
            || settlement.treasury.stone < build_cost.stone
            || settlement.treasury.gold < build_cost.gold
        {
            return err!(errors::RepairError::NotEnoughResources);
        } else {
            settlement.treasury.wood -= build_cost.wood;
            settlement.treasury.water -= build_cost.water;
            settlement.treasury.food -= build_cost.food;
            settlement.treasury.stone -= build_cost.stone;
            settlement.treasury.gold -= build_cost.gold;
        }

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
