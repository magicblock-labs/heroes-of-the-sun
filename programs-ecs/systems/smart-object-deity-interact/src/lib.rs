use bolt_lang::*;
mod errors;

declare_id!("2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB");

#[system]
pub mod smart_object_deity_interact {
    use hero::Hero;
    use smart_object_deity::{SmartObjectDeity, COOLDOWN};

    pub fn execute(ctx: Context<Components>, args: InteractionArgs) -> Result<Components> {
        let hero = &mut ctx.accounts.hero;
        let deity = &mut ctx.accounts.deity;

        let clock = Clock::get();
        let mut now = 0;

        if clock.is_ok() {
            now = clock.unwrap().unix_timestamp
        }

        if deity.state == 0 && now < deity.next_interaction_time {
            return err!(errors::SmartObjectDeityInteractionError::OnCooldown);
        }

        //todo replace with generated dialogue based on component state and user input

        if deity.state == 0 {
            deity.state = 1;
        } else if deity.state == 1 {
            if args.index == 0 {
                deity.state = 2;
            }
            if args.index == 1 {
                deity.state = 3;
            }
            if args.index == 3 {
                deity.state = 0;
            }
        } else if deity.state == 2 {
            deity.state = 0;
        } else if deity.state == 3 {
            if args.index == 1 {
                hero.backpack.food -= 1;
                hero.backpack.stone += 1;
            }
            deity.state = 0;
        }

        //interaction reset
        if deity.state == 0 {
            deity.next_interaction_time = now + COOLDOWN;
        }

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub deity: SmartObjectDeity,
        pub hero: Hero,
    }

    #[arguments]
    struct InteractionArgs {
        pub index: u8,
    }
}
