nuget restore
md artifacts\bin
MSBuild.exe AzureSiteReplicator\AzureSiteReplicator.csproj /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="..\artifacts";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release;SolutionDir="."
copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.dll" artifacts\bin
copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.Tracing.dll" artifacts\bin
copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Delegation.dll" artifacts\bin
nuget pack
