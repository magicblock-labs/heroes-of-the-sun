use bolt_lang::*;

pub use crate::ecs_bundle::{LootDistribution, LootDistributionInit};
use crate::settlement;

#[component_deserialize]
pub struct LootLocation {
    pub x: i32,
    pub y: i32,
    pub loot_type: i8,
}

impl Default for LootDistribution {
    fn default() -> Self {
        let mut loots: Vec<LootLocation> = Vec::new();

        for i in 0..20 {
            loots.push(get_loot_location(i));
        }

        Self::new(LootDistributionInit {
            index: loots.len() as i32 + 1,
            loots,
        })
    }
}

pub fn get_loot_location(i: i32) -> LootLocation {
    let range = 2 * settlement::config::CHUNK_SIZE as i32; //todo use settlement allocation?

    return LootLocation {
        x: ((17 * i) % (range * 2) - range),
        y: ((13 * i) % (range * 2) - range),
        loot_type: 1,
    };
}
