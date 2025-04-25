use bolt_lang::*;
mod errors;

declare_id!("7nsn4z8U1nVCVHud9CLmYLy5ZHK2bSMge6u7YgmssdaA");

#[system]
pub mod claim_quest {
    use settlement::config::Resource;
    use settlement::config::QUESTS_CONFIG;
    use settlement::Settlement;

    use crate::errors::QuestClaimError;

    pub fn execute(ctx: Context<Components>, args: ClaimQuestArgs) -> Result<Components> {
        msg!("execute claim!: {}", args.index);

        let settlement = &mut ctx.accounts.settlement;

        if (settlement.quest_claim_status & (1u64 << args.index)) > 0 {
            return err!(errors::QuestClaimError::AlreadyClaimed);
        }

        // Mark quest as claimed
        settlement.quest_claim_status |= 1u64 << args.index;

        //todo quest completion checks

        // Find the quest with the given index
        let quest_opt = QUESTS_CONFIG.iter().find(|q| q.id == args.index as u32);

        if let Some(quest) = quest_opt {
            // Award the rewards based on the reward type
            match quest.reward_type {
                reward_type if reward_type == Resource::Food as u8 => {
                    settlement.treasury.food += quest.reward_value;
                }
                reward_type if reward_type == Resource::Wood as u8 => {
                    settlement.treasury.wood += quest.reward_value;
                }
                reward_type if reward_type == Resource::Water as u8 => {
                    settlement.treasury.water += quest.reward_value;
                }
                reward_type if reward_type == Resource::Stone as u8 => {
                    settlement.treasury.stone += quest.reward_value;
                }
                _ => panic!("Invalid resource type"),
            }

            Ok(ctx.accounts)
        } else {
            return err!(QuestClaimError::InvalidIndex);
        }
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct ClaimQuestArgs {
        pub index: u8,
    }
}
