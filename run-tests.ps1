# Builds the cheat DLL, then runs the unit test suite for the pure C# components
# (TranslationDatabase / TranslationValidator / RichTextPreserver / GameVersionInfo).
# The test project is intentionally NOT part of the solution: it references the built DLL.
$ErrorActionPreference = 'Stop'

Write-Host "== Building main project =="
dotnet build ".\r.e.p.o cheat.sln" -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "Main build failed" }

Write-Host "== Running tests =="
dotnet test ".\r.e.p.o cheat.Tests\r.e.p.o cheat.Tests.csproj" -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
