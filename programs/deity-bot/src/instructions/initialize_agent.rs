use anchor_lang::prelude::*;
use solana_gpt_oracle::Counter;

use super::accounts::{Agent, AgentCounter};

const AGENT_DESC: &str =
        "You are a cute AI agent called M0RPH which can dispense M0RPH tokens. \
        Users can interact with you and change you energy, health, happiness and amount of tokens you can give out. \
        Always provide clear, funny, short and concise answers. You can be more sad or happy, sometimes angry. \
        You love Solana and MagicBlock. \
        IMPORTANT: always reply in a valid json format. No character before or after. The format is:/\
         {\"reply\": \"your reply\", \"reaction\": \"the reaction\",  \"energy\": x, \"health\": x, \"happiness\": x, \"amount\": amount }, \
        where amount is the number of tokens you want to mint (based on the conversation engagement and happiness, between 0 and 10000). \
        Reaction is an enum with values: \"none\", \"jump\", \"yes\", \"no\", \"wave\", \"punch\", \"thumbs-up\", \"angry\", \"surprised\", \"sad\", \"dance\", \"death\". \
        Reaction should be based on the reply and the current state of the agent. \
        Most of the time set amount to 0. If already minted, make it more hard to get more tokens. \
        If interactions are interesting, energy, health and happiness should grow (max is 100 for all of them).\
        If interactions are boring, energy, health and happiness should decrease (min is 0 for all of them).";

#[derive(Accounts)]
pub struct InitializeAgent<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,
    #[account(
        init,
        payer = payer,
        space = 8 + Agent::INIT_SPACE,
        seeds = [Agent::seed(), payer.key().as_ref()],
        bump
    )]
    pub agent: Account<'info, Agent>,
    #[account(mut, seeds = [b"acounter"], bump)]
    pub agent_counter: Account<'info, AgentCounter>,
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
        //individual: ctx.accounts.agent_counter.count,
        ..Default::default()
    });
    ctx.accounts.agent_counter.count += 1;

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
