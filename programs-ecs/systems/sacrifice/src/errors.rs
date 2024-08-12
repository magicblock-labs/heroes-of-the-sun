use bolt_lang::error_code;

#[error_code]
pub enum SacrificeError {
    #[msg("Supplied Labour index out range")]
    LabourIndexOutOfRange,
    #[msg("Not Restored Yet")]
    NotRestoredYet,
}
