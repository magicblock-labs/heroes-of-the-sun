mod errors;
use bolt_lang::*;

declare_id!("F7m12a5YbScFwNPrKXwg4ua6Z9e7R1ZqXvXigoUfFDMq");

#[system]
pub mod assign_labour {
    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, args: AssignLabourArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;

        if settlement.buildings.len() <= args.building_index as usize {
            return err!(errors::AssignWorkerError::BuildingIndexOutOfRange);
        }

        if settlement.buildings[0].level <= args.labour_index {
            return err!(errors::AssignWorkerError::WorkerIndexOutOfRange);
        }

        if settlement.worker_assignment[args.labour_index as usize] < -1 {
            return err!(errors::AssignWorkerError::NotRestoredYet);
        }

        settlement.worker_assignment[args.labour_index as usize] = args.building_index;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct AssignLabourArgs {
        labour_index: u8,
        building_index: i8,
    }
}
