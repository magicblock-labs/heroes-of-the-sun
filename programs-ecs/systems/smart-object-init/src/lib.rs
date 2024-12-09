use bolt_lang::*;

declare_id!("64Uk4oF6mNyviUdK2xHXE3VMCtbCMDgRr1DMJk777DJZ");

#[system]
pub mod smart_object_init {

    use smart_object_registry::SmartObjectLocation;

    pub fn execute(ctx: Context<Components>, args: SmartObjectInitArgs) -> Result<Components> {
        let smart_object_location = &mut ctx.accounts.smart_object_location;

        //cant init twice?

        smart_object_location.x = args.x;
        smart_object_location.y = args.y;
        smart_object_location.entity = Pubkey::new_from_array(args.entity);
        Ok(ctx.accounts)
    }

    #[system_input]
    pub struct Components {
        pub smart_object_location: SmartObjectLocation,
    }

    #[arguments]
    struct SmartObjectInitArgs {
        pub x: i32,
        pub y: i32,
        
        pub entity: [u8; 32],
    }
}
