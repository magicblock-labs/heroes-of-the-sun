use bolt_lang::*;
mod errors;

declare_id!("4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf");

#[system]
pub mod claim_loot {
    use hero::Hero;
    use loot_distribution::LootDistribution;
    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, args: ClaimLootArgs) -> Result<Components> {
        let loot = &mut ctx.accounts.loot;
        let hero = &mut ctx.accounts.hero;
        let settlement = &mut ctx.accounts.settlement;

        let loot_loc = loot.loots[args.index as usize];
        if loot_loc.x != hero.x || loot_loc.y != hero.y {
            return err!(errors::ClaimLootError::LocationMismatch);
        }

        if hero.owner != settlement.owner {
            return err!(errors::ClaimLootError::OwnersMismatch);
        }

        settlement.treasury.wood += 5;
        loot.index += 1;
        loot.loots[args.index as usize] = loot_distribution::get_loot_location(loot.index);

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub loot: LootDistribution,
        pub hero: Hero,
        pub settlement: Settlement,
    }

    #[arguments]
    struct ClaimLootArgs {
        index: u8,
    }
}
