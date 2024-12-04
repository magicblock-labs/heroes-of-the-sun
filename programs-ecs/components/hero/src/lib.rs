use bolt_lang::*;
use settlement::ResourceBalance;

declare_id!("GBzY8ujNDb1FNkJUXUUjKV5uZPqzi6AoKsPjsqFEHCeh");

#[component(delegate)]
#[derive(Default)]
pub struct Hero {
    pub x: i32,
    pub y: i32,
    pub last_activity: i64,
    pub owner: Pubkey,
    pub backpack: ResourceBalance,
}
