use bolt_lang::*;

#[error_code]
pub enum TokenLauncherInteractError {
    #[msg("Mint Address Mismatch")]
    MintAddressMismatch,

    #[msg("Not enough resources in the backpack")]
    NotEnoughBackpackResources,

    #[msg("Not enough hard currency")]
    NotEnoughHardCurrency,
}
