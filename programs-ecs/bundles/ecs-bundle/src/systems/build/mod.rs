use bolt_lang::*;

use crate::settlement::{config::{BuildingConfig, BUILDINGS_CONFIG}, Settlement};

#[error_code]
pub enum BuildError {
    #[msg("Supplied Building Overlaps With Existing One")]
    WontFit,
    #[msg("Supplied Building Outside Of Settlement Bounds")]
    OutOfBounds,
    #[msg("Not Enough Resources")]
    NotEnoughResources,
    #[msg("Config Index Out Of Bounds")]
    ConfigIndexOutOfRange,
    #[msg("Worker Index Out Of Bounds")]
    SuppliedWorkerIndexOutOfBounds,
}

//move to settlement trait?
pub fn fits(settlement: &mut Account<Settlement>, x: u8, y: u8, new_config: &BuildingConfig) -> bool {
    for existing_building in &settlement.buildings {
        let existing_config = &BUILDINGS_CONFIG[existing_building.id as usize];

        if x < existing_building.x + existing_config.width
            && existing_building.x < x + new_config.width
            && y < existing_building.y + existing_config.height
            && existing_building.y < y + new_config.height
        {
            msg!("collided!");
            msg!(
                "new: x {}, y {}, w {}, h {}",
                x,
                y,
                new_config.width,
                new_config.height
            );

            msg!(
                "existing: x {}, y {}, w {}, h {}",
                existing_building.x,
                existing_building.y,
                existing_config.width,
                existing_config.height
            );

            return false;
        }
    }

    return true;
}
