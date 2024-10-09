use bolt_lang::error_code;

#[error_code]
pub enum UpgradeError {
    #[msg("Supplied Building index out range")]
    BuildingIndexOutOfRange,
    #[msg("Not Enough Resources")]
    NotEnoughResources,
    #[msg("Can't upgrade beyond townhall level")]
    TownHallLevelReached,
}
