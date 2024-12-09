use bolt_lang::error_code;

#[error_code]
pub enum SmartObjectDeityInteractionError {
    #[msg("Smart Object On Cooldown")]
    OnCooldown,
}
