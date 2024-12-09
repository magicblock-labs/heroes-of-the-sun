use bolt_lang::*;

declare_id!("5ewDDvpaTkYvoE7ZJJ9cDmZuqvGQt65hsZSJ9w73Fzr1");

#[component(delegate)]
#[derive(Default)]
pub struct SmartObjectLocation {
    pub x: i32,
    pub y: i32,
    pub entity: Pubkey,
}
