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
}
