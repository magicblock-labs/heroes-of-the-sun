use bolt_lang::*;

declare_id!("At6YLXTGPoQPwqzfZwYEq667RBgAX3sJFiNUALgbpQan");

const DIRECTION_UP: u8 = 0;
const DIRECTION_RIGHT: u8 = 1;
const DIRECTION_DOWN: u8 = 2;
const DIRECTION_LEFT: u8 = 3;

#[system]
pub mod bump_location_allocator {
    use location_allocator::LocationAllocator;

    pub fn execute(ctx: Context<Components>, _args: NoArgs) -> Result<Components> {
        let location = &mut ctx.accounts.location_allocator;

        match location.direction {
            DIRECTION_UP => {
                location.current_y += 1;
                if location.current_y > -location.current_x {
                    location.direction = DIRECTION_RIGHT;
                }
            }
            DIRECTION_RIGHT => {
                location.current_x += 1;
                if location.current_x >= location.current_y {
                    location.direction = DIRECTION_DOWN;
                }
            }
            DIRECTION_DOWN => {
                location.current_y -= 1;
                if location.current_y <= -location.current_x {
                    location.direction = DIRECTION_LEFT;
                }
            }
            DIRECTION_LEFT => {
                location.current_x -= 1;
                if location.current_x <= location.current_y {
                    location.direction = DIRECTION_UP;
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
    struct NoArgs {}
}
