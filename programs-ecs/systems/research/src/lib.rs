use bolt_lang::*;
mod errors;

declare_id!("nhCY8g1oJ34Xhu3koUzpD3DjyxXcnLDVyomnYaTv4yc");

#[system]
pub mod research {

    use settlement::{
        config::{self, get_research_level_u8, ResearchType, BITS_PER_RESEARCH, RESEARCH_MASK},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: ResearchArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        //would be nice to get the size_of(settlement.research) though
        if 32 <= (args.research_type * BITS_PER_RESEARCH) as usize {
            return err!(errors::ResearchError::ResearchIndexOutOfRange);
        }

        if settlement.treasury.wood < 5 {
            return err!(errors::ResearchError::NotEnoughResources);
        }

        msg!("Try Research : {}!", args.research_type);

        let mut research_level = get_research_level_u8(settlement.research, args.research_type);

        msg!("Old Research Level: {}!", research_level);

        if research_level >= RESEARCH_MASK {
            return err!(errors::ResearchError::AlreadyMaxedOut);
        }

        research_level += 1;

        msg!("New Research Level: {}!", research_level);

        let mut research_value = settlement.research;

        msg!("Old research value: {:32b}!", research_value);

        //cut out old value
        research_value &= !((RESEARCH_MASK as u32) << BITS_PER_RESEARCH * args.research_type);

        msg!("Cut out research value: {:32b}!", research_value);

        //replace with new one
        research_value |= (research_level as u32) << BITS_PER_RESEARCH * args.research_type;

        msg!("New out research value: {:32b}!", research_value);

        settlement.research = research_value;
        settlement.treasury.wood -= config::RESEARCH_COST as u16;

        if args.research_type == ResearchType::ExtraUnit as u8 {
            settlement.labour_allocation.push(-1);
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct ResearchArgs {
        pub research_type: u8,
    }
}
