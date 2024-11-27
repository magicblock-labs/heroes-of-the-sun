use {
    anchor_lang::prelude::*,
    anchor_spl::{
        associated_token::AssociatedToken,
        token::{burn, Burn, Mint, Token, TokenAccount},
    },
};

#[derive(Accounts)]
pub struct BurnToken<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,

    #[account(
        mut,
        associated_token::mint = mint_account,
        associated_token::authority = payer
    )]
    pub associated_token_account: Account<'info, TokenAccount>,

    #[account(
        mut,
        seeds = [b"mint"],
        bump,
    )]
    pub mint_account: Account<'info, Mint>,

    pub token_program: Program<'info, Token>,
    pub associated_token_program: Program<'info, AssociatedToken>,
}

pub fn burn_token(ctx: Context<BurnToken>, amount: u64) -> Result<()> {
    msg!("Burning token from associated token account...");
    msg!("Mint: {}", &ctx.accounts.mint_account.key());
    msg!(
        "Token Address: {}",
        &ctx.accounts.associated_token_account.key()
    );

    // PDA signer seeds
    let signer_seeds: &[&[&[u8]]] = &[&[b"mint", &[ctx.bumps.mint_account]]];

    // Invoke the mint_to instruction on the token program
    burn(
        CpiContext::new(
            ctx.accounts.token_program.to_account_info(),
            Burn {
                mint: ctx.accounts.mint_account.to_account_info(),
                from: ctx.accounts.associated_token_account.to_account_info(),
                authority: ctx.accounts.payer.to_account_info(), // PDA mint authority, required as signer
            },
        )
        .with_signer(signer_seeds), // using PDA to sign
        amount * 10u64.pow(ctx.accounts.mint_account.decimals as u32), // Burn tokens, adjust for decimals
    )?;

    msg!("Token burned successfully.");

    Ok(())
}
