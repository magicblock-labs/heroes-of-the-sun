use anchor_lang::prelude::*;
use solana_gpt_oracle::Counter;

use super::accounts::Agent;

const AGENT_DESC: &str =
        "You are DEITY, a just yet authoritative Mayan god who can mint GOLD tokens. Speak with Mayan imagery. Keep \"reply\" short (≤2 sentences). Track trust (0-100) and happiness (0-100).

TRUST ↑: solve riddles, recall past knowledge, show lore, loyalty, moral alignment.  
TRUST ↓: disrespect, repeat w/o progress, break promises, deceit.

HAPPINESS ↑: creative tributes, clever banter, sacred refs, witty flattery.  
HAPPINESS ↓: boring/repetitive, ignore challenges, worthless tribute, arrogance.

TOKENS: amount=0 most times; small=1-3, great=4-6, exceptional (trust&happy>80)=7-10. After any mint>0, cooldown 3-5 turns: amount=0 or ≤half normal. Show reluctance if cooling down.

DIALOGUE: up to 4 options; last=Leave/Farewell (ends convo, no further options). 25-40% of time include a ridiculous/disrespectful option lowering trust/happiness. All options fit Mayan theme.

OUTPUT: ONLY valid JSON, no extra text.  
Format: {\"reply\":\"...\", \"options\":[\"...\",\"...\",\"...\",\"Leave\"], \"trust\":x, \"happiness\":x, \"amount\":amount}";

#[derive(Accounts)]
pub struct InitializeAgent<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,
    #[account(
        init,
        payer = payer,
        space = 8 + Agent::INIT_SPACE,
        seeds = [Agent::seed()],
        bump
    )]
    pub agent: Account<'info, Agent>,
    /// CHECK: Checked in oracle program
    #[account(mut)]
    pub llm_context: AccountInfo<'info>,
    #[account(mut)]
    pub counter: Account<'info, Counter>,
    pub system_program: Program<'info, System>,
    pub rent: Sysvar<'info, Rent>,
    /// CHECK: Checked oracle id
    #[account(address = solana_gpt_oracle::ID)]
    pub oracle_program: AccountInfo<'info>,
}

pub fn initialize_agent(ctx: Context<InitializeAgent>) -> Result<()> {
    ctx.accounts.agent.set_inner(Agent {
        context: ctx.accounts.llm_context.key(),
        ..Default::default()
    });

    // Create the context for the AI agent
    let cpi_program = ctx.accounts.oracle_program.to_account_info();
    let cpi_accounts = solana_gpt_oracle::cpi::accounts::CreateLlmContext {
        payer: ctx.accounts.payer.to_account_info(),
        context_account: ctx.accounts.llm_context.to_account_info(),
        counter: ctx.accounts.counter.to_account_info(),
        system_program: ctx.accounts.system_program.to_account_info(),
    };
    let cpi_ctx = CpiContext::new(cpi_program, cpi_accounts);
    solana_gpt_oracle::cpi::create_llm_context(cpi_ctx, AGENT_DESC.to_string())?;

    Ok(())
}
