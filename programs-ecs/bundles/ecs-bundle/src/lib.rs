use bolt_lang::*;

pub mod settlement;
pub mod hero;
pub mod player;

declare_id!("Cjca6tWWGx77ki6rRinErcoZJEvJHNxfPaut8DoKJHQ5");

#[bundle]
pub mod ecs_bundle {
	use crate::settlement::{Building, EnvironmentState, ResourceBalance};
	use crate::player::Location;
	
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
