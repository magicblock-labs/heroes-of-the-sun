use bolt_lang::error_code;

#[error_code]
pub enum AssignWorkerError {
    #[msg("Supplied Labour index out range")]
    WorkerIndexOutOfRange,
    #[msg("Supplied Building index out range")]
    BuildingIndexOutOfRange,
    #[msg("Not Restored Yet")]
    NotRestoredYet,
}
