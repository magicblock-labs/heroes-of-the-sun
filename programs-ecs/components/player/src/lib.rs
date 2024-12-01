use bolt_lang::*;

declare_id!("FDY4hyNT9yaV3oXowH7u4guB2gW3Aj8psvLnGwQ9BuT6");
#[component_deserialize]
pub struct Location {
    pub x: i16,
    pub y: i16,
}
#[component(delegate)]
pub struct Player {
    #[max_len(5)]
    pub settlements: Vec<Location>,
}

impl Default for Player {
    fn default() -> Self {
        Self::new(PlayerInit {
            settlements: vec![],
        })
    }
}
