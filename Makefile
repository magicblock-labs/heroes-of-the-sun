all:
	-exec bolt build
	-exec dotnet anchorgen -i target/idl/settlement.json -o app/Assets/Scripts/Bolt/Settlement.cs  \;
	-exec dotnet anchorgen -i target/idl/player.json -o app/Assets/Scripts/Bolt/Player.cs  \;
	-exec dotnet anchorgen -i target/idl/locationallocator.json -o app/Assets/Scripts/Bolt/LocationAllocator.cs  \;
	-exec dotnet anchorgen -i target/idl/hero.json -o app/Assets/Scripts/Bolt/Hero.cs  \;
	-exec dotnet anchorgen -i target/idl/loot_distribution.json -o app/Assets/Scripts/Bolt/LootDistribution.cs  \;
	-exec dotnet anchorgen -i target/idl/smart_object_location.json -o app/Assets/Scripts/Bolt/SmartObjectLocation.cs  \;
	-exec dotnet anchorgen -i target/idl/smart_object_deity.json -o app/Assets/Scripts/Bolt/SmartObjectDeity.cs  \;
	-exec dotnet anchorgen -i target/idl/token_minter.json -o app/Assets/Scripts/Bolt/TokenMinter.cs  \;
	-exec dotnet anchorgen -i target/idl/deity_bot.json -o app/Assets/Scripts/Bolt/DeityBot.cs  \;
	-exec dotnet anchorgen -i target/idl/smartobjecttokenlauncher.json -o app/Assets/Scripts/Bolt/SmartObjectTokenLauncher.cs  \;



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
	- exec solana program extend C2H1sb7ZVpgEZFWqXujRK3rx5C2543GNN251wmgfbhUH 50000
	- exec solana program extend 8va4yKEBACkT49C9wo94gS8ZaTdUrq2ipLgZvSNxWbd3 50000
	- exec solana program extend AdrPpoYr67ZcDZsQxsPgeosE3sQbZxercbUn8i1dcvap 50000
	- exec solana program extend DUW1KczxcpeTEY7j9nkvcuAdWGNWoadTeDBKN5Z9xhst 50000

	

idl_local:
	-exec bolt idl init -f target/idl/assign_settlement.json 42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2 --provider.cluster l
	-exec bolt idl init -f target/idl/smartobjecttokenlauncher.json 8va4yKEBACkT49C9wo94gS8ZaTdUrq2ipLgZvSNxWbd3 --provider.cluster l


idl:
	-exec bolt idl init -f target/idl/wait.json 9F6qiZPUWN3bCnr5uVBwSmEDf8QcAFHNSVDH8L7AkZe4 --provider.cluster d
	-exec bolt idl init -f target/idl/move_hero.json 6o9i5V3EvT9oaokbcZa7G92DWHxcqJnjXmCp94xxhQhv --provider.cluster d
	-exec bolt idl init -f target/idl/smartobjecttokenlauncher.json 8va4yKEBACkT49C9wo94gS8ZaTdUrq2ipLgZvSNxWbd3 --provider.cluster d

approve:
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 3ZJ7mgXYhqQf7EsM8q5Ea5YJWA712TFyWGvrj9mRL2gP --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 7nsn4z8U1nVCVHud9CLmYLy5ZHK2bSMge6u7YgmssdaA --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 9F6qiZPUWN3bCnr5uVBwSmEDf8QcAFHNSVDH8L7AkZe4 --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL Csna3V2jUMdQEQKUCxLsQEnYThAGPSWcPCxW9vea1S8d --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 76wsz7SjNtvoFK8aUvojEyfjep5pMSaHQihGVxcjc1EA --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 97qK4zBtZbSGT1mSw5mn12hfHgz4jV4C7cLmwSzH2eua --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 64Uk4oF6mNyviUdK2xHXE3VMCtbCMDgRr1DMJk777DJZ --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 2QPK685TLL7jUG4RYuWXZjv3gw88kUPYw7Aye63cTTjB --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL C2H1sb7ZVpgEZFWqXujRK3rx5C2543GNN251wmgfbhUH --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 6JwZJNAtkciXVGenFSoa99VBNcxyb2W8mvzcMK1vTWKs --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 3VEXJoAZkYxDXigSWso8FnJY8z6C6inpPxU798vqc9um --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 6o9i5V3EvT9oaokbcZa7G92DWHxcqJnjXmCp94xxhQhv --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2 --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL BExuAEwcKxKeqHSN8C1WetUAd6Tm71cZEiP8EBSrH55T --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 4XXA1mX5aN4Fd62FBgNxCU7FzKDYS3KSxFX3RdJYoWPj --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 7gBLDn72Cog7dBvN1LWfo6W36Q7vxcv7CqYAeHwfo3Y --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL fkiWK1Wn6ouGcHb3icX4XGKynef5MpsTQ478ZMdgB1g --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL DUW1KczxcpeTEY7j9nkvcuAdWGNWoadTeDBKN5Z9xhst --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL 5xPJt6GDcmGphNAs6qU3hWAvzLwXuSqhTco6RtoAR9aY --provider.cluster d
	-exec bolt approve-system H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL AdrPpoYr67ZcDZsQxsPgeosE3sQbZxercbUn8i1dcvap --provider.cluster d