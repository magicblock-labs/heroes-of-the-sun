use bolt_lang::*;

declare_id!("5F9tMTcNhgjL3tWCaF5HwLkQP9z4XJ4nTXmbYeS8UXRW");

#[component_deserialize]
pub struct LootLocation {
    pub x: i32,
    pub y: i32,
    pub loot_type: i8,
}
#[component(delegate)]
pub struct LootDistribution {
    pub index: i32,

    #[max_len(100)]
    pub loots: Vec<LootLocation>,
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
