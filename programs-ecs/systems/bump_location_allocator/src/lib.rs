use bolt_lang::*;

declare_id!("C2H1sb7ZVpgEZFWqXujRK3rx5C2543GNN251wmgfbhUH");

#[system]
pub mod bump_location_allocator {

    use location_allocator::LocationAllocator;

    const DIRECTION_UP: u8 = 0;
    const DIRECTION_RIGHT: u8 = 1;
    const DIRECTION_DOWN: u8 = 2;
    const DIRECTION_LEFT: u8 = 3;

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let location_allocator = &mut ctx.accounts.location_allocator;

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

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub location_allocator: LocationAllocator,
    }

    #[arguments]
    struct EmptyArgs {}
}
