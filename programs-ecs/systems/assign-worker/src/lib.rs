mod errors;
use bolt_lang::*;

declare_id!("BExuAEwcKxKeqHSN8C1WetUAd6Tm71cZEiP8EBSrH55T");

#[system]
pub mod assign_worker {
    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, args: AssignLabourArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if settlement.buildings.len() <= args.building_index as usize {
            return err!(errors::AssignWorkerError::BuildingIndexOutOfRange);
        }

        if settlement.buildings[0].level <= args.worker_index {
            return err!(errors::AssignWorkerError::WorkerIndexOutOfRange);
        }

        if settlement.worker_assignment[args.worker_index as usize] < -1 {
            return err!(errors::AssignWorkerError::NotRestoredYet);
        }

        settlement.worker_assignment[args.worker_index as usize] = args.building_index;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct AssignLabourArgs {
        worker_index: u8,
        building_index: i8,
    }
}
