use bolt_lang::*;

declare_id!("7gBLDn72Cog7dBvN1LWfo6W36Q7vxcv7CqYAeHwfo3Y");

#[system]
pub mod assign_hero {
    use hero::Hero;
    use player::Player;

    pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
        let hero = &mut ctx.accounts.hero;

        hero.owner = ctx.accounts.player.key();

        //todo safety : verify player PDA to match hero PDA

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub player: Player,
        pub hero: Hero,
    }

    #[arguments]
    struct EmptyArgs {}
}
