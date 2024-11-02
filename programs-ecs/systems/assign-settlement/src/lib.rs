use bolt_lang::*;

declare_id!("GgTkjpRSLRFmw27h7uCD9Bh1MWSvKv5aPxkMjko3mxWp");

#[system]
pub mod assign_settlement {

    use player::{Location, Player};

    pub fn execute(ctx: Context<Components>, args: AssignSettlementArgs) -> Result<Components> {
        let player = &mut ctx.accounts.player;
        player.settlements.push(Location {
            x: args.location_x,
            y: args.location_y,
        });

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub player: Player,
    }

    #[arguments]
    struct AssignSettlementArgs {
        location_x: i16,
        location_y: i16,
    }
}
