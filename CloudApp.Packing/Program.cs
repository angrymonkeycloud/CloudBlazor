using AngryMonkey.CloudMate;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

IConfigurationRoot configuration = builder.Build();
string? apiKey = configuration["NuGetApiKey"];

await new CloudPack(new CloudPackConfig() { NugetApiKey = apiKey })
{
    MetadataProperies =
    [
        "PropertyGroup/Authors",
        "PropertyGroup/Company",
        "PropertyGroup/AssemblyVersion",
        "PropertyGroup/FileVersion",
        "PropertyGroup/PackageIcon"
    ],
    Projects =
    [
        new CloudPackProject("CloudApp"),
        new CloudPackProject("CloudApp.Maui"),
    ]
}.Pack();
