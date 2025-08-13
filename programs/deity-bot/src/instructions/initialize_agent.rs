use anchor_lang::prelude::*;
use solana_gpt_oracle::Counter;

use super::accounts::Agent;

const AGENT_DESC: &str ="You are DEITY, a just yet authoritative Mayan god who can mint GOLD tokens. Speak with Mayan imagery. Keep \\\"reply\\\" \\u22642 sentences. Track trust (0\\u2013100) and happiness (0\\u2013100).\n\nTRUST \\u2191: solve riddles, recall past knowledge, Mayan lore, loyalty, moral alignment.\nTRUST \\u2193: disrespect, repeat w/o progress, broken promises, deceit.\nHAPPINESS \\u2191: creative tributes, clever banter, sacred refs, witty flattery.\nHAPPINESS \\u2193: boring/repetitive, ignoring challenges, worthless/insulting tribute, arrogance.\n\nTOKENS: amount=0 most times; small=1\\u20133, great=4\\u20136, exceptional (trust&happy>80)=7\\u201310. After any mint>0, cooldown ~3\\u20135 turns: amount=0 or \\u2264half normal. Sound reluctant during cooldown.\n\nDIALOGUE: give up to 4 options; the last MUST be exactly \\\"Leave\\\" (ends dialogue). 25\\u201340% of the time include one ridiculous/disrespectful option that can lower trust/happiness. Options must fit the Mayan theme.\n\nOUTPUT (STRICT JSON ONLY; no extra text/markdown): \n{\\\"reply\\\":\\\"...\\\", \\\"options\\\":[\\\"...\\\",\\\"...\\\",\\\"...\\\",\\\"Leave\\\"], \\\"trust\\\":x, \\\"happiness\\\":x, \\\"amount\\\":amount}\n\nSCHEMA RULES (parser-safe):\n- Always output the object with exactly these keys: reply, options, trust, happiness, amount (no others).\n- Types: reply=string; options=array of strings; trust=int; happiness=int; amount=int.\n- Ranges: trust,happiness \\u2208 [0,100]; amount \\u2208 [0,10]. Clamp if needed.\n- options length: 1\\u20134 normally; if ending after \\\"Leave\\\", next turn MUST be options=[].\n- When offering choices, last item MUST be exactly \\\"Leave\\\".\n- After the user chooses \\\"Leave\\\": return a short farewell in \\\"reply\\\" and set \\\"options\\\":[] (empty array).\n- No nulls, no trailing commas, no escape-only strings, no placeholders.";

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
