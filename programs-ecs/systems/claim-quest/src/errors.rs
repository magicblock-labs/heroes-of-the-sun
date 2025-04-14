use bolt_lang::error_code;

#[error_code]
pub enum QuestClaimError {
    #[msg("Invalid Index")]
    InvalidIndex,
    #[msg("Already Claimed")]
    AlreadyClaimed,
}
