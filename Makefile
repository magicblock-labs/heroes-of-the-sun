all:
	-exec dotnet anchorgen -i target/idl/settlement.json -o app/Assets/Scripts/Bolt/Settlement.cs  \;



