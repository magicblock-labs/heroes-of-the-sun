use bolt_lang::*;
mod errors;

declare_id!("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB");

#[system]
pub mod smart_object_deity_interact {

    use deity_bot::cpi::accounts::InteractAgent;
    use smart_object_deity::{SmartObjectDeity, COOLDOWN};

    pub fn execute(ctx: Context<Components>, args: InteractionArgs) -> Result<Components> {
        let deity = &mut ctx.accounts.deity;

        let clock = Clock::get();
        let mut now = 0;

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        if now < deity.next_interaction_time {
            return err!(errors::SmartObjectDeityInteractionError::OnCooldown);
        }

        //CPI TO DEITY LLM

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
        let system_program = ctx
            .system_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let interaction = ctx
            .interaction()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let agent = ctx.agent().map_err(|_| ProgramError::InvalidAccountData)?;
        let oracle_program = ctx
            .oracle_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let context_account = ctx
            .context_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let payer = ctx.signer().map_err(|_| ProgramError::InvalidAccountData)?;

        msg!("payer: {}", payer.key);
        msg!("mint_account: {}", mint_account.key);
        msg!("associated_token_account: {}", associated_token_account.key);
        msg!("token_program: {}", token_program.key);
        msg!("associated_token_program: {}", associated_token_program.key);
        msg!("system_program: {}", system_program.key);
        msg!("context_account: {}", context_account.key);
        msg!("interaction: {}", interaction.key);
        msg!("agent: {}", agent.key);
        msg!("oracle_program: {}", oracle_program.key);

        let res = deity_bot::cpi::interact_agent(
            CpiContext::new(
                deity_bot_program.clone(),
                InteractAgent {
                    payer: payer.clone(),
                    mint_account: mint_account.clone(),
                    associated_token_account: associated_token_account.clone(),
                    token_program: token_program.clone(),
                    associated_token_program: associated_token_program.clone(),
                    system_program: system_program.clone(),
                    interaction: interaction.clone(),
                    agent: agent.clone(),
                    context_account: context_account.clone(),
                    oracle_program: oracle_program.clone(),
                },
            ),
            args.index,
        );

        //todo add non binary result to signify token mint
        deity.next_interaction_time = now + COOLDOWN;

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub deity: SmartObjectDeity,
    }

    #[arguments]
    struct InteractionArgs {
        pub index: u8,
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
        deity_bot_program: AccountInfo,

        #[account()]
        token_program: Program<'info, Token>,

        #[account()]
        associated_token_program: Program<'info, AssociatedToken>,

        #[account()]
        system_program: Program<'info, System>,

        #[account()]
        interaction: AccountInfo,

        #[account()]
        agent: AccountInfo,

        #[account()]
        context_account: AccountInfo,

        #[account()]
        oracle_program: AccountInfo,
    }
}
