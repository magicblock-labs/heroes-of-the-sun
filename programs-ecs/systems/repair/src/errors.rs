use bolt_lang::error_code;

#[error_code]
pub enum RepairError {
    #[msg("Supplied Building index out range")]
    BuildingIndexOutOfRange,
    #[msg("Not Enough Resources")]
    NotEnoughResources,
}
