use bolt_lang::*;
// use token_minter::cpi::accounts::BurnToken;
mod errors;

declare_id!("3ZJ7mgXYhqQf7EsM8q5Ea5YJWA712TFyWGvrj9mRL2gP");

#[system]
pub mod research {

    use settlement::{
        config::{self, get_research_level_u8, ResearchType, BITS_PER_RESEARCH, RESEARCH_MASK},
        Settlement,
    };

    pub fn execute(ctx: Context<Components>, args: ResearchArgs) -> Result<Components> {
        msg!("execute research!: ");

        // let minter_program = ctx
        //     .minter_program()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // let mint_account = ctx
        //     .mint_account()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // msg!("mint_account: {}", mint_account.key);
        // let associated_token_account = ctx
        //     .associated_token_account()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // msg!("associated_token_account: {}", associated_token_account.key);
        // let token_program = ctx
        //     .token_program()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // msg!("token_program: {}", token_program.key);
        // let associated_token_program = ctx
        //     .associated_token_program()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // msg!("associated_token_program: {}", associated_token_program.key);
        // let payer = ctx
        //     .signer()
        //     .map_err(|_| ProgramError::InvalidAccountData)?
        //     .clone();
        // msg!("payer: {}", payer.key);

        let settlement = &mut ctx.accounts.settlement;

        //would be nice to get the size_of(settlement.research) though
        if 32 <= (args.research_type * BITS_PER_RESEARCH) as usize {
            return err!(errors::ResearchError::ResearchIndexOutOfRange);
        }

        let mut research_level = get_research_level_u8(settlement.research, args.research_type);

        let research_cost = config::get_research_cost(args.research_type, research_level);

        // let res = token_minter::cpi::burn_token(
        //     CpiContext::new(
        //         minter_program,
        //         BurnToken {
        //             payer,
        //             mint_account,
        //             associated_token_account,
        //             token_program,
        //             associated_token_program,
        //         },
        //     ),
        //     research_cost as u64,
        // );
        // if !res.is_ok() {
        //     return err!(errors::ResearchError::NotEnoughResources);
        // }
        // msg!("burn done!: ");

        if settlement.treasury.stone < research_cost {
            return err!(errors::ResearchError::NotEnoughResources);
        }

        if research_level >= RESEARCH_MASK {
            return err!(errors::ResearchError::AlreadyMaxedOut);
        }

        settlement.treasury.stone -= research_cost;
        research_level += 1;

        let mut research_value = settlement.research;

        //cut out old value
        research_value &= !((RESEARCH_MASK as u32) << BITS_PER_RESEARCH * args.research_type);

        //replace with new one
        research_value |= (research_level as u32) << BITS_PER_RESEARCH * args.research_type;

        settlement.research = research_value;
        // settlement.treasury.gold -= research_cost;

        if args.research_type == ResearchType::ExtraUnit as u8 {
            settlement.worker_assignment.push(-1);
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

    // #[extra_accounts]
    // pub struct ExtraAccounts {
    //     #[account(mut)]
    //     signer: Signer<'info>,

    //     #[account()]
    //     associated_token_account: Account<'info, TokenAccount>,

    //     #[account()]
    //     mint_account: Account<'info, Mint>,

    //     #[account()]
    //     minter_program: AccountInfo,

    //     #[account()]
    //     token_program: Program<'info, Token>,

    //     #[account()]
    //     associated_token_program: Program<'info, AssociatedToken>,

    //     #[account()]
    //     system_program: Program<'info, System>,
    // }
}
