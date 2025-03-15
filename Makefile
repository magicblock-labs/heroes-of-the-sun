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
	-exec dotnet anchorgen -i target/idl/deity_bot.json -o app/Assets/Scripts/Bolt/DeityBot.cs  \;



extend:
	- exec solana program extend 7gBLDn72Cog7dBvN1LWfo6W36Q7vxcv7CqYAeHwfo3Y 50000
	- exec solana program extend 42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2 50000
	- exec solana program extend fkiWK1Wn6ouGcHb3icX4XGKynef5MpsTQ478ZMdgB1g 50000
	- exec solana program extend 4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf 50000
	- exec solana program extend 4XXA1mX5aN4Fd62FBgNxCU7FzKDYS3KSxFX3RdJYoWPj 50000
	- exec solana program extend GBzY8ujNDb1FNkJUXUUjKV5uZPqzi6AoKsPjsqFEHCeh 50000
	- exec solana program extend J7q3dEg2KauPKkMamH9Q5FHhCoFYsSq9ramdutMpPTDc 50000
	- exec solana program extend 5F9tMTcNhgjL3tWCaF5HwLkQP9z4XJ4nTXmbYeS8UXRW 50000
	- exec solana program extend 6o9i5V3EvT9oaokbcZa7G92DWHxcqJnjXmCp94xxhQhv 50000
	- exec solana program extend FDY4hyNT9yaV3oXowH7u4guB2gW3Aj8psvLnGwQ9BuT6 50000
	- exec solana program extend 5xPJt6GDcmGphNAs6qU3hWAvzLwXuSqhTco6RtoAR9aY 50000
	- exec solana program extend 3ZJ7mgXYhqQf7EsM8q5Ea5YJWA712TFyWGvrj9mRL2gP 50000
	- exec solana program extend 3VEXJoAZkYxDXigSWso8FnJY8z6C6inpPxU798vqc9um 50000
	- exec solana program extend 6JwZJNAtkciXVGenFSoa99VBNcxyb2W8mvzcMK1vTWKs 50000
	- exec solana program extend 5bKBE1HgusXC5jVVjpk4CvxUM8UGnVPQyvGt7cB6Jk7W 50000
	- exec solana program extend 9RfzWgEBYQAM64a46V3dGRPKYsVY8a7YvZszWPMxvBfk 50000
	- exec solana program extend 2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB 50000
	- exec solana program extend 64Uk4oF6mNyviUdK2xHXE3VMCtbCMDgRr1DMJk777DJZ 50000
	- exec solana program extend 5ewDDvpaTkYvoE7ZJJ9cDmZuqvGQt65hsZSJ9w73Fzr1 50000
	- exec solana program extend 4ZxRnucEWC62kVktmx27cz9d1PzWWNgiZLT5VWFLbfB2 50000
	- exec solana program extend 76wsz7SjNtvoFK8aUvojEyfjep5pMSaHQihGVxcjc1EA 50000
	- exec solana program extend 9F6qiZPUWN3bCnr5uVBwSmEDf8QcAFHNSVDH8L7AkZe4 50000
	- exec solana program extend BExuAEwcKxKeqHSN8C1WetUAd6Tm71cZEiP8EBSrH55T 50000
	- exec solana program extend 62f9zAUjCN5VFqWF43qSUrW6CvivqhsEjDvCHwQ1SjgR 50000

idl:
	-exec bolt idl init -f target/idl/assign_settlement.json 42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2 --provider.cluster d