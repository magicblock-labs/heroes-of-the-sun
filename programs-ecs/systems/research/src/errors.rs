use bolt_lang::error_code;

#[error_code]
pub enum ResearchError {
    #[msg("Supplied Research index out range")]
    ResearchIndexOutOfRange,
    #[msg("Already Maxed Out")]
    AlreadyMaxedOut,
    #[msg("Not Enough Resources")]
    NotEnoughResources,
}
