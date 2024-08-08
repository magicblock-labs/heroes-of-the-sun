all:
	find target/idl -type f -name "*.json" -exec sh -c 'make process_file FILE="{}"' \;


UC = $(shell echo "$1" | awk '{for (i=1;i<=NF;i++) $$i=toupper(substr($$i,1,1)) substr($$i,2)} 1')

process_file:
	-exec dotnet anchorgen -i $(FILE) -o app/Assets/Scripts/Program/$(call UC,$(basename $(notdir $(FILE)))).cs  \;
