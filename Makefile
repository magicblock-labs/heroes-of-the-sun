all:
	-exec bolt build
	-exec dotnet anchorgen -i target/idl/settlement.json -o app/Assets/Scripts/Bolt/Settlement.cs  \;
	-exec dotnet anchorgen -i target/idl/player.json -o app/Assets/Scripts/Bolt/Player.cs  \;
	-exec dotnet anchorgen -i target/idl/locationallocator.json -o app/Assets/Scripts/Bolt/LocationAllocator.cs  \;
	-exec dotnet anchorgen -i target/idl/hero.json -o app/Assets/Scripts/Bolt/Hero.cs  \;
	-exec dotnet anchorgen -i target/idl/lootdistribution.json -o app/Assets/Scripts/Bolt/LootDistribution.cs  \;
	-exec dotnet anchorgen -i target/idl/smartobjectlocation.json -o app/Assets/Scripts/Bolt/SmartObjectLocation.cs  \;
	-exec dotnet anchorgen -i target/idl/smartobjectdeity.json -o app/Assets/Scripts/Bolt/SmartObjectDeity.cs  \;
	-exec dotnet anchorgen -i target/idl/token_minter.json -o app/Assets/Scripts/Bolt/TokenMinter.cs  \;



