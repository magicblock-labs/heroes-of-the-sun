use bolt_lang::error_code;

#[error_code]
pub enum BackpackError {
    #[msg("Hero owner doesn't match settlement owner")]
    OwnerMismatch,
    #[msg("Not enough resources in the backpack")]
    NotEnoughBackpackResources,
    #[msg("Not enough resources in the settlement")]
    NotEnoughSettlementResources,
    #[msg("Not enough backpack capacity")]
    NotEnoughBackpackCapacity,
}
