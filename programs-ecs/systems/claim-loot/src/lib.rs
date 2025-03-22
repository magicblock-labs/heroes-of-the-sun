use bolt_lang::*;
mod errors;

declare_id!("4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf");

#[system]
pub mod claim_loot {
    // use hero::Hero;
    use loot_distribution::LootDistribution;
    use token_minter::cpi::accounts::MintToken;

    pub fn execute(ctx: Context<Components>, args: ClaimLootArgs) -> Result<Components> {
        // Extract and clone all necessary accounts upfront
        let minter_program = ctx
            .minter_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let mint_account = ctx
            .mint_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let associated_token_account = ctx
            .associated_token_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let token_program = ctx
            .token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let associated_token_program = ctx
            .associated_token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let system_program = ctx
            .system_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let payer = ctx.signer().map_err(|_| ProgramError::InvalidAccountData)?;

        let session_token = ctx
            .session_token()
            .map_err(|_| ProgramError::InvalidAccountData)?;

        msg!("payer: {}", payer.key);
        msg!("mint_account: {}", mint_account.key);
        msg!("associated_token_account: {}", associated_token_account.key);
        msg!("token_program: {}", token_program.key);
        msg!("associated_token_program: {}", associated_token_program.key);
        msg!("system_program: {}", system_program.key);
        msg!("session_token: {}", session_token.key);

        let loot = &mut ctx.accounts.loot;
        // let hero = &mut ctx.accounts.hero;

        // let loot_loc = loot.loots[args.index as usize];

        //todo commit hero location before comparing to loot location?
        // if loot_loc.x != hero.x || loot_loc.y != hero.y {
        //     return err!(errors::ClaimLootError::LocationMismatch);
        // }

        let res = token_minter::cpi::mint_token(
            CpiContext::new(
                minter_program.clone(),
                MintToken {
                    payer: payer.clone(),
                    mint_account: mint_account.clone(),
                    associated_token_account: associated_token_account.clone(),
                    token_program: token_program.clone(),
                    associated_token_program: associated_token_program.clone(),
                    system_program: system_program.clone(),
                    session_token: Some(session_token.clone()),
                },
            ),
            1 as u64,
        );

        if res.is_ok() {
            loot.index += 1;
            loot.loots[args.index as usize] = loot_distribution::get_loot_location(loot.index);
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub loot: LootDistribution,
    }

    #[arguments]
    struct ClaimLootArgs {
        index: u8,
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

        #[account()]
        pub session_token: Option<Account<'info, SessionToken>>,
    }
}
