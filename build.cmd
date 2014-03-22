nuget restore
MSBuild.exe AzureSiteReplicator\AzureSiteReplicator.csproj /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="..\artifacts";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release;SolutionDir="."
copy "%ProgramFiles%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.dll" artifacts\bin
copy "%ProgramFiles%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.Tracing.dll" artifacts\bin
copy "%ProgramFiles%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Delegation.dll" artifacts\bin
nuget pack
