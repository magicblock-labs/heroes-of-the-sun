use bolt_lang::*;

declare_id!("2JDZnj8f2tTvQhyQtoPrFxcfGJvuunVt9aGG8rDnpkKU");
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
