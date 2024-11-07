use bolt_lang::*;

declare_id!("42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2");

#[system]
pub mod assign_settlement {

    use location_allocator::LocationAllocator;
    use player::{Location, Player};
    use settlement::Settlement;

    const DIRECTION_UP: u8 = 0;
    const DIRECTION_RIGHT: u8 = 1;
    const DIRECTION_DOWN: u8 = 2;
    const DIRECTION_LEFT: u8 = 3;

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let player = &mut ctx.accounts.player;
        let settlement = &mut ctx.accounts.settlement;
        let location_allocator = &mut ctx.accounts.location_allocator;

        player.settlements.push(Location {
            x: location_allocator.current_x,
            y: location_allocator.current_y,
        });

        match location_allocator.direction {
            DIRECTION_UP => {
                location_allocator.current_y += 1;
                if location_allocator.current_y > -location_allocator.current_x {
                    location_allocator.direction = DIRECTION_RIGHT;
                }
            }
            DIRECTION_RIGHT => {
                location_allocator.current_x += 1;
                if location_allocator.current_x >= location_allocator.current_y {
                    location_allocator.direction = DIRECTION_DOWN;
                }
            }
            DIRECTION_DOWN => {
                location_allocator.current_y -= 1;
                if location_allocator.current_y <= -location_allocator.current_x {
                    location_allocator.direction = DIRECTION_LEFT;
                }
            }
            DIRECTION_LEFT => {
                location_allocator.current_x -= 1;
                if location_allocator.current_x <= location_allocator.current_y {
                    location_allocator.direction = DIRECTION_UP;
                }
            }
            4_u8..=u8::MAX => {
                panic!("invalid direction!")
            }
        }

        settlement.owner = ctx.accounts.player.key();

        //todo safety : verify settlement PDA with x/y extra seed

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub player: Player,
        pub settlement: Settlement,
        pub location_allocator: LocationAllocator,
    }

    #[arguments]
    struct EmptyArgs {}
}
