# Ai.Orchestrator.Plugins
Plugins created to be used with Ai Orchestrator

# ICommand

# Config files

# CsProj changes
1. The .csproj file needs to be updated to allow for dynamic loading to do so edit the .csproj file of the project and add the following:

```
<PropertyGroup>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
</PropertyGroup>
```

2. Update the property group with the dotnet version and add EnableDynamicLoading, like below:
```
<PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableDynamicLoading>true</EnableDynamicLoading>
</PropertyGroup>
```

3. Update references. Add Private = false and ExcludeAssets = runtime to Ai.Orchestrator.Models reference. 

*NOTE: Ai.Orchestrator.Common does not work for this yet*
```
<ItemGroup>
  <Reference Include="Ai.Orchestrator.Common">
    <HintPath>..\..\ai.orchestrator\Ai.Orchestrator\bin\Debug\net7.0\Plugins\Ai.Orchestrator.Common.dll</HintPath>
  </Reference>
  <Reference Include="Ai.Orchestrator.Models">
    <HintPath>..\..\ai.orchestrator\Ai.Orchestrator\bin\Debug\net7.0\Ai.Orchestrator.Models.dll</HintPath>
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
  </Reference>
</ItemGroup>
```