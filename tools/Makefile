# Below is a workaround for .NET SDK 7.0 trying to allocate large amounts of memory for GC work:
# https://github.com/dotnet/runtime/issues/85556#issuecomment-1529177092
DOTNET_PAGE_SIZE = $(shell getconf PAGESIZE)
DOTNET_AVPHYS_PAGES = $(shell getconf _AVPHYS_PAGES)
DOTNET_HEAP_LIMIT = $(shell printf '%X\n' $$(($(DOTNET_AVPHYS_PAGES) * $(DOTNET_PAGE_SIZE))))

invoke-build:
	chmod +x ./build.sh
	./build.sh $(ENVIRONMENT) || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./build.sh $(ENVIRONMENT))

invoke-build-ci:
	chmod +x ./build.sh
	./build.sh $(ENVIRONMENT) -p:ContinuousIntegrationBuild=true || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./build.sh $(ENVIRONMENT) -p:ContinuousIntegrationBuild=true)

invoke-doc-build:
	chmod +x ./docgen.sh
	./docgen.sh || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./docgen.sh)
