use bolt_lang::*;

pub use crate::ecs_bundle::{Player, PlayerInit};

#[component_deserialize]
pub struct Location {
    pub x: i16,
    pub y: i16,
}

impl Default for Player {
    fn default() -> Self {
        Self::new(PlayerInit {
            settlements: vec![],
        })
    }
}
