mod errors;

use anchor_spl::token::spl_token;
use spl_token::state::Mint as SplMint;

use bolt_lang::*;

declare_id!("DUW1KczxcpeTEY7j9nkvcuAdWGNWoadTeDBKN5Z9xhst");

#[system]
pub mod smart_object_token_launcher_interact {
    use anchor_spl::token::{mint_to, spl_token, MintTo};
    use bolt_lang::solana_program::program_pack::Pack;
    use ecs_bundle::hero::Hero;
    use ecs_bundle::smart_object_token_launcher::SmartObjectTokenLauncher;

    pub fn execute(
        ctx: Context<Components>,
        args: SmartObjectTokenLauncherInteractionArgs,
    ) -> Result<Components> {
        let mint_account = ctx
            .mint_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("mint_account: {}", mint_account.key);

        let mint_authority = ctx
            .mint_authority()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("mint_authority: {}", mint_authority.key);

        let associated_token_account = ctx
            .associated_token_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("associated_token_account: {}", associated_token_account.key);

        let token_program = ctx
            .token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("token_program: {}", token_program.key);

        let launcher = &mut ctx.accounts.launcher;
        let hero = &mut ctx.accounts.hero;

        let mint_account_key = mint_account.key();

        let mint_data = mint_account.data.borrow();

        let mint = SplMint::unpack_unchecked(&mint_data[..])
            .map_err(|_| ProgramError::InvalidAccountData)?;
        drop(mint_data);

        msg!("SUPPLY: {}", mint.supply);

        if launcher.mint != mint_account_key {
            return err!(errors::TokenLauncherInteractError::MintAddressMismatch);
        }

        //check positive balance

        // Calculate costs
        let food_cost = args.quantity as u64 * launcher.recipe.food as u64;
        let water_cost = args.quantity as u64 * launcher.recipe.water as u64;
        let wood_cost = args.quantity as u64 * launcher.recipe.wood as u64;
        let stone_cost = args.quantity as u64 * launcher.recipe.stone as u64;

        if launcher.recipe.food > 0 {
            if (hero.backpack.food as u64) < food_cost {
                return err!(errors::TokenLauncherInteractError::NotEnoughBackpackResources);
            }
        }

        if launcher.recipe.water > 0 {
            if (hero.backpack.water as u64) < water_cost {
                return err!(errors::TokenLauncherInteractError::NotEnoughBackpackResources);
            }
        }

        if launcher.recipe.wood > 0 {
            if (hero.backpack.wood as u64) < wood_cost {
                return err!(errors::TokenLauncherInteractError::NotEnoughBackpackResources);
            }
        }

        if launcher.recipe.stone > 0 {
            if (hero.backpack.stone as u64) < stone_cost {
                return err!(errors::TokenLauncherInteractError::NotEnoughBackpackResources);
            }
        }

        //subtract

        hero.backpack.food = hero.backpack.food.wrapping_sub(food_cost as u16);
        hero.backpack.water = hero.backpack.water.wrapping_sub(water_cost as u16);
        hero.backpack.wood = hero.backpack.wood.wrapping_sub(wood_cost as u16);
        hero.backpack.stone = hero.backpack.stone.wrapping_sub(stone_cost as u16);

        // --- Hard currency transfer to PDA-controlled associated token account based on bonding curve ---
        // let payment_mint = ctx
        //     .payment_mint_account()
        //     .map_err(|_| ProgramError::InvalidAccountData)?;
        let payment_token_account = ctx
            .payment_token_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!(
            "from (payment_token_account): {}",
            payment_token_account.key()
        );
        msg!("from data len: {}", payment_token_account.data_len());

        let payment_token_authority = ctx
            .payment_token_authority()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("payment_token_authority: {}", payment_token_authority.key());

        let full_token_supply = mint.supply / 1_000_000_000;
        let base_price = 1.0;
        let coefficient = 0.05;
        let price_per_token = base_price + coefficient * (full_token_supply as f64).powi(2);
        let bonding_cost = (price_per_token * 1_000_000_000.0 * args.quantity as f64).ceil() as u64;

        let payment_token_account_data = anchor_spl::token::TokenAccount::try_deserialize(
            &mut &**payment_token_account.data.borrow(),
        )
        .map_err(|_| ProgramError::InvalidAccountData)?;

        if payment_token_account_data.amount < bonding_cost {
            return err!(errors::TokenLauncherInteractError::NotEnoughHardCurrency);
        }

        msg!(
            "Token account authority: {}",
            payment_token_account_data.owner
        );

        let payment_token_program = ctx
            .token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let destination_token_account = ctx
            .destination_token_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!(
            "to   (destination_token_account): {}",
            destination_token_account.key()
        );
        msg!("to data len: {}", destination_token_account.data_len());

        let transfer_ctx = CpiContext::new(
            payment_token_program.to_account_info(),
            anchor_spl::token::Transfer {
                from: payment_token_account.to_account_info(),
                to: destination_token_account.to_account_info(),
                authority: payment_token_authority.to_account_info(),
            },
        );

        anchor_spl::token::transfer(transfer_ctx, bonding_cost)?;

        //proceed to minting

        let (_, bump) = Pubkey::find_program_address(
            &[
                b"authority",
                mint_account_key.as_ref(), // Same seeds as macro
            ],
            ctx.program_id, // your program's id
        );

        // PDA signer seeds
        let signer_seeds: &[&[u8]] = &[
            b"authority",
            mint_account_key.as_ref(),
            &[bump], // bump always last
        ];

        // Invoke the mint_to instruction on the token program
        mint_to(
            CpiContext::new(
                token_program.to_account_info(),
                MintTo {
                    mint: mint_account.to_account_info(),
                    to: associated_token_account.to_account_info(),
                    authority: mint_authority.to_account_info(), // PDA mint authority, required as signer
                },
            )
            .with_signer(&[signer_seeds]), // using PDA to sign
            args.quantity as u64 * 10u64.pow(9 as u32),
        )?; //* 10u64.pow(mint_account.decimals as u32), // Mint tokens, adjust for decimals

        msg!("Token minted successfully.");

        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub launcher: SmartObjectTokenLauncher,
        pub hero: Hero,
    }

    #[arguments]
    struct SmartObjectTokenLauncherInteractionArgs {
        pub quantity: u16,
    }

    // These accounts are used to transfer payment tokens (hard currency) to a PDA-controlled vault as part of bonding curve pricing.
    // They are distinct from the minting token accounts used for actual token output.
    #[extra_accounts]
    pub struct ExtraAccounts {
        #[account(mut)]
        pub payer: Signer<'info>,

        #[account(
            init_if_needed,
            space=spl_token::state::Account::LEN,
            payer = payer,
            // associated_token::mint = mint_account,
            // associated_token::authority = payer,
        )]
        associated_token_account: Account<'info, TokenAccount>,

        // Create mint account
        #[account()]
        pub mint_account: Account<'info, Mint>,

        #[account(
            mut,
            seeds = [b"authority", mint_account.key().as_ref()],
            bump,
        )]
        pub mint_authority: UncheckedAccount<'info>,

        pub token_program: Program<'info, Token>,

        pub associated_token_program: Program<'info, AssociatedToken>,
        pub system_program: Program<'info, System>,

        // --- Hard currency transfer setup ---
        #[account(mut)]
        pub payment_mint_account: Account<'info, Mint>,

        #[account(mut)]
        pub payment_token_account: Account<'info, TokenAccount>,

        #[account()]
        pub payment_token_authority: Signer<'info>,

        #[account(mut)]
        pub destination_token_account: Account<'info, TokenAccount>,

        /// CHECK: Token vault PDA
        #[account(
            seeds = [b"vault"],
            bump
        )]
        pub destination_pda: UncheckedAccount<'info>,
    }
}
