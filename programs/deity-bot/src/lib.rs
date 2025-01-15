use anchor_lang::prelude::*;
use instructions::*;
pub mod instructions;

declare_id!("62f9zAUjCN5VFqWF43qSUrW6CvivqhsEjDvCHwQ1SjgR");

#[program]
pub mod deity_bot {
    use super::*;

    pub fn initialise_agent(ctx: Context<InitializeAgent>) -> Result<()> {
        initialize_agent::initialize_agent(ctx)
    }

    pub fn interact_agent(ctx: Context<InteractAgent>, option: u8) -> Result<()> {
        interact_agent::interact_agent(ctx, option)
    }

    pub fn callback_agent(ctx: Context<CallbackFromAgent>, response: String) -> Result<()> {
        callback_agent::callback_from_agent(ctx, response)
    }
}
