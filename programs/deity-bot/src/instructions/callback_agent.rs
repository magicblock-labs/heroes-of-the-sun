use anchor_lang::prelude::*;
use solana_gpt_oracle::Identity;
use token_minter::cpi::accounts::MintToken;

use anchor_spl::{
    associated_token::AssociatedToken,
    token::{Mint, Token, TokenAccount},
};

use super::accounts::Agent;

pub fn callback_from_agent(ctx: Context<CallbackFromAgent>, response: String) -> Result<()> {
    // Check if the callback is from the LLM program
    if !ctx.accounts.identity.to_account_info().is_signer {
        return Err(ProgramError::InvalidAccountData.into());
    }

    // Parse the JSON response
    let response: String = response
        .trim()
        .trim_start_matches("```json")
        .trim_end_matches("```")
        .to_string();
    let parsed: serde_json::Value =
        serde_json::from_str(&response).unwrap_or_else(|_| serde_json::json!({}));

    // Extract the reply and amount
    let reply = parsed["reply"]
        .as_str()
        .unwrap_or("I'm sorry, I'm busy now!");

    let amount = parsed["amount"].as_u64().unwrap_or(0);
    let happiness = parsed["happiness"].as_u64().unwrap_or(0);
    let trust = parsed["trust"].as_u64().unwrap_or(0);

    msg!("Agent Reply: {:?}", reply);
    msg!("Happiness: {:?}", happiness);
    msg!("Trust: {:?}", trust);

    ctx.accounts.agent.happiness = happiness as u8;
    ctx.accounts.agent.trust = trust as u8;

    if amount == 0 {
        return Ok(());
    }

    // Invoke the mint_to instruction on the token program
    token_minter::cpi::mint_token(
        CpiContext::new(
            ctx.accounts.token_program.to_account_info(),
            MintToken {
                payer: ctx.accounts.user.to_account_info(),
                mint_account: ctx.accounts.mint_account.to_account_info(),
                associated_token_account: ctx.accounts.associated_token_account.to_account_info(),
                token_program: ctx.accounts.token_program.to_account_info(),
                associated_token_program: ctx.accounts.associated_token_program.to_account_info(),
                system_program: ctx.accounts.system_program.to_account_info(),
            },
        ),
        1 as u64,
    )?;
    Ok(())
}

#[derive(Accounts)]
pub struct CallbackFromAgent<'info> {
    /// CHECK: Checked in oracle program
    pub identity: Account<'info, Identity>,
    /// CHECK: The user wo did the interaction
    pub user: AccountInfo<'info>,
    #[account(mut, seeds = [Agent::seed(), user.key().as_ref()], bump)]
    pub agent: Account<'info, Agent>,
    #[account(
        mut,
        seeds = [b"mint"],
        bump
    )]
    pub mint_account: Account<'info, Mint>,
    #[account(
        mut,
        associated_token::mint = mint_account,
        associated_token::authority = user,
    )]
    pub associated_token_account: Account<'info, TokenAccount>,
    pub token_program: Program<'info, Token>,
    pub associated_token_program: Program<'info, AssociatedToken>,
    pub system_program: Program<'info, System>,
}
