#addin "Cake.FileHelpers"
#addin "nuget:http://nuget.oss-concept.ch/nuget/?package=Opten.Cake"

var target = Argument("target", "Default");

var dest = Directory("./artifacts");
var umb = dest + Directory("_umbraco");
string version = null;

// Cleanup

Task("Clean")
	.Does(() =>
{
	if (DirectoryExists(dest))
	{
		CleanDirectory(dest);
		DeleteDirectory(dest, recursive: true);
	}
});

// Versioning

Task("Version") 
	.IsDependentOn("Clean") 
	.Does(() =>
{
	if (DirectoryExists(dest) == false)
	{
		CreateDirectory(dest);
	}

	version = "0.1.0";

	PatchAssemblyInfo("../OP10.NewContentAtTopHandler/Properties/AssemblyInfo.cs", version);
	
	FileWriteText(dest + File("OP10.NewContentAtTopHandler.variables.txt"), "version=" + version);

});

// Building

Task("Restore-NuGet-Packages") 
	.IsDependentOn("Version") 
	.Does(() =>
{ 
	NuGetRestore("../OP10.NewContentAtTopHandler.sln", new NuGetRestoreSettings {
		NoCache = true
	}); 
});

Task("Build") 
	.IsDependentOn("Restore-NuGet-Packages") 
	.Does(() =>
{
	MSBuild("../src/OP10.NewContentAtTopHandler/OP10.NewContentAtTopHandler.csproj", settings =>
		settings.SetConfiguration("Release"));

	CreateDirectory(umb + Directory("bin"));
	CopyFileToDirectory(File("../src/OP10.NewContentAtTopHandler/bin/Release/OP10.NewContentAtTopHandler.dll"), umb + Directory("bin"));
	CopyFileToDirectory(File("package.xml"), umb);
	
	Information("Patch package.xml: {0}", ReplaceTextInFiles(
       umb + File("package.xml"),
       "$ASSEMBLY_VERSION$",
       version
	).Count());
});

Task("Pack")
	.IsDependentOn("Build")
	.Does(() =>
{
	// NuGet
	NuGetPack("./OP10.NewContentAtTopHandler.nuspec", new NuGetPackSettings {
		Version = version,
		BasePath = umb,
		OutputDirectory = dest
	});

	// Umbraco
	MSBuild("./UmbracoPackage.proj", settings =>
		settings.SetConfiguration("Release")
			    .WithTarget("Package")
				.WithProperty("BuildDir", MakeAbsolute(umb).FullPath.Replace("/", "\\"))
				.WithProperty("ArtifactsDir", dest));
});

// Deploying

Task("Deploy")
	.Does(() =>
{
	string packageId = "OP10.NewContentAtTopHandler";

	// Get the Version from the .txt file
	version = EnvironmentVariable("bamboo_inject_" + packageId.Replace(".", "_") + "_version");

	// Get the path to the package
	var package = File(packageId + "." + version + ".nupkg");             

	// Push the package
	NuGetPush(package, new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package",
		ApiKey = EnvironmentVariable("NUGET_API_KEY")
	});

	// Notifications
	Slack();
});

Task("Default")
	.IsDependentOn("Pack");

RunTarget(target);
