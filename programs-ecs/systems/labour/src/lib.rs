use bolt_lang::*;

declare_id!("BEc67x2mycQPPeWDLB8r2LCV4TSZCHTfp7rjpjwFwUhH");

#[system]
pub mod labour {
    use settlement::Settlement;

    pub fn execute(ctx: Context<Components>, args: LabourArgs) -> Result<Components> {
        let settlement = &mut ctx.accounts.settlement;
        settlement.labour_allocation[args.labour as usize] = args.building;
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub settlement: Settlement,
    }

    #[arguments]
    struct LabourArgs {
        labour: u8,
        building: i8,
    }
}
