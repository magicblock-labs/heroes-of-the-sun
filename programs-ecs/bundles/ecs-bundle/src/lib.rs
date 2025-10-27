use bolt_lang::*;

pub mod settlement;
pub mod hero;
pub mod player;
pub mod location_allocator;
pub mod loot_distribution;
pub mod smart_object_deity;
pub mod smart_object_location;
pub mod smart_object_token_launcher;

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

}
