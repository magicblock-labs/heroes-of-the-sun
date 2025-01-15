use anchor_lang::prelude::*;
use anchor_lang::Discriminator;
use solana_gpt_oracle::ContextAccount;

use anchor_spl::{
    associated_token::AssociatedToken,
    token::{Mint, Token, TokenAccount},
};

use super::accounts::Agent;

pub fn interact_agent(ctx: Context<InteractAgent>, option: u8) -> Result<()> {
    let cpi_program = ctx.accounts.oracle_program.to_account_info();
    let cpi_accounts = solana_gpt_oracle::cpi::accounts::InteractWithLlm {
        payer: ctx.accounts.payer.to_account_info(),
        interaction: ctx.accounts.interaction.to_account_info(),
        context_account: ctx.accounts.context_account.to_account_info(),
        system_program: ctx.accounts.system_program.to_account_info(),
    };
    let cpi_ctx = CpiContext::new(cpi_program, cpi_accounts);

    let mut text = "hello".to_string();
    if option > 0 {
        text = option.to_string();
    }

    solana_gpt_oracle::cpi::interact_with_llm(
        cpi_ctx,
        text,
        crate::ID,
        crate::CallbackFromAgent::discriminator(),
        Some(vec![
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx.accounts.payer.to_account_info().key(),
                is_signer: false,
                is_writable: false,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx.accounts.agent.to_account_info().key(),
                is_signer: false,
                is_writable: true,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx.accounts.mint_account.to_account_info().key(),
                is_signer: false,
                is_writable: true,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx
                    .accounts
                    .associated_token_account
                    .to_account_info()
                    .key(),
                is_signer: false,
                is_writable: true,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx.accounts.token_program.to_account_info().key(),
                is_signer: false,
                is_writable: false,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx
                    .accounts
                    .associated_token_program
                    .to_account_info()
                    .key(),
                is_signer: false,
                is_writable: false,
            },
            solana_gpt_oracle::AccountMeta {
                pubkey: ctx.accounts.system_program.to_account_info().key(),
                is_signer: false,
                is_writable: false,
            },
        ]),
    )?;

    Ok(())
}

#[derive(Accounts)]
#[instruction(text: String)]
pub struct InteractAgent<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,
    /// CHECK: Checked in oracle program
    #[account(mut)]
    pub interaction: AccountInfo<'info>,
    #[account(seeds = [Agent::seed(), payer.key().as_ref()], bump)]
    pub agent: Account<'info, Agent>,
    #[account(address = agent.context)]
    pub context_account: Account<'info, ContextAccount>,
    #[account(
        init_if_needed,
        payer = payer,
        associated_token::mint = mint_account,
        associated_token::authority = payer,
    )]
    pub associated_token_account: Account<'info, TokenAccount>,
    #[account(
        mut,
        seeds = [b"mint"],
        bump
    )]
    pub mint_account: Account<'info, Mint>,
    /// CHECK: Checked oracle id
    #[account(address = solana_gpt_oracle::ID)]
    pub oracle_program: AccountInfo<'info>,
    pub token_program: Program<'info, Token>,
    pub associated_token_program: Program<'info, AssociatedToken>,
    pub system_program: Program<'info, System>,
}
