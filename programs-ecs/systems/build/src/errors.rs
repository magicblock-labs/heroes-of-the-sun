use bolt_lang::error_code;

#[error_code]
pub enum BuildError {
    #[msg("Supplied Building Overlaps With Existing One")]
    WontFit,
    #[msg("Supplied Building Outside Of Settlement Bounds")]
    OutOfBounds,
    #[msg("Not Enough Resources")]
    NotEnoughResources,
}
