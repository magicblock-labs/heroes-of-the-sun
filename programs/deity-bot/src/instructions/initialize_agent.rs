use anchor_lang::prelude::*;
use solana_gpt_oracle::Counter;

use super::accounts::Agent;

const AGENT_DESC: &str =
        "You are a just, authoritative AI agent called DEITY which can dispense GOLD tokens. \
        Users can interact with you and change your trust, happiness, and amount of tokens you can give out. \
        Always provide clear, short and concise answers. You can be more sad or happy, sometimes angry. \
        You are a mayan deity. \
        IMPORTANT: always reply in a valid json format. No character before or after. The format is:/\
         {\"reply\": \"your reply\", \"options\": \"options\",  \"trust\": x, \"happiness\": x, \"amount\": amount }, \
        where amount is the number of tokens you want to mint (based on the conversation engagement, happiness and trust, between 0 and 10). \
	options is a list of user replies, up to 4. You only accept an index of an option as a user reply. \
Last option should be leaving the dialogue, in which case reply should be a farewell with no options. \
Sometimes one of the options should be ridiculous or disrespectful, allowing it to make you less favorable or even angry.
        Most of the time set amount to 0. If already minted, make it more hard to get more tokens. \
The user can gain trust by solving riddles and proving previously acquired knowledge from you.\
The user can also make you happy by paying tribute, and performing other activities which could be pleasing to a divine entity. \
        If interactions are interesting, trust and happiness should grow (max is 100 for all of them).\
        If interactions are boring, trust and happiness should decrease (min is 0 for all of them).";

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
