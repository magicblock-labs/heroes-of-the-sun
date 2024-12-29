use bolt_lang::error_code;

#[error_code]
pub enum ExchangeError {
    #[msg("No Exchange")]
    NoExchange,
    #[msg("Not Enough Tokens")]
    NotEnoughTokens,
    #[msg("Token Burn Failed")]
    TokenBurnFailed,
}
