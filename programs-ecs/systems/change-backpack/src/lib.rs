mod errors;
use bolt_lang::*;

declare_id!("97qK4zBtZbSGT1mSw5mn12hfHgz4jV4C7cLmwSzH2eua");

const CAPACITY: u16 = 5;

#[system]
pub mod change_backpack {
    use hero::Hero;
    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, args: ChangeBackpackArgs) -> Result<Components> {
        let hero = &mut ctx.accounts.hero;
        let settlement = &mut ctx.accounts.settlement;
        //check owners

        if hero.owner != settlement.owner {
            return err!(errors::BackpackError::OwnerMismatch);
        }

        //check positive balance

        if args.food < 0 {
            if hero.backpack.food < -args.food as u16 {
                return err!(errors::BackpackError::NotEnoughBackpackResources);
            }
        }

        if args.food > 0 {
            if settlement.treasury.food < args.food as u16 {
                return err!(errors::BackpackError::NotEnoughSettlementResources);
            }
            if hero.backpack.food + args.food as u16 > CAPACITY {
                return err!(errors::BackpackError::NotEnoughBackpackCapacity);
            }
        }

        if args.water < 0 {
            if hero.backpack.water < -args.water as u16 {
                return err!(errors::BackpackError::NotEnoughBackpackResources);
            }
        }

        if args.water > 0 {
            if settlement.treasury.water < args.water as u16 {
                return err!(errors::BackpackError::NotEnoughSettlementResources);
            }
            if hero.backpack.water + args.water as u16 > CAPACITY {
                return err!(errors::BackpackError::NotEnoughBackpackCapacity);
            }
        }

        if args.wood < 0 {
            if hero.backpack.wood < -args.wood as u16 {
                return err!(errors::BackpackError::NotEnoughBackpackResources);
            }
        }

        if args.wood > 0 {
            if settlement.treasury.wood < args.wood as u16 {
                return err!(errors::BackpackError::NotEnoughSettlementResources);
            }
            if hero.backpack.wood + args.wood as u16 > CAPACITY {
                return err!(errors::BackpackError::NotEnoughBackpackCapacity);
            }
        }

        if args.stone < 0 {
            if hero.backpack.stone < -args.stone as u16 {
                return err!(errors::BackpackError::NotEnoughBackpackResources);
            }
        }

        if args.stone > 0 {
            if settlement.treasury.stone < args.stone as u16 {
                return err!(errors::BackpackError::NotEnoughSettlementResources);
            }
            if hero.backpack.stone + args.stone as u16 > CAPACITY {
                return err!(errors::BackpackError::NotEnoughBackpackCapacity);
            }
        }

        //transfer
        hero.backpack.food = hero.backpack.food.wrapping_add_signed(args.food);
        settlement.treasury.food = settlement.treasury.food.wrapping_add_signed(-args.food);

        hero.backpack.water = hero.backpack.water.wrapping_add_signed(args.water);
        settlement.treasury.water = settlement.treasury.water.wrapping_add_signed(-args.water);

        hero.backpack.wood = hero.backpack.wood.wrapping_add_signed(args.wood);
        settlement.treasury.wood = settlement.treasury.wood.wrapping_add_signed(-args.wood);

        hero.backpack.stone = hero.backpack.stone.wrapping_add_signed(args.stone);
        settlement.treasury.stone = settlement.treasury.stone.wrapping_add_signed(-args.stone);

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub hero: Hero,
        pub settlement: Settlement,
    }

    #[arguments]
    struct ChangeBackpackArgs {
        food: i16,
        water: i16,
        wood: i16,
        stone: i16,
    }
}
