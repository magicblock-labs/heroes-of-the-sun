use bolt_lang::error_code;

#[error_code]
pub enum AssignLabourError {
    #[msg("Supplied Labour index out range")]
    LabourIndexOutOfRange,
    #[msg("Supplied Building index out range")]
    BuildingIndexOutOfRange,
    #[msg("Not Restored Yet")]
    NotRestoredYet,
}
