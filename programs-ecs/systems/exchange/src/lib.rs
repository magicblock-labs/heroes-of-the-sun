use bolt_lang::*;
mod errors;

declare_id!("Csna3V2jUMdQEQKUCxLsQEnYThAGPSWcPCxW9vea1S8d");

#[system]
pub mod exchange {
    use settlement::Settlement;
    use token_minter::cpi::accounts::BurnToken;

    pub fn execute(ctx: Context<Components>, args: ExchangeArgs) -> Result<Components> {
        let mut total_cost: u64 = 0;

        total_cost += args.tokens_for_food;
        total_cost += args.tokens_for_water;
        total_cost += args.tokens_for_wood;
        total_cost += args.tokens_for_stone;

        if total_cost == 0 {
            return err!(errors::ExchangeError::NoExchange);
        }

        msg!("execute exchange!: ");

        let minter_program = ctx
            .minter_program()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        let mint_account = ctx
            .mint_account()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        msg!("mint_account: {}", mint_account.key);
        let associated_token_account = ctx
            .associated_token_account()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        msg!("associated_token_account: {}", associated_token_account.key);
        let token_program = ctx
            .token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        msg!("token_program: {}", token_program.key);
        let associated_token_program = ctx
            .associated_token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        msg!("associated_token_program: {}", associated_token_program.key);
        let payer = ctx
            .signer()
            .map_err(|_| ProgramError::InvalidAccountData)?
            .clone();
        msg!("payer: {}", payer.key);

        //todo check balance before burning gas on CPI

        let res = token_minter::cpi::burn_token(
            CpiContext::new(
                minter_program,
                BurnToken {
                    payer,
                    mint_account,
                    associated_token_account,
                    token_program,
                    associated_token_program,
                },
            ),
            total_cost,
        );
        if !res.is_ok() {
            return err!(errors::ExchangeError::TokenBurnFailed);
        }
        msg!("burn done!: ");

        let settlement = &mut ctx.accounts.settlement;

        settlement.treasury.food +=
            args.tokens_for_food as u16 * settlement::config::EXCHANGE_RATES.food;

        settlement.treasury.water +=
            args.tokens_for_water as u16 * settlement::config::EXCHANGE_RATES.water;

        settlement.treasury.wood +=
            args.tokens_for_wood as u16 * settlement::config::EXCHANGE_RATES.wood;

        settlement.treasury.stone +=
            args.tokens_for_stone as u16 * settlement::config::EXCHANGE_RATES.stone;

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct ExchangeArgs {
        pub tokens_for_food: u64,
        pub tokens_for_water: u64,
        pub tokens_for_wood: u64,
        pub tokens_for_stone: u64,
    }

    #[extra_accounts]
    pub struct ExtraAccounts {
        #[account(mut)]
        signer: Signer<'info>,

        #[account()]
        associated_token_account: Account<'info, TokenAccount>,

        #[account()]
        mint_account: Account<'info, Mint>,

        #[account()]
        minter_program: AccountInfo,

        #[account()]
        token_program: Program<'info, Token>,

        #[account()]
        associated_token_program: Program<'info, AssociatedToken>,

        #[account()]
        system_program: Program<'info, System>,
    }
}
