use bolt_lang::error_code;

#[error_code]
pub enum ClaimLootError {
    #[msg("Hero and settlement owners don't match")]
    OwnersMismatch,
    #[msg("Hero not at loot location")]
    LocationMismatch,
}
