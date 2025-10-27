use bolt_lang::*;

pub mod settlement;
pub mod hero;
pub mod player;
pub mod location_allocator;
pub mod loot_distribution;
pub mod smart_object_deity;
pub mod smart_object_location;
pub mod smart_object_token_launcher;

pub mod systems;

declare_id!("Cjca6tWWGx77ki6rRinErcoZJEvJHNxfPaut8DoKJHQ5");

#[bundle]
pub mod ecs_bundle {
	use crate::settlement::{Building, EnvironmentState, ResourceBalance};
	use crate::player::Location;
	use crate::loot_distribution::LootLocation;

	#[component(delegate)]
	pub struct SmartObjectTokenLauncher {
		pub system: Pubkey,
		pub mint: Pubkey,
		pub recipe: ResourceBalance,
	}

	#[component(delegate)]
	#[derive(Default)]
	pub struct LocationAllocator {
		pub current_x: i16,
		pub current_y: i16,
		pub direction: u8,
	}	

	#[component(delegate)]
	#[derive(Default)]
	pub struct SmartObjectLocation {
		pub x: i32,
		pub y: i32,
		pub entity: Pubkey,
	}

	#[component(delegate)]
	pub struct SmartObjectDeity {
		pub system: Pubkey,
	}	

	#[component(delegate)]
	pub struct LootDistribution {
		pub index: i32,
	
		#[max_len(100)]
		pub loots: Vec<LootLocation>,
	}	

	#[component(delegate)]
	pub struct Settlement {
		#[max_len(20, 6)]
		pub buildings: Vec<Building>,
		pub owner: Pubkey,

		pub environment: EnvironmentState,
		pub treasury: ResourceBalance,

		pub faith: u8,
		pub time_units: u8,
		pub last_time_claim: i64,
		pub research: u32,

		#[max_len(30, 1)]
		pub worker_assignment: Vec<i8>, //index is worker unit index, value is building index from /buildings/ array; singed: use -1 for free slot
		pub quest_claim_status: u64,
	}

	#[component(delegate)]
	#[derive(Default)]
	pub struct Hero {
		pub x: i32,
		pub y: i32,
		pub last_activity: i64,
		pub owner: Pubkey,
		pub backpack: ResourceBalance,
	}

	#[component(delegate)]
	pub struct Player {
		#[max_len(5)]
		pub settlements: Vec<Location>,
	}

	#[system]
	pub mod assign_hero {
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
	}

	#[system]
	pub mod assign_settlement {
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

	#[system]
	pub mod assign_worker {
		use crate::systems::assign_worker::AssignWorkerError;

		pub fn execute(ctx: Context<Components>, args: AssignLabourArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			if settlement.buildings.len() <= args.building_index as usize {
				return err!(AssignWorkerError::BuildingIndexOutOfRange);
			}
	
			if settlement.buildings[0].level <= args.worker_index {
				return err!(AssignWorkerError::WorkerIndexOutOfRange);
			}
	
			if settlement.worker_assignment[args.worker_index as usize] < -1 {
				return err!(AssignWorkerError::NotRestoredYet);
			}
	
			settlement.worker_assignment[args.worker_index as usize] = args.building_index;
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct AssignLabourArgs {
			worker_index: u8,
			building_index: i8,
		}
	}
	
	#[system]
	pub mod build {
        use crate::{settlement::config::{get_build_time, get_construction_cost, get_extraction_cap, BUILDINGS_CONFIG, MAP_HEIGHT, MAP_WIDTH}, systems::build::{fits, BuildError}};

		pub fn execute(ctx: Context<Components>, args: BuildArgs) -> Result<Components> {
			if args.config_index as usize >= BUILDINGS_CONFIG.len() {
				return err!(BuildError::ConfigIndexOutOfRange);
			}

			let new_building_config = &BUILDINGS_CONFIG[args.config_index as usize];

			//check map bounds
			if args.x + new_building_config.width >= MAP_WIDTH {
				return err!(BuildError::OutOfBounds);
			}

			if args.y + new_building_config.height >= MAP_HEIGHT {
				return err!(BuildError::OutOfBounds);
			}

			let settlement = &mut ctx.accounts.settlement;
			if !fits(settlement, args.x, args.y, new_building_config) {
				return err!(BuildError::WontFit);
			}

			let build_cost =
				get_construction_cost(settlement.research, new_building_config.cost_tier, 1, 1.0);

			if settlement.treasury.wood < build_cost.wood
				|| settlement.treasury.water < build_cost.water
				|| settlement.treasury.food < build_cost.food
				|| settlement.treasury.stone < build_cost.stone
			{
				return err!(BuildError::NotEnoughResources);
			} else {
				settlement.treasury.wood -= build_cost.wood;
				settlement.treasury.water -= build_cost.water;
				settlement.treasury.food -= build_cost.food;
				settlement.treasury.stone -= build_cost.stone;
			}

			let new_building = Building {
				x: args.x,
				y: args.y,
				id: new_building_config.id,
				deterioration: 0,
				level: 0,
				turns_to_build: get_build_time(
					settlement.research,
					new_building_config.build_time_tier,
					1,
				),
				extraction: get_extraction_cap(0),
			};

			settlement.buildings.push(new_building);

			if args.worker_index >= 0 {
				if settlement.worker_assignment.len() as i16 <= args.worker_index {
					return err!(BuildError::SuppliedWorkerIndexOutOfBounds);
				}
				settlement.worker_assignment[args.worker_index as usize] =
					(settlement.buildings.len() - 1) as i8;
			}

			Ok((ctx.accounts))
		}

		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}

		#[arguments]
		struct BuildArgs {
			config_index: u8,
			worker_index: i16,
			x: u8,
			y: u8,
		}
	}

	#[system]
	pub mod bump_location_allocator {
		pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
	
	//todo safety verify current slot is actally occupied so you dont bump endleslly
	
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
	}

	#[system]
	pub mod change_backpack {
	    use crate::systems::change_backpack::BackpackError;
		const CAPACITY: u16 = 5;
	
		pub fn execute(ctx: Context<Components>, args: ChangeBackpackArgs) -> Result<Components> {
			let hero = &mut ctx.accounts.hero;
			let settlement = &mut ctx.accounts.settlement;
			//check owners
	
			if hero.owner != settlement.owner {
				return err!(BackpackError::OwnerMismatch);
			}
	
			//todo: tbc do we want to be near settlement for exchange?
	
			//check positive balance
	
			if args.food < 0 {
				if hero.backpack.food < -args.food as u16 {
					return err!(BackpackError::NotEnoughBackpackResources);
				}
			}
	
			if args.food > 0 {
				if settlement.treasury.food < args.food as u16 {
					return err!(BackpackError::NotEnoughSettlementResources);
				}
				if hero.backpack.food + args.food as u16 > CAPACITY {
					return err!(BackpackError::NotEnoughBackpackCapacity);
				}
			}
	
			if args.water < 0 {
				if hero.backpack.water < -args.water as u16 {
					return err!(BackpackError::NotEnoughBackpackResources);
				}
			}
	
			if args.water > 0 {
				if settlement.treasury.water < args.water as u16 {
					return err!(BackpackError::NotEnoughSettlementResources);
				}
				if hero.backpack.water + args.water as u16 > CAPACITY {
					return err!(BackpackError::NotEnoughBackpackCapacity);
				}
			}
	
			if args.wood < 0 {
				if hero.backpack.wood < -args.wood as u16 {
					return err!(BackpackError::NotEnoughBackpackResources);
				}
			}
	
			if args.wood > 0 {
				if settlement.treasury.wood < args.wood as u16 {
					return err!(BackpackError::NotEnoughSettlementResources);
				}
				if hero.backpack.wood + args.wood as u16 > CAPACITY {
					return err!(BackpackError::NotEnoughBackpackCapacity);
				}
			}
	
			if args.stone < 0 {
				if hero.backpack.stone < -args.stone as u16 {
					return err!(BackpackError::NotEnoughBackpackResources);
				}
			}
	
			if args.stone > 0 {
				if settlement.treasury.stone < args.stone as u16 {
					return err!(BackpackError::NotEnoughSettlementResources);
				}
				if hero.backpack.stone + args.stone as u16 > CAPACITY {
					return err!(BackpackError::NotEnoughBackpackCapacity);
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

	#[system]
	pub mod claim_loot {
		use ecs_bundle::loot_distribution;
		use token_minter::cpi::accounts::MintToken;
	
		pub fn execute(ctx: Context<Components>, args: ClaimLootArgs) -> Result<Components> {
			// Extract and clone all necessary accounts upfront
			let minter_program = ctx
				.minter_program()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			let mint_account = ctx
				.mint_account()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			msg!("mint_account: {}", mint_account.key);
			let associated_token_account = ctx
				.associated_token_account()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			msg!("associated_token_account: {}", associated_token_account.key);
			let token_program = ctx
				.token_program()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			msg!("token_program: {}", token_program.key);
			let associated_token_program = ctx
				.associated_token_program()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			msg!("associated_token_program: {}", associated_token_program.key);
			let system_program = ctx
				.system_program()
				.map_err(|_| ProgramError::InvalidAccountData)?;
			msg!("system_program: {}", system_program.key);
			let payer = ctx.signer().map_err(|_| ProgramError::InvalidAccountData)?;
	
			msg!("payer: {}", payer.key);
	
			let session_token = ctx.session_token().map(|opt| opt.to_account_info()).ok();
	
			let loot = &mut ctx.accounts.loot;
			// let hero = &mut ctx.accounts.hero;
	
			// let loot_loc = loot.loots[args.index as usize];
	
			//todo commit hero location before comparing to loot location?
			// if loot_loc.x != hero.x || loot_loc.y != hero.y {
			//     return err!(errors::ClaimLootError::LocationMismatch);
			// }
	
			let res = token_minter::cpi::mint_token(
				CpiContext::new(
					minter_program.clone(),
					MintToken {
						payer: payer.clone(),
						mint_account: mint_account.clone(),
						associated_token_account: associated_token_account.clone(),
						token_program: token_program.clone(),
						associated_token_program: associated_token_program.clone(),
						system_program: system_program.clone(),
						session_token: session_token,
					},
				),
				1 as u64,
			);
	
			if res.is_ok() {
				loot.index += 1;
				loot.loots[args.index as usize] = loot_distribution::get_loot_location(loot.index);
			}
	
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub loot: LootDistribution,
		}
	
		#[arguments]
		struct ClaimLootArgs {
			index: u8,
		}
	
		#[extra_accounts]
		pub struct ExtraAccounts {
			#[account(mut)]
			signer: Signer<'info>,
	
			#[account()]
			associated_token_account: Account<'info, TokenAccount>,
	
			#[account()]
			mint_account: Account<'info, Mint>,
	
			#[account()]
			minter_program: AccountInfo,
	
			#[account()]
			token_program: Program<'info, Token>,
	
			#[account()]
			associated_token_program: Program<'info, AssociatedToken>,
	
			#[account()]
			system_program: Program<'info, System>,
			#[account()]
			pub session_token: Option<Account<'info, SessionToken>>,
		}
	}

	#[system]
	pub mod claim_quest {
		use crate::settlement::config::get_quest_progress;
		use crate::settlement::config::Resource;
		use crate::settlement::config::QUESTS_CONFIG;
		use crate::systems::claim_quest::QuestClaimError;
	
		pub fn execute(ctx: Context<Components>, args: ClaimQuestArgs) -> Result<Components> {
			msg!("execute claim!: {}", args.index);
	
			let settlement = &mut ctx.accounts.settlement;
	
			if (settlement.quest_claim_status & (1u64 << args.index)) > 0 {
				return err!(QuestClaimError::AlreadyClaimed);
			}
	
			// Mark quest as claimed
			settlement.quest_claim_status |= 1u64 << args.index;
	
			// Find the quest with the given index
			let quest_opt = QUESTS_CONFIG.iter().find(|q| q.id == args.index as u32);
	
			if let Some(quest) = quest_opt {
				// quest completion checks
				let progress = get_quest_progress(
					settlement.buildings.clone(),
					settlement.treasury,
					settlement.faith,
					settlement.research,
					quest,
				);
	
				msg!("progress!: {}", progress);
	
				if progress < quest.target_value as u32 {
					return err!(QuestClaimError::TargetNotReached);
				}
	
				// Award the rewards based on the reward type
				match quest.reward_type {
					reward_type if reward_type == Resource::Food as u8 => {
						settlement.treasury.food += quest.reward_value;
					}
					reward_type if reward_type == Resource::Wood as u8 => {
						settlement.treasury.wood += quest.reward_value;
					}
					reward_type if reward_type == Resource::Water as u8 => {
						settlement.treasury.water += quest.reward_value;
					}
					reward_type if reward_type == Resource::Stone as u8 => {
						settlement.treasury.stone += quest.reward_value;
					}
					_ => panic!("Invalid resource type"),
				}
	
				Ok(ctx.accounts)
			} else {
				return err!(QuestClaimError::InvalidIndex);
			}
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct ClaimQuestArgs {
			pub index: u8,
		}
	}	

	#[system]
	pub mod claim_time {
		use crate::settlement::{
			config::{self, get_research_level, ResearchType},
		};
		const SECONDS_IN_MINUTE: i64 = 60;
	
		pub fn execute(ctx: Context<Components>, _args: EmptyArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			let faith = (settlement.faith
				+ config::FAITH_BONUS_RESEARCH_MULTIPLIER
					* get_research_level(settlement.research, ResearchType::FaithBonus))
				as u16;
	
			msg!("faith {}", faith);
	
			let cap: u8 = config::BASE_ENERGY_CAP
				+ (faith as f32 * config::ENERGY_CAP_FAITH_MULTIPLIER) as u8
				+ (config::MAX_ENERGY_CAP_RESEARCH_MULTIPLIER
					* get_research_level(settlement.research, ResearchType::MaxEnergyCap)); //[31..22] + research
	
			msg!("cap {}", cap);
	
			let s_per_unit: i64 = SECONDS_IN_MINUTE
				* (config::BASE_MINUTE_PER_ENERGY_UNIT
					- (config::ENERGY_REGENERATION_RESEARCH_MULTIPLIER
						* get_research_level(settlement.research, ResearchType::EnergyRegeneration))
						as i64
					- (faith as f32 * config::ENERGY_REGEN_FAITH_MULTIPLIER) as i64); //[10..4] - research min per time unit
	
			msg!("s_per_unit {}", s_per_unit);
	
			let now = Clock::get()?.unix_timestamp;
	
			let time_passed: i64 = now - settlement.last_time_claim;
			msg!(
				"now, last_time_claim, time_passed {} {} {}",
				now,
				settlement.last_time_claim,
				time_passed
			);
	
			if cap > settlement.time_units {
				let claimable = u8::min(
					(time_passed / s_per_unit as i64) as u8,
					cap - settlement.time_units,
				);
	
				msg!("claimable {}", claimable);
	
				if claimable > 0 {
					settlement.time_units += claimable;
					settlement.last_time_claim += claimable as i64 * s_per_unit;
				}
			} else {
				settlement.last_time_claim = now;
			}
	
			msg!(
				"new time_units&last_time_claim {} {}",
				settlement.time_units,
				settlement.last_time_claim
			);
	
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	}		

	#[system]
	pub mod move_hero {
		pub fn execute(ctx: Context<Components>, args: MoveHeroArgs) -> Result<Components> {
	
			//todo min cooldown?
	
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

	#[system]
	pub mod repair {
	    use crate::systems::repair::RepairError;

		pub fn execute(ctx: Context<Components>, args: RepairArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			if args.index as usize >= settlement.buildings.len() {
				return err!(RepairError::BuildingIndexOutOfRange);
			}
	
			let building = settlement.buildings[args.index as usize];
			let building_config = &BUILDINGS_CONFIG[building.id as usize];
	
			let max_deterioration = config::BASE_DETERIORATION_CAP
				+ config::DETERIORATION_CAP_RESEARCH_MULTIPLIER
					* get_research_level(settlement.research, ResearchType::DeteriorationCap);
	
			let build_cost = get_construction_cost(
				settlement.research,
				building_config.cost_tier,
				building.level,
				building.deterioration as f32 / max_deterioration as f32,
			);
	
			if settlement.treasury.wood < build_cost.wood
				|| settlement.treasury.water < build_cost.water
				|| settlement.treasury.food < build_cost.food
				|| settlement.treasury.stone < build_cost.stone
			{
				return err!(RepairError::NotEnoughResources);
			} else {
				settlement.treasury.wood -= build_cost.wood;
				settlement.treasury.water -= build_cost.water;
				settlement.treasury.food -= build_cost.food;
				settlement.treasury.stone -= build_cost.stone;
			}
	
			//all checks passed
			settlement.buildings[args.index as usize].deterioration = 0;
	
			Ok((ctx.accounts))
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct RepairArgs {
			index: u8,
		}
	}	

	#[system]
	pub mod research {
		use crate::{settlement::config::{get_research_level_u8, BITS_PER_RESEARCH, RESEARCH_MASK}, systems::research::ResearchError};
	
		pub fn execute(ctx: Context<Components>, args: ResearchArgs) -> Result<Components> {
			msg!("execute research!: ");
	
			let settlement = &mut ctx.accounts.settlement;
	
			//would be nice to get the size_of(settlement.research) though
			if 32 <= (args.research_type * BITS_PER_RESEARCH) as usize {
				return err!(ResearchError::ResearchIndexOutOfRange);
			}
	
			let mut research_level = get_research_level_u8(settlement.research, args.research_type);
	
			let research_cost = config::get_research_cost(args.research_type, research_level);
	
			if settlement.treasury.stone < research_cost {
				return err!(ResearchError::NotEnoughResources);
			}
	
			if research_level >= RESEARCH_MASK {
				return err!(ResearchError::AlreadyMaxedOut);
			}
	
			settlement.treasury.stone -= research_cost;
			research_level += 1;
	
			let mut research_value = settlement.research;
	
			//cut out old value
			research_value &= !((RESEARCH_MASK as u32) << BITS_PER_RESEARCH * args.research_type);
	
			//replace with new one
			research_value |= (research_level as u32) << BITS_PER_RESEARCH * args.research_type;
	
			settlement.research = research_value;
			// settlement.treasury.gold -= research_cost;
	
			if args.research_type == ResearchType::ExtraUnit as u8 {
				settlement.worker_assignment.push(-1);
			}
	
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct ResearchArgs {
			pub research_type: u8,
		}
	}	

	#[system]
	pub mod reset {
	    use crate::settlement::config::BuildingType;

		pub fn execute(ctx: Context<Components>, _args: ResetArgs) -> Result<Components> {
			let clock = Clock::get();
			let mut now = 0;
	
			if clock.is_ok() {
				now = clock.unwrap().unix_timestamp
			}
	
			ctx.accounts.settlement.buildings = vec![Building {
				x: 8,
				y: 8,
				deterioration: 0,
				id: BuildingType::TownHall,
				level: 1,
				turns_to_build: 0,
				extraction: get_extraction_cap(1),
			}];
	
			ctx.accounts.settlement.worker_assignment = vec![-1]; //one worker comes as default from town hall
			ctx.accounts.settlement.treasury = config::INITIAL_TREASURY;
			ctx.accounts.settlement.environment = config::INITIAL_ENVIRONMENT;
			ctx.accounts.settlement.time_units = config::INITIAL_TIME_UNITS;
			ctx.accounts.settlement.faith = config::INITIAL_FAITH;
			ctx.accounts.settlement.last_time_claim = now;
			ctx.accounts.settlement.research = 0;
			ctx.accounts.settlement.quest_claim_status = 0;
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct ResetArgs {}
	}	

	#[system]
	pub mod sacrifice {
    	use crate::systems::sacrifice::SacrificeError;
	
		pub fn execute(ctx: Context<Components>, args: SacrificeArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			if settlement.worker_assignment.len() <= args.index as usize {
				return err!(SacrificeError::LabourIndexOutOfRange);
			}
	
			if settlement.worker_assignment[args.index as usize] < -1 {
				return err!(SacrificeError::NotRestoredYet);
			}
	
			settlement.worker_assignment[args.index as usize] = config::BASE_DEATH_TIMEOUT
				+ (config::DEATH_TIMEOUT_RESEARCH_MULTIPLIER
					* get_research_level(settlement.research, ResearchType::DeathTimeout))
					as i8;
			settlement.faith += config::SACRIFICE_FAITH_BOOST;
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct SacrificeArgs {
			index: u8,
		}
	}	

	#[system]
	pub mod smart_object_init {
	    use crate::systems::smart_object_init::SmartObjectInitError;

		pub fn execute(ctx: Context<Components>, args: SmartObjectInitArgs) -> Result<Components> {
			let smart_object_location = &mut ctx.accounts.smart_object_location;
	
			//prevent init twice
			if smart_object_location.entity != Pubkey::default() {
				return err!(SmartObjectInitError::AlreadyInitialized);
			}
	
			smart_object_location.x = args.x;
			smart_object_location.y = args.y;
			smart_object_location.entity = Pubkey::new_from_array(args.entity);
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub smart_object_location: SmartObjectLocation,
		}
	
		#[arguments]
		struct SmartObjectInitArgs {
			pub x: i32,
			pub y: i32,
	
			pub entity: [u8; 32],
		}
	}

	#[system]
	pub mod upgrade {
    	use crate::systems::upgrade::UpgradeError;

		pub fn execute(ctx: Context<Components>, args: UpgradeArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			let index = args.index;
	
			if index >= settlement.buildings.len() {
				return err!(UpgradeError::BuildingIndexOutOfRange);
			}
	
			let building = settlement.buildings[index];
	
			if building.turns_to_build > 0 {
				return err!(UpgradeError::UnderConstruction);
			}
	
			if index > 0 && building.level >= settlement.buildings[0].level {
				return err!(UpgradeError::TownHallLevelReached);
			}
	
			let building_config = &BUILDINGS_CONFIG[building.id as usize];
	
			let build_cost = get_construction_cost(
				settlement.research,
				building_config.cost_tier,
				building.level + 1,
				1.0,
			);
	
			if settlement.treasury.wood < build_cost.wood
				|| settlement.treasury.water < build_cost.water
				|| settlement.treasury.food < build_cost.food
				|| settlement.treasury.stone < build_cost.stone
			{
				return err!(UpgradeError::NotEnoughResources);
			} else {
				settlement.treasury.wood -= build_cost.wood;
				settlement.treasury.water -= build_cost.water;
				settlement.treasury.food -= build_cost.food;
				settlement.treasury.stone -= build_cost.stone;
			}
	
			if args.worker_index >= 0 {
				if settlement.worker_assignment.len() as i16 <= args.worker_index {
					return err!(UpgradeError::SuppliedWorkerIndexOutOfBounds);
				}
				settlement.worker_assignment[args.worker_index as usize] = (index) as i8;
			}
	
			//all checks passed
			settlement.buildings[index].turns_to_build += get_build_time(
				settlement.research,
				building_config.build_time_tier,
				building.level,
			);
	
			Ok((ctx.accounts))
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct UpgradeArgs {
			index: usize,
			worker_index: i16,
		}
	}	

	#[system]
	pub mod wait {
    use crate::settlement::config::{get_collection_level_multiplier, get_storage_level_multiplier, ENVIRONMENT_MAX};

		pub fn execute(ctx: Context<Components>, args: WaitArgs) -> Result<Components> {
			let settlement = &mut ctx.accounts.settlement;
	
			let time_to_wait = u16::min(args.time, settlement.time_units as u16);
			settlement.time_units -= time_to_wait as u8;
	
			let mut water_storage: u16 = config::INITIAL_TREASURY.water;
			let mut food_storage: u16 = config::INITIAL_TREASURY.food;
			let mut wood_storage: u16 = config::INITIAL_TREASURY.wood;
			let mut stone_storage: u16 = config::INITIAL_TREASURY.stone;
	
			//calc current storage capacity for all resources
			for building in settlement.buildings.to_vec() {
				if building.turns_to_build > 0 && building.level == 0 {
					continue;
				}
	
				match building.id {
					BuildingType::TownHall => {
						water_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
						food_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
						wood_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
						stone_storage += config::TOWNHALL_STORAGE_PER_LEVEL * building.level as u16;
					}
					BuildingType::WaterStorage => {
						water_storage += config::WATER_STORAGE_PER_LEVEL
							* get_storage_level_multiplier(building.level);
					}
					BuildingType::FoodStorage => {
						food_storage += config::FOOD_STORAGE_PER_LEVEL
							* get_storage_level_multiplier(building.level);
					}
					BuildingType::WoodStorage => {
						wood_storage += config::WOOD_STORAGE_PER_LEVEL
							* get_storage_level_multiplier(building.level);
					}
					BuildingType::StoneStorage => {
						stone_storage += config::STONE_STORAGE_PER_LEVEL
							* get_storage_level_multiplier(building.level);
					}
					_ => {}
				}
			}
	
			let storage_research =
				get_research_level(settlement.research, ResearchType::StorageCapacity);
			let storage_multiplier =
				1.0 + config::STORAGE_CAPACITY_RESEARCH_MULTIPLIER * storage_research as f32;
			if storage_multiplier > 1.0 {
				water_storage = (water_storage as f32 * storage_multiplier).floor() as u16;
				food_storage = (food_storage as f32 * storage_multiplier).floor() as u16;
				wood_storage = (wood_storage as f32 * storage_multiplier).floor() as u16;
				stone_storage = (stone_storage as f32 * storage_multiplier).floor() as u16;
			}
	
			//wells generate water without worker assigned
			for building in settlement.buildings.to_vec() {
				if building.turns_to_build > 0 && building.level == 0 {
					continue;
				}
				match building.id {
					BuildingType::WaterCollector => {
						let mut collected = 0;
	
						if water_storage > settlement.treasury.water {
							collected = time_to_wait * get_collection_level_multiplier(building.level)
								+ get_research_level(
									settlement.research,
									ResearchType::ResourceCollectionSpeed,
								) as u16;
							collected = u16::min(collected, water_storage - settlement.treasury.water);
						}
	
						if collected > 0 {
							settlement.treasury.water += collected;
						}
					}
					_ => {}
				}
			}
	
			//process all buildings with allocated worker
			let mut alive_workers: u16 = 0;
			for worker_index in 0..settlement.worker_assignment.len() {
				let building_index = settlement.worker_assignment[worker_index];
	
				if building_index >= -1 {
					alive_workers += 1;
				}
	
				if building_index < 0 {
					//worker unallocated
					continue;
				}
	
				let building_index_usize = building_index as usize;
				let building = settlement.buildings[building_index_usize];
	
				let max_deterioration = config::BASE_DETERIORATION_CAP
					+ config::DETERIORATION_CAP_RESEARCH_MULTIPLIER
						* get_research_level(settlement.research, ResearchType::DeteriorationCap);
	
				if building.deterioration >= max_deterioration {
					//allocated building broken
					settlement.worker_assignment[worker_index] = -1;
					continue;
				}
	
				if building.turns_to_build > 0 {
					settlement.buildings[building_index_usize].turns_to_build -=
						u8::min(time_to_wait as u8, building.turns_to_build);
	
					if settlement.buildings[building_index_usize].turns_to_build <= 0 {
						settlement.buildings[building_index_usize].level += 1;
	
						settlement.buildings[building_index_usize].extraction +=
							get_extraction_cap(building.level);
						if matches!(building.id, BuildingType::TownHall) {
							settlement.worker_assignment.push(-1);
						}
	
						//finished building anything but food/wood collectors - release workers
						match settlement.buildings[building_index_usize].id {
							BuildingType::FoodCollector => {}
							BuildingType::WoodCollector => {}
							BuildingType::StoneCollector => {}
							_ => settlement.worker_assignment[worker_index] = -1,
						}
					} else {
						continue;
					}
				}
	
				let building_type = building.id;
	
				//TODO [CLEANUP] check if this code can be reused (across 3 different resources) (e.g. using array to store resources)
				match building_type {
					BuildingType::FoodCollector => {
						let mut collected = 0;
	
						if food_storage > settlement.treasury.food {
							collected = u16::min(
								time_to_wait * get_collection_level_multiplier(building.level)
									+ get_research_level(
										settlement.research,
										ResearchType::ResourceCollectionSpeed,
									) as u16,
								settlement.environment.food,
							);
							collected = u16::min(collected, food_storage - settlement.treasury.food);
						}
	
						if collected > 0 {
							settlement.environment.food -= collected;
							settlement.treasury.food += collected;
						}
					}
					BuildingType::WoodCollector => {
						let mut collected = 0;
	
						if wood_storage > settlement.treasury.wood {
							collected = u16::min(
								time_to_wait * get_collection_level_multiplier(building.level)
									+ get_research_level(
										settlement.research,
										ResearchType::ResourceCollectionSpeed,
									) as u16,
								settlement.environment.wood,
							);
							collected = u16::min(collected, wood_storage - settlement.treasury.wood);
						}
						if collected > 0 {
							settlement.environment.wood -= collected;
							settlement.treasury.wood += collected;
						}
					}
					BuildingType::StoneCollector => {
						let mut collected = 0;
	
						if stone_storage > settlement.treasury.stone {
							collected = u16::min(
								time_to_wait * get_collection_level_multiplier(building.level)
									+ get_research_level(
										settlement.research,
										ResearchType::ResourceCollectionSpeed,
									) as u16,
								building.extraction,
							);
							collected = u16::min(collected, stone_storage - settlement.treasury.stone);
						}
						if collected > 0 {
							settlement.buildings[building_index_usize].extraction -= collected;
							settlement.treasury.stone += collected;
						}
					}
	
					_ => {}
				}
			}
	
			let regeneration_rate = config::get_regeneration_rate(settlement.research);
	
			//regeneration sources in environment
			settlement.environment.food += u16::min(
				time_to_wait * regeneration_rate,
				ENVIRONMENT_MAX.food - settlement.environment.food,
			);
			settlement.environment.wood += u16::min(
				time_to_wait * regeneration_rate,
				ENVIRONMENT_MAX.wood - settlement.environment.wood,
			);
	
			//deteriorate buildings
			for building in &mut settlement.buildings {
				if building.turns_to_build == 0 && building.deterioration < u8::MAX {
					building.deterioration += time_to_wait as u8;
				}
			}
	
			if settlement.treasury.water < alive_workers || settlement.treasury.food < alive_workers {
				//kill one
				for i in 0..settlement.worker_assignment.len() {
					if (settlement.worker_assignment[i]) >= -1 {
						settlement.worker_assignment[i] = config::BASE_DEATH_TIMEOUT
							+ (config::DEATH_TIMEOUT_RESEARCH_MULTIPLIER
								* get_research_level(settlement.research, ResearchType::DeathTimeout))
								as i8;
	
						alive_workers -= 1;
						break;
					}
				}
			}
	
			let consumption_rate: u16 = (alive_workers
				- u16::min(
					alive_workers,
					get_research_level(settlement.research, ResearchType::Consumption) as u16,
				))
				* time_to_wait;
	
			settlement.treasury.water -= u16::min(settlement.treasury.water, consumption_rate);
			settlement.treasury.food -= u16::min(settlement.treasury.food, consumption_rate);
	
			let mut i: usize = 0;
	
			//restore sacrificed worker
			for building_index in settlement.worker_assignment.to_vec() {
				if building_index < -1 {
					settlement.worker_assignment[i] += time_to_wait as i8;
					if settlement.worker_assignment[i] > -1 {
						settlement.worker_assignment[i] = -1;
					}
				}
	
				i += 1;
			}
	
			//calc faith as a lerp to 'runway'
			let mut runway = 0;
			if alive_workers > 0 {
				runway = u16::min(settlement.treasury.food, settlement.treasury.water)
					/ alive_workers as u16;
			}
	
			msg!("runway {}", runway);
	
			if settlement.faith >= config::FAITH_TO_RUNWAY_LERP_PER_TURN
				&& runway < settlement.faith as u16
			{
				settlement.faith -= config::FAITH_TO_RUNWAY_LERP_PER_TURN;
			} else if settlement.faith < u8::MAX - config::FAITH_TO_RUNWAY_LERP_PER_TURN
				&& runway > settlement.faith as u16
			{
				settlement.faith += config::FAITH_TO_RUNWAY_LERP_PER_TURN;
			}
	
			Ok(ctx.accounts)
		}
	
		#[system_input]
		pub struct Components {
			pub settlement: Settlement,
		}
	
		#[arguments]
		struct WaitArgs {
			time: u16,
		}
	}	
}
