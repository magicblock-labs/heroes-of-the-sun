use bolt_lang::*;

declare_id!("DvznnhhpuH3WkBsUonUytkHhd6MYz91c2iLvRuvLeSnV");

#[component]
#[derive(Default)]
pub struct LocationAllocator {
    pub current_x: i16,
    pub current_y: i16,
    pub direction: u8,
}
