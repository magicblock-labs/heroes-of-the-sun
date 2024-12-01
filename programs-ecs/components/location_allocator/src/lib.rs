use bolt_lang::*;

declare_id!("J7q3dEg2KauPKkMamH9Q5FHhCoFYsSq9ramdutMpPTDc");

#[component]
#[derive(Default)]
pub struct LocationAllocator {
    pub current_x: i16,
    pub current_y: i16,
    pub direction: u8,
}
