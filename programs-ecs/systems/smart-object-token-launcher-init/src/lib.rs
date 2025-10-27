use bolt_lang::*;
mod errors;
use std::str::FromStr;

declare_id!("AdrPpoYr67ZcDZsQxsPgeosE3sQbZxercbUn8i1dcvap");

#[system]
pub mod smart_object_token_launcher_init {

    use anchor_spl::{
        metadata::{
            create_metadata_accounts_v3, mpl_token_metadata::types::DataV2,
            CreateMetadataAccountsV3,
        },
        token::{self, spl_token::instruction::AuthorityType, SetAuthority},
    };
    use ecs_bundle::settlement::ResourceBalance;
    use ecs_bundle::smart_object_token_launcher::SmartObjectTokenLauncher;

    pub fn execute(
        ctx: Context<Components>,
        args: SmartObjectTokenLauncherInitArgs,
    ) -> Result<Components> {
        {
            let launcher = &ctx.accounts.smart_object_token_launcher;
            if launcher.mint != Pubkey::default() {
                return err!(errors::SmartObjectTokenLauncherInitError::AlreadyInitialized);
            }
        }

        msg!("Creating metadata account");

        // Extract and clone all necessary accounts upfront
        let mint_account = ctx
            .mint_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("mint_account: {}", mint_account.key);
        let mint_authority = ctx
            .mint_authority()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("mint_authority: {}", mint_authority.key);
        let metadata_account = ctx
            .metadata_account()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("metadata_account: {}", metadata_account.key);
        let token_program = ctx
            .token_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let token_metadata_program = ctx
            .token_metadata_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;
        let system_program = ctx
            .system_program()
            .map_err(|_| ProgramError::InvalidAccountData)?;

        let payer = ctx.payer().map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("payer: {}", payer.key);

        let rent = ctx.rent().map_err(|_| ProgramError::InvalidAccountData)?;
        msg!("rent: {}", rent.key);

        // let session_token = ctx
        //     .session_token()
        //     .map_err(|_| ProgramError::InvalidAccountData)?;

        // msg!("session_token: {}", session_token.key);

        let mint_account_key = mint_account.key();

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

        msg!(
            "signer_seeds: {:?} {:?} {:?}",
            signer_seeds[0],
            signer_seeds[1],
            signer_seeds[2]
        );

        // Cross Program Invocation (CPI) signed by PDA
        // Invoking the create_metadata_account_v3 instruction on the token metadata program
        create_metadata_accounts_v3(
            CpiContext::new(
                token_metadata_program.to_account_info(),
                CreateMetadataAccountsV3 {
                    metadata: metadata_account.to_account_info(),
                    mint: mint_account.to_account_info(),
                    mint_authority: mint_authority.to_account_info(), // PDA is mint authority
                    update_authority: mint_authority.to_account_info(), // PDA is update authority
                    payer: payer.to_account_info(),
                    system_program: system_program.to_account_info(),
                    rent: rent.to_account_info(),
                },
            )
            .with_signer(&[signer_seeds]),
            DataV2 {
                name: args.token_name.clone(),
                symbol: args.token_symbol.clone(),
                uri: args.token_uri.clone(),
                seller_fee_basis_points: 0,
                creators: None,
                collection: None,
                uses: None,
            },
            true,
            true,
            None,
        )?;

        msg!("Token created successfully.");

        let (interaction_pda, _) = Pubkey::find_program_address(
            &[b"authority", mint_account_key.as_ref()],
            &Pubkey::from_str("DUW1KczxcpeTEY7j9nkvcuAdWGNWoadTeDBKN5Z9xhst").unwrap(),
        );

        token::set_authority(
            CpiContext::new(
                token_program.to_account_info(),
                SetAuthority {
                    account_or_mint: mint_account.to_account_info(), // 🎯 The Mint
                    current_authority: mint_authority.to_account_info(), // 👑 Current authority (PDA signer)
                },
            )
            .with_signer(&[signer_seeds]),
            AuthorityType::MintTokens,
            Some(interaction_pda), // the PDA derived with ProgramB::id()
        )?;

        let launcher = &mut ctx.accounts.smart_object_token_launcher;
        launcher.mint = mint_account_key;
        msg!("mint_account_key {}", mint_account_key);
        msg!("launcher.mint {}", launcher.mint);
        msg!("Set launcher mint: {}", launcher.mint);

        launcher.recipe = ResourceBalance {
            water: args.recipe_water,
            food: args.recipe_food,
            wood: args.recipe_wood,
            stone: args.recipe_stone,
        };
        msg!(
            "Set recipe: water={} food={} wood={} stone={}",
            args.recipe_water,
            args.recipe_food,
            args.recipe_wood,
            args.recipe_stone
        );
        msg!("launcher.recipe.water: {}", launcher.recipe.water);
        msg!("launcher.recipe.food: {}", launcher.recipe.food);
        msg!("launcher.recipe.wood: {}", launcher.recipe.wood);
        msg!("launcher.recipe.stone: {}", launcher.recipe.stone);
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub smart_object_token_launcher: SmartObjectTokenLauncher,
    }

    #[arguments]
    struct SmartObjectTokenLauncherInitArgs {
        pub token_name: String,
        pub token_symbol: String,
        pub token_uri: String,

        pub recipe_food: u16,
        pub recipe_water: u16,
        pub recipe_wood: u16,
        pub recipe_stone: u16,
    }

    #[extra_accounts]
    pub struct ExtraAccounts {
        #[account(mut)]
        pub payer: Signer<'info>,

        // Create mint account
        #[account()]
        pub mint_account: Account<'info, Mint>,

        /// CHECK: Validate address by deriving pda
        #[account(
        mut,
        seeds = [b"metadata", token_metadata_program.key().as_ref(), mint_account.key().as_ref()],
        bump)]
        pub metadata_account: UncheckedAccount<'info>,

        /// CHECK: Validate address by deriving pda
        #[account(
            mut,
            seeds = [b"authority", mint_account.key().as_ref()],
            bump,
        )]
        pub mint_authority: UncheckedAccount<'info>,

        pub token_program: Program<'info, Token>,
        pub token_metadata_program: Program<'info, Metadata>,
        pub system_program: Program<'info, System>,
        pub rent: Sysvar<'info, Rent>,
    }
}
