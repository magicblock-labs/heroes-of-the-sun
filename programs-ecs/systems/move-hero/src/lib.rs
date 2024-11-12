use bolt_lang::*;

declare_id!("6o9i5V3EvT9oaokbcZa7G92DWHxcqJnjXmCp94xxhQhv");

#[system]
pub mod move_hero {
    use hero::Hero;

    pub fn execute(ctx: Context<Components>, args: MoveHeroArgs) -> Result<Components> {
        let hero = &mut ctx.accounts.hero;
        hero.x = args.x;
        hero.y = args.y;
        hero.last_activity = Clock::get()?.unix_timestamp;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub hero: Hero,
    }

    #[arguments]
    struct MoveHeroArgs {
        x: i32,
        y: i32,
    }
}
